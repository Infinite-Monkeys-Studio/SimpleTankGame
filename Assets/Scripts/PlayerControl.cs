using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerControl : NetworkBehaviour
{
    [SerializeField] private float maxVelocity = 2;
    [SerializeField] private float moveForce = 1;
    [SerializeField] private float wallForce = 10;
    [SerializeField] private float dragForce = 0.2f;
    [SerializeField] private float collisionDistance = 5;
    [SerializeField] private float fireRate = 1;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Transform TurretTransform;
    [SerializeField] private Vector3 TurretOffset = new Vector3(-90, 90, 0);
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bullet;
    [SerializeField] private ParticleSystem muzzleFlash;

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    NetworkVariable<Vector3> Velocity = new NetworkVariable<Vector3>();
    
    NetworkVariable<float> Rotation = new NetworkVariable<float>();    
    
    NetworkVariable<int> Health = new NetworkVariable<int>();
    NetworkVariable<bool> CanShoot = new NetworkVariable<bool>();

    private float cachedRotation = 0;
    private float lastShotTime;
    private Vector3 wallNormal = new Vector3();
    private float wallSeparation = 0;

    void Start()
    {
        if (IsServer)
        {
            lastShotTime = Time.time;
            CanShoot.Value = true;
            Health.Value = 2;
            healthBar.setHealth(Health.Value);
        }
    }

    void Update()
    {
        if(IsServer)
        {
            UpdateServer();
        }

        if(IsClient && IsOwner)
        {
            UpdateClient();
        }
    }

    void UpdateClient()
    {
        //do rotation
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Vector3 TargetDirection = hit.point - transform.position;
            float rads = Mathf.Atan2(TargetDirection.x, TargetDirection.z);
            cachedRotation = rads * Mathf.Rad2Deg;
        }

        if (cachedRotation != Rotation.Value)
        {
            UpdateClientRotationServerRpc(cachedRotation);
        }

        //do movement
        float inputX = Input.GetAxis("Vertical");
        float inputY = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(inputX, 0, inputY);
        movement = movement.normalized * moveForce;
        UpdateClientPositionServerRpc(Position.Value + movement);

        //do shooting
        if (Input.GetButtonDown("Fire") && CanShoot.Value)
        {
            ClientShootServerRpc();
            muzzleFlash.Play();
        }
    }

    [ServerRpc]
    public void ClientShootServerRpc()
    {
        if(CanShoot.Value)
        {
            CanShoot.Value = false;
            lastShotTime = Time.time;
            var bulletInstance = Instantiate(bullet, muzzle.position, muzzle.rotation);
            bulletInstance.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void HandleShotByBullet()
    {
        if(IsServer)
        {
            Health.Value -= 1;
            Debug.Log($"Health {Health.Value}");
            if (Health.Value <= 0)
            {
                //do death
                Debug.Log("death");
                //update score
                int myTeamIndex = gameObject.GetComponent<TeamPlayer>().getTeamIndex();
                ScoreKeeper.Singleton.TeamScored(myTeamIndex);

                //respawn
                Vector3 newSpawn = GetRandomPositionOnPlane();
                Position.Value = newSpawn;
                transform.position = newSpawn;
                Health.Value = 2;
            }
        }
    }

    void UpdateServer()
    {
        //do shooting
        if(Time.time - (1/fireRate) > lastShotTime)
        {
            CanShoot.Value = true;
        }
        TurretTransform.rotation = Quaternion.Euler(TurretOffset) * Quaternion.AngleAxis(Rotation.Value, Vector3.forward);

        
        //do movement and collision
        Vector3 requestedMove = Position.Value - transform.position;
        
        if(wallSeparation > 0)
        {
            var dot = wallNormal * Vector3.Dot(requestedMove, wallNormal);
            requestedMove -= dot;
            transform.position += requestedMove;
            transform.position += wallNormal * wallSeparation;
            Position.Value = transform.position;
            wallSeparation = 0;
        } else
        {
            transform.position += Velocity.Value * Time.deltaTime;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        int best = 0;

        for (int i = 1; i < collision.contactCount; i++)
        {
            if (collision.GetContact(best).separation < collision.GetContact(i).separation) best = i;
        }

        var contact = collision.GetContact(best);

        var correction = contact.normal * -contact.separation;
        wallNormal = wallNormal * wallSeparation + correction;
        wallSeparation = wallNormal.magnitude;
        wallNormal.Normalize();
    }

    [ServerRpc]
    void UpdateClientPositionServerRpc(Vector3 newClientPosition)
    {
        Position.Value = newClientPosition;
    }

    [ServerRpc]
    void SetNewMovementForce(Vector3 newMovementForce)
    {
        Velocity.Value += newMovementForce;
        Vector3.ClampMagnitude(Velocity.Value, maxVelocity);
    }

    [ServerRpc]
    void UpdateClientRotationServerRpc(float newRotation)
    {
        Rotation.Value = newRotation;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
    }

    private void updateHealth(int previousValue, int newValue)
    {
        healthBar.setHealth(Health.Value);
    }

    private void OnEnable()
    {
        Health.OnValueChanged += updateHealth;
    }

    private void OnDisable()
    {
        Health.OnValueChanged -= updateHealth;
    }
}
