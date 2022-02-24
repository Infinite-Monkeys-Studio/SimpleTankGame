using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerControl : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float collisionDistance = 5;
    [SerializeField] private float fireRate = 1;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Transform TurretTransform;
    [SerializeField] private Vector3 TurretOffset = new Vector3(-90, 90, 0);
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bullet;
    [SerializeField] private ParticleSystem muzzleFlash;

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    NetworkVariable<float> Rotation = new NetworkVariable<float>();
    NetworkVariable<int> Health = new NetworkVariable<int>();
    NetworkVariable<bool> CanShoot = new NetworkVariable<bool>();

    private float cachedRotation = 0;
    private float lastShotTime;

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
        float inputX = Input.GetAxis("Vertical") * moveSpeed;
        float inputY = Input.GetAxis("Horizontal") * moveSpeed;

        Vector3 movement = new Vector3(inputX, 0, inputY);
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
            }
        }
    }

    void UpdateServer()
    {
        if(Time.time - (1/fireRate) > lastShotTime)
        {
            CanShoot.Value = true;
        }
        TurretTransform.rotation = Quaternion.Euler(TurretOffset) * Quaternion.AngleAxis(Rotation.Value, Vector3.forward);

        
        Vector3 requestedMove = Position.Value - transform.position;
        Ray ray = new Ray(transform.position, requestedMove);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            if(requestedMove.magnitude > Vector3.Distance(transform.position, hit.point) - collisionDistance)
            {
                Debug.Log("too close");
                Position.Value = transform.position;
            } else
            {
                transform.position += requestedMove;
            }
        }
    }

    [ServerRpc]
    void UpdateClientPositionServerRpc(Vector3 newClientPosition)
    {
        Position.Value = newClientPosition;
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
