using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkCompanionLogic : Creature
{
    //this sits on every companion and
    //1) at outset will listen for possibleEvents
    //2) returns getDecision when need to figure out what to do
    
    FrameworkEvent defaultEvent;//what to run in case no instructions... it will be just idle
    [SerializeField] List<FrameworkEvent> possibleEvents = new List<FrameworkEvent>();//using List instead of HashSet cuz List size is only 10, otherwise better to use HashSet
    [SerializeField] FrameworkEvent currentEvent;
    Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    Player player;
    public IEnumerator currentCoroutine;
    Rigidbody rb;
    
    protected override void Awake(){
        base.Awake();
        player = GameObject.FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody>();
        player.OnTeach += Learn;//tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
        //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
    }

    protected override void Start(){
        base.Start();
    }

    void Learn(){ 
        if (toDo.Count < 3){//companion will only learn the first 10 FrameworkEvents a player performs
            toDo.Enqueue(player.CurrentEvent.Clone());
        } else if (currentEvent == null){
            GetDecision();
        }
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    public void GetDecision(){
        currentEvent = null;
        FrameworkEvent nextEvent = toDo.Dequeue();
        Debug.Log("Dequeued " + nextEvent);
        currentEvent = nextEvent;
        PerformDecision(nextEvent);
    }

    void PerformDecision(FrameworkEvent nextEvent){
        if (Target == null){
            Target = FindClosestObjectOfLayer(nextEvent.TargetLayer);
        }
        if (Target != null){
            Target = FindClosestObjectOfLayer(nextEvent.TargetLayer);//check again if something closer popped up
            StartCoroutine(FaceTarget());//face new target
            TargetDist = GetTargetDist(Target.transform.position);
            if (nextEvent.CheckRange(this)){
                if (nextEvent.PerformEvent(this)){
                    Debug.Log(nextEvent + " succeeded");
                } else {
                    Debug.Log(nextEvent + " failed");
                }
                toDo.Enqueue(currentEvent);
                Debug.Log("Requeued " + currentEvent);
                Invoke("GetDecision",.5f);
            } else {
                StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
            }
        } else {
            Invoke("RepeatDecision",1f);//repeat this (keep trying to find a target)
        }
    }

    void RepeatDecision(){
        PerformDecision(currentEvent);
    }

    protected IEnumerator FaceTarget(float turnSpeed = .03f){
        Vector3 dir = (Target.transform.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
        int counter = 0;
        while (counter<50){
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
            counter++;
            yield return null;
        }
    }

    protected IEnumerator Movement(Vector3 dir){
        Vector3 rotCheck = transform.rotation.eulerAngles - Quaternion.LookRotation(new Vector3(dir.x,0,dir.z)).eulerAngles;
        Debug.Log(rotCheck);
        if (rotCheck.y > 5 || rotCheck.y < -5){//this is probably so stupidly expensive
            StartCoroutine(FaceTarget());
        }

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

    //checking if we need to move in order to fulfill next FrameworkEvent in the queue
    void MovementCheck(){
        FrameworkEvent nextEvent = toDo.Peek();
        if (nextEvent != null && nextEvent.MyRequiredRange != FrameworkGameStateVector.Range.None){//if next Event requires me to be in range 
            //check if I am in distance of said range
            //if not, add move to the queue and perform it as a coroutine

        }

    }

    Queue<FrameworkEvent> FrameWorkEventsToDo(){
        toDo = new Queue<FrameworkEvent>();
        return toDo;
    }

    public FrameworkEvent GetNextEvent(){
        return toDo.Dequeue();
    }
}
