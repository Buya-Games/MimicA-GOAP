using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : MonoBehaviour
{
    GameManager manager;
    [SerializeField] Transform poopPos;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.layer == 7){ //if berry
            manager.spawner.DespawnEnvironment(col.gameObject,Spawner.EnvironmentType.Berry);
            manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
        }
        if (col.gameObject.layer == 9){ //if human 
            Creature creature = col.gameObject.GetComponent<Creature>();
            if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 7){//holding a berry
                manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Berry);
                manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.Bomb);
            }

        }
    }

    void EatBerryAndPoopBomb(GameObject col){
        
    }
}
