using UnityEngine;
using System;

//extends Creature but basically it's own class. handles player actions
public class Player : Creature
{
    PlayerControl control;
    public event Action OnTeach;
    public GOAPAct CurrentAction;
    [SerializeField] LayerMask interactLM;
    bool teaching = false; //set by companions to true when they're born (and false after 10 turns)
    public float bobHeight; //determines timing skill for melee action

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
            base.Update();
            bobHeight = visibleMesh.transform.localPosition.y * 5;//just an artbitrary number watched by MeleeAttack
            
            if (HeldItem != null){
                control.mouseHighlight = true;
            } else {
                control.mouseHighlight = false;
            }
        }
    }

    //triggered when pressing space bar (via PlayerControl)
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
                if (bobHeight < 1.3f){
                    Swing(2);//2 will show a red trail streak to indicate pw0ness
                } else {
                    Swing();
                }
            }   
        }
    }

    public void MeleeAttack(){
        MeleeAttack melee = new MeleeAttack(Target.gameObject.layer, bobHeight);
        CurrentAction = melee;//prior event should be destroyed by GC i think?
        if (melee.CheckPreconditions(GetCurrentState())){ //if this can be performed
            if (melee.PerformEvent(this)){ //perform it
                if (teaching){//and if you're teaching someone
                    OnTeach();//broadcast the teaching event
                }
                //tutorial shit
                if (manager.Tutorial && manager.tut.Tut0WASD){
                    manager.tut.Tut0WASD = false;
                    manager.tut.Tut1PickupBerry = true;
                    manager.tut.DisplayNextTip(1);//pickup the berry
                }
                //end tutorial shit
            }
        }
    }
    
    public override void PickUp(IThrowable item){
        base.PickUp(item);
        PickupItem pickup = new PickupItem(item.ThisGameObject().layer);
        CurrentAction = pickup;
        if (teaching){
            OnTeach();
        }
        //tutorial shit
        if (manager.Tutorial && manager.tut.Tut1PickupBerry){
            manager.tut.Tut1PickupBerry = false;
            manager.tut.Tut2EatBerry = true;
            manager.tut.DisplayNextTip(2);//eat the berry
        }
        //end tutorial shit
    }

    //you can eat anything you can pick up. and that will be taught to buddies.
    public void EatItem(){
        if (HeldItem != null){
            CurrentAction = new Eat(HeldItem.layer);
            if (CurrentAction.PerformEvent(this)){
                if (teaching){
                    OnTeach();
                }
                //tutorial shit
                if (manager.Tutorial){
                    if (manager.tut.Tut2EatBerry){
                        manager.tut.Tut2EatBerry = false;
                        manager.tut.Tut3HardMelee = true;
                        manager.tut.DisplayNextTip(3);//hit bush well
                        manager.tut.SpawnBush();
                    } else {
                        manager.tut.SpawnBush();   
                    }
                    
                }
                //end tutorial shit
            }
        }
    }

    //throwing an item is determined by where the mouse is
    public void ThrowItem(){
        Vector3 mousePos = control.MousePos();//get mouse position
        int targetLayer = control.ThrowingTarget(mousePos);//get what is at the mouse position

        //note, mousePos isn't actually used yet. I thought about teaching buddies to throw to a specific position throw RELATIVE to the cow for example
        //but unnecessary
        ThrowItem throwItem = new ThrowItem(mousePos,HeldItem.layer,control.ThrowingTarget(mousePos));
        
        CurrentAction = throwItem;
        if (CurrentAction.PerformEvent(this)){
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
        manager.PlayerDeath();
    }
}
