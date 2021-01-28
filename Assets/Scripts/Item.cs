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
    protected bool flying;
    Spawner spawner;
    Vector3 origScale;
    bool feeding = false;

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
            myType = Spawner.EnvironmentType.Bomb;
        }
    }

    public GameObject ThisGameObject(){
        return this.gameObject;
    }

    public void FlyingOrPickedUp(){
        flying = true;
        spawner.RemoveFlyingObject(gameObject,myType);
    }

    public void PickUp(Transform agent){
        Vector3 pos = agent.position;
        pos.y += 3;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        rb.isKinematic = true;

        transform.SetParent(agent);
        Vector3 newScale = transform.localScale;
        newScale.x/=agent.transform.localScale.x;
        //newScale.y/=agent.transform.localScale.y;
        newScale.z/=agent.transform.localScale.z;
        transform.localScale = newScale;
    }

    public void ThrowObject(Vector3 where, float throwStrength, bool forFeeding = false){
        if (!flying){
            FlyingOrPickedUp();
        }
        if (forFeeding){
            feeding = true;
        }
        Vector3 higherPos = transform.position;
        higherPos.y = 5;
        transform.position = higherPos;
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;

        transform.parent = null;
        transform.localScale = origScale;
        // if (myType == Spawner.EnvironmentType.Bomb){
        //     transform.localScale = Vector3.one;
        // } else {
        //     transform.localScale = Vector3.one * 0.5f;
        // }
        if (!feeding){
            rb.velocity = CalculateTraj(where, throwStrength);
        }
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
        if (flying){
            if (myType == Spawner.EnvironmentType.Bomb){
                BombBoom();
            }
            flying = false;
            spawner.RemoveFlyingObject(gameObject,myType,true);
        }
        if (feeding){
            manager.particles.EatingBerry(transform.position);
            manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.Berry);
            feeding = false;
            if (col.gameObject.layer == 12){
                col.gameObject.GetComponent<Creature>().Eat();
            }
        }
        if (col.gameObject.layer == 13){ //if player
            Player player = col.gameObject.GetComponent<Player>();
            if (player.HeldItem == null){
                if (!flying){
                    FlyingOrPickedUp();
                }
                player.HeldItem = this.gameObject;
                PickUp(player.transform);
                player.PickupItem(this);
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
                    enemy.TakeHit(gameObject,10);
                    enemyHit = true;
                }
            }
        }
        if (!enemyHit){
            Vector3 spawnPoint = transform.position;
            spawnPoint.y = .25f;
            manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Mushroom);
        }
        manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.Bomb);
    }
}
