using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : MonoBehaviour, IHittable
{
    GameManager manager;
    [SerializeField] Transform poopPos;
    [HideInInspector]public int health = 10;
    Rigidbody rb;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        Invoke("RandomWalk",1f);
    }

    void RandomWalk(){
        StartCoroutine(MovingAround(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))));
    }

    IEnumerator MovingAround(Vector3 dir){
        StartCoroutine(FaceTarget(dir));
        int counter = Random.Range(500,1000);
        while (counter>0){
            transform.position = transform.position + dir * (Time.fixedDeltaTime * .5f);
            counter--;
            yield return null;
        }
        RandomWalk();
    }

    protected IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .01f){
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
        int counter = 0;
        while (counter<150){
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
            counter++;
            yield return null;
        }
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 300);

        // Health-=damage;
        // if (Health <= 0){
        //     Die();
        // }
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.layer == 7){ //if berry
            manager.spawner.DespawnEnvironment(col.gameObject,Spawner.EnvironmentType.Berry);
            manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
            manager.particles.EatingBerry(col.contacts[0].point);
        }
        if (col.gameObject.layer == 9){ //if fungus
            manager.spawner.DespawnEnvironment(col.gameObject,Spawner.EnvironmentType.Fungus);
            manager.CollectFungus();
            manager.particles.EatingFungus(col.contacts[0].point);
        }
        if (col.gameObject.layer == 13 || col.gameObject.layer == 12){ //if human or companion
            Creature creature = col.gameObject.GetComponent<Creature>();
            if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 7){//holding a berry
                manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Berry);
                manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
            }
            if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 9){//holding a fungus
                manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Fungus);
                manager.CollectFungus();
            }
        }
    }
}
