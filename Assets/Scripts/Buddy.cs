using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this sits on every AI companion and
//1) at outset will watch player to learn goals and actions
//2) after X actions, will use GOAPPlan to dynamically create action plans to meet goals
public class Buddy : Goblin
{
    
    [SerializeField] FrameworkEvent currentEvent;
    
    Player player;
    [HideInInspector]public bool learning;
    
    float eventMaxTime = 10; //don't spend more than 10 seconds on any given decision
    Queue<GameState.State> MyGoals = new Queue<GameState.State>();
    Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    int learningActions = 5;//# of player actions that creature will observe to learn, then start mimicing
    [HideInInspector] public float motiveReproduction, motiveHarvest, motiveAttack;//the 3 possible goals of a creature

    protected override void Awake(){
        base.Awake();
    }
    protected override void Start(){
        base.Start();
        Init();
        player = GameObject.FindObjectOfType<Player>();
        manager.spawner.ActiveBuddies.Add(this.gameObject);//##REMOVE THIS AFTER YOU FINISH TESTING AND GAME STARTS WITH 0 BUDDIES
        learning = true;
        player.CheckForStudents();
        player.OnTeach += Learn;//tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
        //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
    }

    void Learn(){
        if (availableActions.Count<learningActions){//listen until X actions
            availableActions.Add(player.CurrentEvent.Clone());
            motiveAttack+=player.CurrentEvent.motiveAttack;
            motiveHarvest+=player.CurrentEvent.motiveHarvest;
            motiveReproduction+=player.CurrentEvent.motiveReproduction;
        }
        if (availableActions.Count >= learningActions){//after X, we will stop listening and setup our lifetime goals and GET ON WITH OUR LIVES!
            SetGoals();
            learning = false;
            player.OnTeach -= Learn;//turning off listener
            player.CheckForStudents();//telling player to stop teaching unless other students
            GetPlan();
        }
    }

    void SetGoals(){
        MyGoals.Clear();
        if (motiveAttack>motiveHarvest){//i'm sure more elegant way to do this, but just grinding thru it guh
            if (motiveAttack>motiveReproduction){
                MyGoals.Enqueue(GameState.State.goalAttacked);
            } else if (motiveAttack==motiveReproduction){
                MyGoals.Enqueue(GameState.State.goalReproduced);
                MyGoals.Enqueue(GameState.State.goalAttacked);
            } else {
                MyGoals.Enqueue(GameState.State.goalReproduced);
            }
        } else if (motiveAttack==motiveHarvest && motiveAttack>motiveReproduction){
            MyGoals.Enqueue(GameState.State.goalHarvested);
            MyGoals.Enqueue(GameState.State.goalAttacked);
        } else if (motiveHarvest>motiveReproduction){
            MyGoals.Enqueue(GameState.State.goalHarvested);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest>motiveAttack) {
            MyGoals.Enqueue(GameState.State.goalHarvested);
            MyGoals.Enqueue(GameState.State.goalReproduced);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest==motiveAttack) {
            MyGoals.Enqueue(GameState.State.goalHarvested);
            MyGoals.Enqueue(GameState.State.goalReproduced);
            MyGoals.Enqueue(GameState.State.goalAttacked);
        } else {
            MyGoals.Enqueue(GameState.State.goalReproduced);
        }
        Debug.Log(motiveAttack + " " + motiveHarvest + " " + motiveReproduction + ", " + MyGoals.Peek());
    }

    List<GameState.State> ToList(Queue<GameState.State> q){
        List<GameState.State> toList = new List<GameState.State>();
        GameState.State s = q.Dequeue();
        toList.Add(s);
        q.Enqueue(s);
        return toList;
    }

    public void GetPlan(){
        //Debug.Log(Time.time + " running Plan");
        toDo = planner.MakePlan(this,GetCurrentState(),ToList(MyGoals));

        if (toDo == null){ //if failed to find a plan, try again in 1 second
            //Debug.Log(Time.time + ": didn't find a plan so invoking Plan again");
            Invoke("GetPlan",1f);
        } else {
            //Debug.Log(Time.time + " we have a " + toDo.Count + " point plan!");
            GetDecision();
        }
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    public void GetDecision(){
        currentEvent = null;
        if (toDo.Count>0){
            currentEvent = toDo.Dequeue();
            PerformDecision(currentEvent);
        } else {
            //Debug.Log("oop, need a new plan");
            GetPlan();
        }
    }

    void PerformDecision(FrameworkEvent nextEvent){
        if (Time.time > eventMaxTime){//should we really just give up? what if still moving to X?
            eventMaxTime+=Time.time;
            GetDecision();
        } else {
            if (nextEvent.GetTarget(this)){
                if (nextEvent.CheckRange(this)){
                    StartCoroutine(FaceTarget((Target.transform.position - transform.position).normalized));
                    if (nextEvent.PerformEvent(this)){
                        //Debug.Log(nextEvent + " SUCCEEDED");
                    } else {
                        Debug.Log(nextEvent + " FAILED");
                    }
                    Invoke("GetDecision",.5f);
                } else {
                    StopAllCoroutines();
                    StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                }
            } else {
                Invoke("GetPlan",1f);//if can't find any targets, get a new plan
            }
        }
    }

    void LookAtLocation(Vector3 where){
        StartCoroutine(FaceTarget(where));
    }

    IEnumerator Movement(Vector3 dir){
        // Vector3 rotCheck = transform.rotation.eulerAngles - Quaternion.LookRotation(new Vector3(dir.x,0,dir.z)).eulerAngles;
        // if (rotCheck.y > 5 || rotCheck.y < -5){//this is probably so stupidly expensive
        //     StartCoroutine(FaceTarget(Target.transform.position));
        // }
        StartCoroutine(FaceTarget(dir));
        int counter = 0;
        rb.velocity = Vector3.zero;
        while (counter < 50){
            rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed/2);
            counter++;
            yield return null;
        }
        PostMovementChecks();
    }

    void PostMovementChecks(){
        Queue<FrameworkEvent> quickCheck = planner.MakePlan(this,GetCurrentState(),ToList(MyGoals));
        if (currentEvent != null && quickCheck.Peek() == currentEvent){
            //Debug.Log("yup, plan is still good so will do " + currentEvent);
            if (currentEvent.CheckPreconditions(GetCurrentState())){
                //Debug.Log("and " + currentEvent + "prechecks are good too so will do it");
                PerformDecision(currentEvent);
            } else {
                //Debug.Log("can no longer perform " + currentEvent + ", finding new plan");
                Invoke("GetPlan",0.5f); //seems can no longer perform the current event, so getting a new plan
            }
        } else {
            toDo = quickCheck;
            //Debug.Log("oh plan changed from " + currentEvent + " to " + toDo.Peek());
            GetDecision();
        }
    }
}