using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletControl : NetworkBehaviour
{
    [SerializeField] private float Speed = 1;
    [SerializeField] private float MaxLife = 5;
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private MeshRenderer shell;

    float life;
    private NetworkVariable<bool> alive = new NetworkVariable<bool>(true);

    void Start()
    {
        if(IsServer)
        {
            life = MaxLife;
            alive.Value = true;
        }
    }

    
    void Update()
    {
        if(IsServer)
        {   
            if(alive.Value)
            {
                life -= Time.deltaTime;
                transform.position += transform.forward * Speed;
                if (life <= 0) alive.Value = false;
            } else
            {
                doDeath();
            }
        } else
        {
            if (!alive.Value)
            {
                doDeath();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            alive.Value = false;
            var player = other.GetComponentInParent<PlayerControl>();
            if (player != null)
            {
                player.HandleShotByBullet();
            }
        }
            
    }

    private void doDeath()
    {
        if (!explosion.isPlaying)
        {
            explosion.Play();
            shell.enabled = false;
        }

        if(IsServer)
        {
            if (!explosion.IsAlive())
            {
                Destroy(gameObject);
            }
        }
        
    }
}
