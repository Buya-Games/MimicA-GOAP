using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IThrowable
{
    protected GameManager manager;
    protected Rigidbody rb;
    float gravity;
    [SerializeField] float height;
    Spawner.EnvironmentType myType;
    protected bool notStationary;
    Spawner spawner;
    Vector3 origScale;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        spawner = FindObjectOfType<Spawner>();
        gravity = Physics.gravity.y;
        origScale = transform.localScale;
        SetType();
    }

    void SetType(){
        if (gameObject.layer == 7){
            myType = Spawner.EnvironmentType.Berry;
        }
        if (gameObject.layer == 9){
            myType = Spawner.EnvironmentType.Fungus;
        }
        if (gameObject.layer == 10){
            myType = Spawner.EnvironmentType.BerryPoop;
        }
        if (gameObject.layer == 16){
            myType = Spawner.EnvironmentType.FungusPoop;
        }
    }

    public GameObject ThisGameObject(){
        return this.gameObject;
    }

    public void FlyingOrPickedUp(){
        notStationary = true;
        spawner.ThrowOrPickUpObject(gameObject,myType);
    }

    public void PickUp(Creature agent){
        notStationary = true;
        Vector3 pos = agent.visibleMesh.position;
        pos.y += 3;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        rb.isKinematic = true;

        transform.SetParent(agent.visibleMesh);
        Vector3 newScale = transform.localScale;//just adjusting scale cuz player/buddy scale isn't uniform
        newScale.x/=agent.transform.localScale.x;
        newScale.z/=agent.transform.localScale.z;
        transform.localScale = newScale;
        agent.PickUp(this);
    }

    public void Drop(){
        notStationary = false;
        spawner.ThrowOrPickUpObject(gameObject,myType,true);
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;

        transform.parent = null;
        transform.localScale = origScale;
    }

    public void ThrowObject(Vector3 where, float throwStrength, bool forFeeding = false){
        if (!notStationary){
            FlyingOrPickedUp();
        }
        // if (forFeeding){
        //     feeding = true;
        // }
        Vector3 higherPos = transform.position;
        higherPos.y = 5;
        transform.position = higherPos;
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;

        transform.parent = null;
        transform.localScale = origScale;
        //if (!feeding){
            higherPos.y = 5;
            rb.velocity = CalculateTraj(where, throwStrength);
        //}
    }

    Vector3 CalculateTraj(Vector3 where, float throwStrength){
        height = (transform.position - where).sqrMagnitude/10; 
        float disY = where.y - transform.position.y;
        Vector3 disXZ = new Vector3(where.x - transform.position.x, 0, where.z - transform.position.z);
        float x = (height / disXZ.magnitude) * 2;

        Vector3 velY = Vector3.up * Mathf.Sqrt(-2 * gravity * x);
        Vector3 velXZ = disXZ / (Mathf.Sqrt(-2*x/gravity) + Mathf.Sqrt(2*(disY-x)/gravity));
        velXZ *= throwStrength;
        return velXZ + velY;
    }

    protected virtual void OnCollisionEnter(Collision col){
        if (notStationary && col.gameObject.layer == 11){
            if (myType == Spawner.EnvironmentType.BerryPoop || myType == Spawner.EnvironmentType.FungusPoop){
                BombBoom();
            }
            notStationary = false;
            spawner.ThrowOrPickUpObject(gameObject,myType,true);
        }
        if (notStationary && col.gameObject.layer == 4){
            notStationary = false;
            spawner.ThrowOrPickUpObject(gameObject,myType,true);

            Vector3 spawnPoint = transform.position;
            spawnPoint.y = 0f;
            if (myType == Spawner.EnvironmentType.BerryPoop){
                manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Bush);
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.BerryPoop);
                manager.particles.BombExplosion(transform.position);
            }
            if (myType == Spawner.EnvironmentType.FungusPoop){
                manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Mushroom);
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.FungusPoop);
                manager.particles.BombExplosion(transform.position);
            }
        }
        if (col.gameObject.layer == 13){ //if player
            Player player = col.gameObject.GetComponent<Player>();
            if (player.HeldItem == null){
                if (!notStationary){
                    FlyingOrPickedUp();
                }
                PickUp(player);
            } else {
                //dont know why but sometimes item gets stuck
                //so check if its still one of the children
                Item itemCheck = player.GetComponentInChildren<Item>();
                if (itemCheck == null){ //if not, then player has no item
                    Debug.Log("faker");
                    player.HeldItem = null;
                    if (!notStationary){
                        FlyingOrPickedUp();
                    }
                    PickUp(player);
                }
            }
        }
    }

    void BombBoom(){
        manager.particles.BombExplosion(transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position,3f);
        bool enemyHit = false;
        foreach (Collider nearbyObject in colliders){
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null && nearbyObject != gameObject){
                rb.AddExplosionForce(2000f,transform.position,3f);
                float dist = ((transform.position - nearbyObject.transform.position).magnitude) * 2;
                dist = 1 / dist; ///perfect will be like .5, so work from there to calculate the damage?
                Enemy enemy = nearbyObject.GetComponent<Enemy>();
                if (enemy != null){
                    enemy.TakeHit(gameObject,100);
                    enemyHit = true;
                }
            }
        }
        // if (!enemyHit){
        //     Vector3 spawnPoint = transform.position;
        //     spawnPoint.y = .25f;
        //     if (myType == Spawner.EnvironmentType.BerryPoop){
        //         manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Bush);
        //     }
        //     if (myType == Spawner.EnvironmentType.FungusPoop){
        //         manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Mushroom);
        //     }
        // }
        if (enemyHit){
            if (myType == Spawner.EnvironmentType.BerryPoop){
            manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.BerryPoop);
            }
            if (myType == Spawner.EnvironmentType.FungusPoop){
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.FungusPoop);
            }
        }
    }
}
