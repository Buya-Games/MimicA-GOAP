using System.Collections.Generic;
using UnityEngine;

//handles creating action plans around goals. need to optimize more
public class CreatureLogic : Creature
{
    public List<GOAPAct> availableActions = new List<GOAPAct>();
    protected Queue<GameState.State> myGoals = new Queue<GameState.State>();
    protected Queue<GOAPAct> toDo = new Queue<GOAPAct>();
    public GOAPAct CurrentAction;//should be protected not public, but just using when player dead and buddies learn from each other
    float eventMaxTime = 5; //don't spend more than 5 seconds on any given decision
    float breakTime = .5f;//amount of time between decisions and stuff
    GOAPPlan planner;

    protected override void Awake() {
        base.Awake();
        planner = FindObjectOfType<GOAPPlan>();
    }


    protected virtual void GetPlan(){
        if (alive){
            CurrentAction = null;
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

        if (Target != null && !Target.activeSelf){//if somehow target became inactive, set it to null
            if (manager.debug){Debug.Log("target set to null");}
            ClearTarget();
        }

        if (toDo != null && toDo.Count>0){//if we still got things we wanna peform
            CurrentAction = toDo.Dequeue();
            PerformDecision();
        } else {
            GetPlan();
        }
    }

    //need to tidy this up later
    protected virtual void PerformDecision(){
        
        //maximum time to spend on an action before check if a better plan exists
        //should we really just give up? what if still in the process of moving?
        if (Time.time > eventMaxTime){
            eventMaxTime+=Time.time;
            GetDecision();
        } else {
            if (CurrentAction != null){ 
                if (CurrentAction.GetTarget(this)){//checks if there's a target for this action
                    if (CurrentAction.CheckPreconditions(GetCurrentState())){//checks that currentevent can still be performed
                        if (CurrentAction.CheckRange(this)){//checks if in range
                            
                            //if already in range, creature would sometimes look wrong way
                            StartCoroutine(FaceTarget((Target.transform.position - transform.position).normalized));
                            if (CurrentAction.PerformEvent(this)){//performs actual action
                                //Debug.Log(nextEvent + " SUCCEEDED");
                            } else {
                                Debug.Log(CurrentAction + " FAILED");
                            }
                            Invoke("GetDecision",breakTime * Random.Range(0.4f,.6f));
                        } else { //if not in range, move to target
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

    //checks if health is below 50 and if it is, sets the goal to EATING
    //for now creatures have just one goal so myGoals doesn't need to be a queue. but might have multiple goals in future
    protected List<GameState.State> HungryCheck(){
        List<GameState.State> whatToDo = new List<GameState.State>();
        if (health<50){//if hungry, start eating
            whatToDo.Add(GameState.State.goalEat);

            //just in case drop whatever you were carrying as it might impede action
            if (HeldItem != null){
                DropItem();
            }
        } else { //carry on with your goals
            //GameState.State goal = myGoals.Peek();
            whatToDo.Add(myGoals.Peek());//just adding one goal at a time... passing them all would be more ideal but more work for GOAPPlan
            if (manager.debug){Debug.Log(string.Format("[{0}] goal is {1}",myName,whatToDo[0]));}
            //myGoals.Enqueue(goal);
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
