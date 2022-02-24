using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletControl : NetworkBehaviour
{
    [SerializeField] private float Speed = 1;
    [SerializeField] private float MaxLife = 5;

    float life;

    void Start()
    {
        if(IsServer)
        {
            life = MaxLife;
        }
    }

    
    void Update()
    {
        if(IsServer)
        {
            transform.position += transform.forward * Speed;
            life -= Time.deltaTime;
            if(life <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
