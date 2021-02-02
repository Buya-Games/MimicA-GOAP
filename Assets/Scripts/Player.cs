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
    bool teaching = false; //set by companions to true when they're born (and false after 10 turns)
    public float bobHeight;
    public float bobMax, bobMin;

    // Awake is called before Start so I use it to initialize all the core stuff
    protected override void Awake(){
        base.Awake();
        control = GetComponent<PlayerControl>();
        bobMin = 5;
    }

    //Start is called before first frame update
    protected override void Start(){
        base.Start();
        Init();
        MyStats.Speed = 4;
        bobSpeed = 15;
        control.moveSpeed = MyStats.Speed;
    }

    protected override void Update(){
        base.Update();
        bobHeight = visibleMesh.transform.localPosition.y * 6;
        if (bobHeight > bobMax){
            bobMax = bobHeight;
        }
        if (bobHeight < bobMin){
            bobMin = bobHeight;
        }
    }

    public void Interact(){
        if (HeldItem != null){
            ThrowItem();
        } else {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position,2,interactLM);//check surroundings for stuff
            if (nearbyObjects.Length>0){ 
                Target = nearbyObjects[0].gameObject;//i only use the first object of an array which I believe is random, but that's fine I think?
                MeleeAttack();
            } else {
                Swing();
            }   
        }
    }

    public void MeleeAttack(){
        MeleeAttack melee = new MeleeAttack(Target.gameObject.layer, bobHeight);
        CurrentEvent = melee;//prior event should be destroyed by GC
        if (melee.CheckPreconditions(GetCurrentState())){
            if (melee.PerformEvent(this)){
                if (teaching){
                    OnTeach();
                }
            }
        }
    }
    
    public override void PickUp(IThrowable item){
        base.PickUp(item);
        PickupItem pickup = new PickupItem(item.ThisGameObject().layer);
        CurrentEvent = pickup;
        if (teaching){
            OnTeach();
        }
        //I don't perform anything here cuz for player the logic is done inside Item.cs
    }

    public void ThrowItem(){
        Vector3 mousePos = control.MousePos();
        int targetLayer = control.ThrowingTarget(mousePos);
        if (targetLayer == 13 && HeldItem.layer == 7){//throwing to self
            Eat eat = new Eat();
            CurrentEvent = eat;
        } else {
            ThrowItem throwItem = new ThrowItem(mousePos,HeldItem.layer,control.ThrowingTarget(mousePos));
            CurrentEvent = throwItem;
        }
        if (CurrentEvent.PerformEvent(this)){
            if (teaching){
                OnTeach();
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
