using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLogic : Creature
{
    public List<GOAPAct> availableActions = new List<GOAPAct>();
    protected Queue<GameState.State> myGoals = new Queue<GameState.State>();
    protected Queue<GOAPAct> toDo = new Queue<GOAPAct>();
    public GOAPAct CurrentAction;//should be protected not public, but just using when player dead and buddies learn from each other
    float eventMaxTime = 5; //don't spend more than 5 seconds on any given decision
    float breakTime = .5f;//amount of time between decisions and stuff
    GOAPPlan planner;
    protected Eat eatBerry;
    protected PickupItem pickupBerry;
    protected MeleeAttack harvestBerry;

    protected override void Awake() {
        base.Awake();
        planner = FindObjectOfType<GOAPPlan>();

        //everyone gets basic skills to eat and survive
        eatBerry = new Eat(7);
        pickupBerry = new PickupItem(7);
        harvestBerry = new MeleeAttack(6,2);
    }

    protected void AddCoreSkills(){
        availableActions.Add(harvestBerry);//eat berry
        availableActions.Add(pickupBerry);//harvest berries from bushes
        availableActions.Add(eatBerry);//pickup berries from ground
    }


    protected virtual void GetPlan(){
        if (alive){
            CurrentAction = null;
            myText.text = "";
            toDo = planner.MakePlan(this,GetCurrentState(),HungryCheck());
            if (toDo == null || toDo.Count == 0){ //if failed to find a plan or plan has no moves
                if(manager.debug){Debug.Log("toDo was null");}
                if (HeldItem != null){//if holding item is preventing you finding a path or actions, let's try dropping the item
                    DropItem();
                }
                Invoke("Idle",breakTime * Random.Range(1f,2f));
            } else {
                if(manager.debug){Debug.Log(toDo.Peek());}
                GetDecision();
            }
        }
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    protected virtual void GetDecision(){
        CurrentAction = null;
        myText.text = "";

        if (Target != null && !Target.activeSelf){//if somehow target became inactive, set it to null
            if (manager.debug){Debug.Log("target set to null");}
            ClearTarget();
        }

        if (toDo != null && toDo.Count>0){//if we still got things we wanna peform
            CurrentAction = toDo.Dequeue();
            myText.text = CurrentAction.ToString();
            PerformDecision();
        } else {
            GetPlan();
        }
    }

    //need to tidy this up later
    protected virtual void PerformDecision(){
        if (Time.time > eventMaxTime){//should we really just give up? what if still moving to X?
            eventMaxTime+=Time.time;
            GetDecision();
        } else {
            if (CurrentAction != null){ 
                if (CurrentAction.GetTarget(this)){
                    if (CurrentAction.CheckPreconditions(GetCurrentState())){//checks that currentevent can still be performed
                        if (CurrentAction.CheckRange(this)){
                            StartCoroutine(FaceTarget((Target.transform.position - transform.position).normalized));
                            if (CurrentAction.PerformEvent(this)){
                                //Debug.Log(nextEvent + " SUCCEEDED");
                            } else {
                                Debug.Log(CurrentAction + " FAILED");
                            }
                            Invoke("GetDecision",breakTime * Random.Range(0.4f,.6f));
                        } else {
                            StopAllCoroutines();
                            StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                        }
                    } else { if(manager.debug){Debug.Log(CurrentAction + ": failed precheck");}
                        Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//if can't perform this action anymore,  get a new plan
                    }
                } else { if(manager.debug){Debug.Log(CurrentAction + ": no targets");}
                    Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//if can't find any targets, get a new plan
                }
            } else { if(manager.debug){Debug.Log("no event");}
                Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//no event, get new plan
            }
        }
    }

    protected List<GameState.State> HungryCheck(){
        List<GameState.State> whatToDo = new List<GameState.State>();
        if (health<50){//if hungry, start eating
            if (HeldItem != null){
                DropItem();
            }
            whatToDo.Add(GameState.State.goalEat);

            //adding/removing eat cuz when they bored sometimes all they do is just keep eating
            // if (!availableActions.Contains(eatBerry)){
            //     availableActions.Add(eatBerry);
            // }
        } else { //carry on with your goals
            GameState.State goal = myGoals.Dequeue();
            whatToDo.Add(goal);//just adding one goal at a time... passing them all would be more ideal but then more work for GOAPPlan
            myGoals.Enqueue(goal);

            // if (availableActions.Contains(eatBerry)){
            //     availableActions.Remove(eatBerry);
            // }
        }
        return whatToDo;
    }

    protected override void PostMovementChecks(){
        base.PostMovementChecks();
        //check if plan has changed. This is stupidly expensive but I dunno how else to make them dynamically adapt to world changes
        Queue<GOAPAct> checkPlan = planner.MakePlan(this,GetCurrentState(),HungryCheck());
        if (checkPlan != null && checkPlan.Count > 0 && checkPlan.Peek() == CurrentAction){
            //checks that action/goals/target haven't changed, but that we still out of range so need to keep moving
            if (CurrentAction.CheckPreconditions(GetCurrentState()) && !CurrentAction.CheckRange(this) && Target != null){
                StopAllCoroutines();
                StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
            } else {
                Invoke("PerformDecision",breakTime * .2f);
            }
        } else {
            toDo = checkPlan;
            if (manager.debug){Debug.Log("plan changed");}
            Invoke("GetDecision",breakTime * .4f);
        }
    }
}
