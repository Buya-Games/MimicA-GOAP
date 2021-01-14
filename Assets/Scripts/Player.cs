using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Creature
{
    PlayerControl control;
    public event Action OnTeach;
    public FrameworkEvent CurrentEvent;
    [SerializeField] LayerMask interactLM;

    // Awake is called before Start so I use it to initialize all the core stuff
    protected override void Awake(){
        base.Awake();
        control = GetComponent<PlayerControl>();
    }

    //Start is called before first frame update
    protected override void Start(){
        base.Start();
        control.moveSpeed = MyStats.Speed;
    }

    public void Interact(){
        if (HeldItem != null){
            ThrowItem();
        } else {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position,2,interactLM);//for player, I just look at surrounding environment. For AI, use FindClosestObjectOfLayer
            if (nearbyObjects.Length>0){ 
                Target = nearbyObjects[0].gameObject;//i only use the first object of an array which I believe is random, but that's fine I think?
                TargetDist = GetTargetDist(Target.transform.position);
                MeleeAttack();
            }   
        }
    }

    public void ThrowItem(){
        ThrowItem throwItem = new ThrowItem(control.MousePos(),HeldItem.layer);
        CurrentEvent = throwItem;
        if (throwItem.CheckPreconditions(this)){
            if (throwItem.PerformEvent(this)){
                OnTeach();
            }
        }
    }

    public void MeleeAttack(){
        MeleeAttack melee = new MeleeAttack();
        melee.TargetLayer = Target.gameObject.layer;
        CurrentEvent = melee;//prior event should be destroyed by GC
        if (melee.CheckPreconditions(this)){
            if (melee.PerformEvent(this)){
                OnTeach();
            }
        }
    }
    
    public void PickupBerry(Berry berry){
        Target = berry.gameObject;

        PickupBerry pickup = new PickupBerry();
        pickup.TargetLayer = Target.gameObject.layer;
        CurrentEvent = pickup;
        OnTeach();
        //I don't perform anything here cuz for player the logic is done inside Berry.cs
    }

}
