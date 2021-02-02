using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLogic : Creature
{
    public List<FrameworkEvent> availableActions = new List<FrameworkEvent>();
    protected Queue<GameState.State> myGoals = new Queue<GameState.State>();
    protected Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    protected FrameworkEvent currentEvent;
    float eventMaxTime = 5; //don't spend more than 5 seconds on any given decision
    float breakTime = .5f;//amount of time between decisions and stuff
    GOAPPlan planner;

    protected override void Awake() {
        base.Awake();
        planner = FindObjectOfType<GOAPPlan>();
    }
    public override void Init(){
        base.Init();

        //give everyone basic skills to eat
        availableActions.Add(new Eat());//eat berry
        availableActions.Add(new MeleeAttack(6,1));//harvest berries from bushes
        availableActions.Add(new PickupItem(7));//pickup berries from ground
    }

    protected virtual void GetPlan(){
        currentEvent = null;
        myText.text = "";
        toDo = planner.MakePlan(this,GetCurrentState(),HungryCheck());
        if (toDo == null){ //if failed to find a plan
            Invoke("Idle",breakTime * Random.Range(1f,2f));
        } else {
            GetDecision();
        }
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    protected virtual void GetDecision(){
        currentEvent = null;
        myText.text = "";

        if (toDo != null && toDo.Count>0){
            currentEvent = toDo.Dequeue();
            myText.text = currentEvent.ToString();
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
            if (currentEvent != null){ 
                if (currentEvent.GetTarget(this)){ 
                    if (currentEvent.CheckPreconditions(GetCurrentState())){//checks that currentevent can still be performed
                        if (currentEvent.CheckRange(this)){
                            StartCoroutine(FaceTarget((Target.transform.position - transform.position).normalized));
                            if (currentEvent.PerformEvent(this)){
                                //Debug.Log(nextEvent + " SUCCEEDED");
                            } else {
                                Debug.Log(currentEvent + " FAILED");
                            }
                            Invoke("GetDecision",breakTime * Random.Range(0.4f,.6f));
                        } else {
                            StopAllCoroutines();
                            StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                        }
                    } else { if(manager.debug){Debug.Log(currentEvent + ": failed precheck");}
                        Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//if can't find any targets, get a new plan
                    }
                } else { if(manager.debug){Debug.Log(currentEvent + ": no targets");}
                    Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//if can't find any targets, get a new plan
                }
            } else { if(manager.debug){Debug.Log("no event");}
                Invoke("GetPlan",breakTime * Random.Range(0.8f,1.2f));//no event, get new plan
            }
        }
    }

    protected List<GameState.State> HungryCheck(){
        List<GameState.State> whatToDo = new List<GameState.State>();
        if (health<50){
            if (HeldItem != null){
                DropItem();
            }
            whatToDo.Add(GameState.State.goalEat);
        } else {
            GameState.State goal = myGoals.Dequeue();
            whatToDo.Add(goal);//just adding one goal at a time... passing them all would be more ideal
            myGoals.Enqueue(goal);
        }
        return whatToDo;
    }

    protected override void PostMovementChecks(){
        base.PostMovementChecks();
        if (currentEvent != null && currentEvent.CheckPreconditions(GetCurrentState()) && !currentEvent.CheckRange(this)){
            if (Target != null){//checks that we still perform it but out of range
                StopAllCoroutines();
                StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
            } else {
                Invoke("PerformDecision",breakTime * .2f);
            }
        } else {
            Invoke("PerformDecision",breakTime * .2f);
        }
    }


}
