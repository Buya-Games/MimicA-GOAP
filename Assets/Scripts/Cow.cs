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
            Debug.Log("ate a berry");
            manager.spawner.DespawnEnvironment(col.gameObject,Spawner.EnvironmentType.Berry);
            manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
        }
        if (col.gameObject.layer == 9){ //if fungus
            Debug.Log("ate a fungus");
            manager.spawner.DespawnEnvironment(col.gameObject,Spawner.EnvironmentType.Fungus);
            manager.CollectFungus();
        }
        if (col.gameObject.layer == 13 || col.gameObject.layer == 12){ //if human or companion
            Creature creature = col.gameObject.GetComponent<Creature>();
            if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 7){//holding a berry
                Debug.Log("fed berry by a creature");
                manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Berry);
                manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
            }
            if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 9){//holding a fungus
                Debug.Log("fed fungus by a creature");
                manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Fungus);
                manager.CollectFungus();
            }
        }
    }
}
