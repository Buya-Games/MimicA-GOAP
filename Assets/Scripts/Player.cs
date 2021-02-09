using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Creature
{
    PlayerControl control;
    public event Action OnTeach;
    public GOAPAct CurrentEvent;
    [SerializeField] LayerMask interactLM;
    bool teaching = false; //set by companions to true when they're born (and false after 10 turns)
    public float bobHeight;

    // Awake is called before Start so I use it to initialize all the core stuff
    protected override void Awake(){
        base.Awake();
        control = GetComponent<PlayerControl>();
    }

    //Start is called before first frame update
    void Start(){
        Init();
        MyStats.Speed = 6;
        bobSpeed = 14;
        control.moveSpeed = MyStats.Speed;
        MyStats.Range = 15;
        myName = myName + " (Player)";
        gameObject.name = myName;
    }

    protected override void Update(){
        if (manager.PlayerAlive){
            //base.Update();
            Vector3 bobPos = visibleMesh.localPosition + transform.up * Mathf.Sin(Time.time * bobSpeed) * 0.015f;//mesh bobs up/down (collider doesn't move)
            bobPos.y = Mathf.Clamp(bobPos.y,0,.7f);
            visibleMesh.localPosition = bobPos;
            bobHeight = visibleMesh.transform.localPosition.y * 5;
            // if (bobHeight > bobMax){
            //     bobMax = bobHeight;
            // }
            // if (bobHeight < bobMin){
            //     bobMin = bobHeight;
            // }
            // if (visibleMesh.localPosition.y > .7f || visibleMesh.localPosition.y < 0){
            //     visibleMesh.localPosition = new Vector3(0,0.25f,0);
            // }
            if (HeldItem != null){
                control.mouseHighlight = true;
            } else {
                control.mouseHighlight = false;
            }
        }
    }

    public void Interact(){
        if (HeldItem != null){
            ThrowItem();
        } else {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position,2,interactLM);//check surroundings for stuff
            if (nearbyObjects.Length>0){
                GameObject closest = Tools.FindClosestColliderInGroup(nearbyObjects,transform.position);
                if (closest != null){
                    SetTarget(closest);
                }
                //Target = nearbyObjects[0].gameObject;//i only use the first object of an array which I believe is random, but that's fine I think?
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

    public void EatItem(){
        if (HeldItem != null){
            CurrentEvent = new Eat(HeldItem.layer);
            if (CurrentEvent.PerformEvent(this)){
                if (teaching){
                    OnTeach();
                }
            }
        }
    }

    public void ThrowItem(){
        Vector3 mousePos = control.MousePos();
        int targetLayer = control.ThrowingTarget(mousePos);
        // if (targetLayer == 13 && HeldItem.layer == 7){//throwing to self
        //     Eat eat = new Eat();
        //     CurrentEvent = eat;
        // } else {
            ThrowItem throwItem = new ThrowItem(mousePos,HeldItem.layer,control.ThrowingTarget(mousePos));
            CurrentEvent = throwItem;
        //}
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

    protected override void Die(){
        if (HeldItem != null){
            DropItem();
        }
        StopAllCoroutines();
        control.StopMovement();
        healthBar.gameObject.SetActive(false);
        Debug.Log(myName + " has died");
        manager.PlayerDeath();
    }
}
