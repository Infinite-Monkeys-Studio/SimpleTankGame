using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerControl : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 1;
    [SerializeField]
    private Transform TurretTransform;
    [SerializeField]
    private Vector3 TurretOffset = new Vector3(-90, 90, 0);
    [SerializeField]
    private Transform muzzle;
    [SerializeField]
    private GameObject bullet;

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    NetworkVariable<float> Rotation = new NetworkVariable<float>();

    private Vector3 cachedPosition = new Vector3();
    private float cachedRotation = 0;

    void Start()
    {
        transform.position = GetRandomPositionOnPlane();
    }

    public override void OnNetworkSpawn()
    {
        
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

        cachedPosition += movement;

        if(cachedPosition != Position.Value)
        {
            UpdateClientPositionServerRpc(cachedPosition);
        }

        //do shooting
        if(Input.GetButtonDown("Fire"))
        {
            ClientShootServerRpc();
        }
    }

    [ServerRpc]
    private void ClientShootServerRpc()
    {
        var bulletInstance = Instantiate(bullet, muzzle.position, muzzle.rotation);
        bulletInstance.GetComponent<NetworkObject>().Spawn();
    }

    void UpdateServer()
    {
        transform.position = Position.Value;
        TurretTransform.rotation = Quaternion.Euler(TurretOffset) * Quaternion.AngleAxis(Rotation.Value, Vector3.forward);
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
}
