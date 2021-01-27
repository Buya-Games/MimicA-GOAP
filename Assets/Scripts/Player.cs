using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Goblin
{
    PlayerControl control;
    public event Action OnTeach;
    public FrameworkEvent CurrentEvent;
    [SerializeField] LayerMask interactLM;
    bool teaching = false; //set by companions to true when they're born (and false after 10 turns)

    // Awake is called before Start so I use it to initialize all the core stuff
    protected override void Awake(){
        base.Awake();
        control = GetComponent<PlayerControl>();
    }

    //Start is called before first frame update
    protected override void Start(){
        base.Start();
        Init();
        control.moveSpeed = MyStats.Speed;
    }

    public void Interact(){
        if (HeldItem != null){
            ThrowItem();
        } else {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position,2,interactLM);//for player, I just look at surrounding environment. For AI, use FindClosestObjectOfLayer
            if (nearbyObjects.Length>0){ 
                Target = nearbyObjects[0].gameObject;//i only use the first object of an array which I believe is random, but that's fine I think?
                MeleeAttack();
            } else {
                Swing();
            }   
        }
    }

    public void MeleeAttack(){
        MeleeAttack melee = new MeleeAttack(Target.gameObject.layer);
        CurrentEvent = melee;//prior event should be destroyed by GC
        if (melee.CheckPreconditions(GetCurrentState())){
            if (melee.PerformEvent(this)){
                if (teaching){
                    OnTeach();
                }
            }
        }
    }
    
    public void PickupItem(IThrowable item){
        Target = item.ThisGameObject();
        PickupItem pickup = new PickupItem(Target.gameObject.layer);
        CurrentEvent = pickup;
        if (teaching){
            OnTeach();
        }
        //I don't perform anything here cuz for player the logic is done inside Item.cs
    }

    public void ThrowItem(){
        ThrowItem throwItem = new ThrowItem(control.MousePos(),HeldItem.layer);
        CurrentEvent = throwItem;
        if (throwItem.CheckPreconditions(GetCurrentState())){
            if (throwItem.PerformEvent(this)){
                if (teaching){
                    OnTeach();
                }
            }
        }
    }

    //checks if any companions still teachable
    public void CheckForStudents(){
        bool anyoneLearning = false;
        foreach (GameObject b in manager.spawner.ActiveBuddies){
            if (b.GetComponent<Buddy>().learning){
                anyoneLearning = true;
            }
        }
        if (anyoneLearning){
            teaching = true;
        } else {
            teaching = false;
        }
    }

}
