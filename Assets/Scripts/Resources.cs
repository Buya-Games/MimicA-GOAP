using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resources : MonoBehaviour, ITargettable
{
    bool accessible;
    public Spawner.EnvironmentType MyType;
    GameManager manager;
    [SerializeField] public GameObject Owner { get; set; }
    public GameObject gameObj { get; set; }

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        SetType();
        gameObj = gameObject;
    }

    void SetType(){
        if (gameObject.layer == 6){
            MyType = Spawner.EnvironmentType.Bush;
        }
        if (gameObject.layer == 8){
            MyType = Spawner.EnvironmentType.Mushroom;
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
}
