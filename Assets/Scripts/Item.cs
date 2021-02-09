using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IThrowable, ITargettable
{
    GameManager manager;
    Rigidbody rb;
    float gravity;
    float height = 5;
    public Spawner.EnvironmentType MyType;
    bool accessible = true;//once object is pickedup/thrown it is inaccessible until it drops
    Vector3 origScale;
    public GameObject Owner { get; set; }
    public GameObject gameObj { get; set; }

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        gravity = Physics.gravity.y;
        origScale = transform.localScale;
        gameObj = gameObject;
        SetType();
    }

    void SetType(){
        if (gameObject.layer == 7){
            MyType = Spawner.EnvironmentType.Berry;
        }
        if (gameObject.layer == 9){
            MyType = Spawner.EnvironmentType.Fungus;
        }
        if (gameObject.layer == 10){
            MyType = Spawner.EnvironmentType.BerryPoop;
        }
        if (gameObject.layer == 16){
            MyType = Spawner.EnvironmentType.FungusPoop;
        }
    }

    public void Targeted(GameObject who){
        if (accessible){
            accessible = false;
            Owner = who;
            //manager.spawner.ThrowOrPickUpObject(gameObject,MyType,accessible);    
        }
    }

    public void NotTargeted(){
        if (!accessible){
            accessible = true;
            Owner = null;
            //manager.spawner.ThrowOrPickUpObject(gameObject,MyType,accessible);    
        }
    }

    public GameObject ThisGameObject(){
        return this.gameObject;
    }

    public void PickUp(Creature agent){
        accessible = false;
        //manager.spawner.ThrowOrPickUpObject(gameObject,MyType,accessible);
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
        accessible = true;
        //manager.spawner.ThrowOrPickUpObject(gameObject,MyType,accessible);
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;

        transform.parent = null;
        transform.localScale = origScale;
    }

    public void ThrowObject(Vector3 where, float throwStrength){
        if (accessible){
            accessible = false;
            //manager.spawner.ThrowOrPickUpObject(gameObject,MyType,accessible);
        }

        Vector3 higherPos = transform.position;
        higherPos.y = 5;
        transform.position = higherPos;
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        transform.parent = null;
        transform.localScale = origScale;
        Vector3 traj = CalculateTrajectory(where,throwStrength);
        if (!float.IsNaN(traj.x)){//##spits out weird number if target is in same position, so just checking for that exception
            rb.velocity = CalculateTrajectory(where, throwStrength);
        }
    }

    Vector3 CalculateTrajectory(Vector3 where, float throwStrength){
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
        if (!accessible && col.gameObject.layer == 11){//if collide with enemy after it has been thrown
            Drop();
            if (MyType == Spawner.EnvironmentType.BerryPoop || MyType == Spawner.EnvironmentType.FungusPoop){
                BombBoom();
            }
        }
        if (!accessible && col.gameObject.layer == 4){//if collide with ground after it has been thrown
            Drop();
            Vector3 spawnPoint = transform.position;
            spawnPoint.y = 0f;
            if (MyType == Spawner.EnvironmentType.BerryPoop){
                manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Bush);
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.BerryPoop);
                manager.particles.BombExplosion(transform.position);
            }
            if (MyType == Spawner.EnvironmentType.FungusPoop){
                manager.spawner.SpawnEnvironment(spawnPoint,Spawner.EnvironmentType.Mushroom);
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.FungusPoop);
                manager.particles.BombExplosion(transform.position);
            }
        }
        if (col.gameObject.layer == 13){//if player
            Player player = col.gameObject.GetComponent<Player>();
            if (player.HeldItem == null){
                PickUp(player);
            } else {
                //dont know why but sometimes item gets stuck so check if its still one of the children
                Item itemCheck = player.GetComponentInChildren<Item>();
                if (itemCheck == null){ //if not a child, then player has no item
                    player.HeldItem = null;
                    Drop();
                }
            }
        }
    }

    void BombBoom(){
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
        if (enemyHit){
            manager.particles.BombExplosion(transform.position);
            if (MyType == Spawner.EnvironmentType.BerryPoop){
            manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.BerryPoop);
            }
            if (MyType == Spawner.EnvironmentType.FungusPoop){
                manager.spawner.DespawnEnvironment(gameObject,Spawner.EnvironmentType.FungusPoop);
            }
        }
    }
}
