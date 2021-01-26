using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkCompanionLogic : Creature
{
    //this sits on every companion and
    //1) at outset will listen for possibleEvents
    //2) returns getDecision when need to figure out what to do
    [SerializeField] FrameworkEvent currentEvent;
    Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    Player player;
    public IEnumerator currentCoroutine;
    public bool learning;
    Rigidbody rb;
    float eventMaxTime = 10; //don't spend more than 10 seconds on any given decision
    Queue<GameState.State> MyGoal = new Queue<GameState.State>();
    public List<FrameworkEvent> queuedActions = new List<FrameworkEvent>();

    protected override void Start(){
        base.Start();
        player = GameObject.FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody>();
        manager.spawner.ActiveCompanions.Add(this);
        learning = true;
        player.CheckForStudents();
        player.OnTeach += Learn;//tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
        //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
    }

    void Learn(){
        if (availableActions.Count<4){//listen until 10 actions
            availableActions.Add(player.CurrentEvent.Clone());
            motiveAttack+=player.CurrentEvent.motiveAttack;
            motiveHarvest+=player.CurrentEvent.motiveHarvest;
            motiveReproduction+=player.CurrentEvent.motiveReproduction;
        }
        if (availableActions.Count >= 4){//after 10, we will setup our lifetime goals and stop listening
            SetGoals();
            learning = false;
            player.OnTeach -= Learn;//turning off listener
            player.CheckForStudents();//telling player to stop teaching unless other students
            GetPlan();
        }
        
        // if (toDo.Count<1){
        //     toDo.Enqueue(player.CurrentEvent.Clone());
        //     Debug.Log("learned " + player.CurrentEvent);
        //     if (currentEvent == null){
        //         GetDecision();
        //     }
        // }
    }

    void SetGoals(){
        MyGoal.Clear();
        if (motiveAttack>motiveHarvest){//i'm sure more elegant way to do this, but just grinding thru it guh
            if (motiveAttack>motiveReproduction){
                MyGoal.Enqueue(GameState.State.goalAttacked);
            } else if (motiveAttack==motiveReproduction){
                MyGoal.Enqueue(GameState.State.goalReproduced);
                MyGoal.Enqueue(GameState.State.goalAttacked);
            } else {
                MyGoal.Enqueue(GameState.State.goalReproduced);
            }
        } else if (motiveAttack==motiveHarvest && motiveAttack>motiveReproduction){
            MyGoal.Enqueue(GameState.State.goalHarvested);
            MyGoal.Enqueue(GameState.State.goalAttacked);
        } else if (motiveHarvest>motiveReproduction){
            MyGoal.Enqueue(GameState.State.goalHarvested);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest>motiveAttack) {
            MyGoal.Enqueue(GameState.State.goalHarvested);
            MyGoal.Enqueue(GameState.State.goalReproduced);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest==motiveAttack) {
            MyGoal.Enqueue(GameState.State.goalHarvested);
            MyGoal.Enqueue(GameState.State.goalReproduced);
            MyGoal.Enqueue(GameState.State.goalAttacked);
        } else {
            MyGoal.Enqueue(GameState.State.goalReproduced);
        }
        Debug.Log(motiveAttack + " " + motiveHarvest + " " + motiveReproduction + ", " + MyGoal.Peek());
    }

    

    List<GameState.State> GetGoalState(){
        List<GameState.State> goalState = new List<GameState.State>();
        goalState.Add(MyGoal.Dequeue());
        MyGoal.Enqueue(goalState[0]);
        return goalState;
    }

    public void GetPlan(){
        FrameworkPlanner planner = FindObjectOfType<FrameworkPlanner>();
        toDo.Clear();
        toDo = planner.MakePlan(this,GetCurrentState(),GetGoalState());
        // for (int i = 0;i<toDo.Count;i++){
        //     queuedActions.Add(toDo.Dequeue());
        // }
        Debug.Log("we have a " + toDo.Count + " point plan!");
        GetDecision();
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    public void GetDecision(){
        currentEvent = null;
        if (toDo.Count>0){
            Debug.Log("yup, gonna do " + toDo.Peek());
            currentEvent = toDo.Dequeue();
            PerformDecision(currentEvent);
        } else {
            Debug.Log("oop, need a new plan");
            GetPlan();
        }
    }

    void PerformDecision(FrameworkEvent nextEvent){
        if (Time.time > eventMaxTime){
            eventMaxTime+=Time.time;
            GetDecision();
        } else {
            if (Target == null){
                Debug.Log(nextEvent + " had no target, so looking for a " + nextEvent.EventLayer);
                Target = FindClosestObjectOfLayer(nextEvent.EventLayer);
            }
            //Target = FindClosestObjectOfLayer(nextEvent.EventLayer);//check again if something closer popped up
            if (Target != null){
                //StartCoroutine(FaceTarget());//facing target
                TargetDist = GetTargetDist(Target.transform.position);
                Debug.Log(nextEvent + " range is " + nextEvent.EventRange + " and current TargetDist is " + TargetDist);
                if (nextEvent.CheckRange(this)){
                    if (nextEvent.CheckPreconditions(GetCurrentState())){
                        if (nextEvent.PerformEvent(this)){
                            GetDecision();
                            Debug.Log(nextEvent + " SUCCEEDED");
                        } else {
                            GetDecision();
                            Debug.Log(nextEvent + " FAILED");
                        }
                        toDo.Enqueue(currentEvent);
                        //Debug.Log("Finished and requeued " + currentEvent);
                        Invoke("GetDecision",.5f);
                    } else {
                        toDo.Enqueue(currentEvent);
                        //Debug.Log("Precheck failed and requeued " + currentEvent);
                        Invoke("GetDecision",.5f);
                    }
                } else {
                    //StartCoroutine(FaceTarget());//facing target
                    StopAllCoroutines();
                    StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                }
            } else {
                Invoke("RepeatDecision",1f);//repeat this (keep trying to find a target)
            }
        }
    }

    void RepeatDecision(){
        PerformDecision(currentEvent);
    }

    public void LookAtLocation(Vector3 where){
        StartCoroutine(FaceTarget(where));
    }

    IEnumerator Movement(Vector3 dir){
        // Vector3 rotCheck = transform.rotation.eulerAngles - Quaternion.LookRotation(new Vector3(dir.x,0,dir.z)).eulerAngles;
        // if (rotCheck.y > 5 || rotCheck.y < -5){//this is probably so stupidly expensive
        //     StartCoroutine(FaceTarget(Target.transform.position));
        // }
        int counter = 0;
        rb.velocity = Vector3.zero;
        while (counter < 50){
            rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed/2);
            counter++;
            yield return null;
        }
        TargetDist = GetTargetDist(Target.transform.position);
        PerformDecision(currentEvent);
    }

    Queue<FrameworkEvent> FrameWorkEventsToDo(){
        toDo = new Queue<FrameworkEvent>();
        return toDo;
    }

    public FrameworkEvent GetNextEvent(){
        return toDo.Dequeue();
    }
}
