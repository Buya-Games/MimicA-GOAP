using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//base class for stats, moving, swinging melee
public class Creature : MonoBehaviour, IHittable
{
    protected GameManager manager;
    protected Rigidbody rb;
    protected GOAPPlan planner;
    public Stats MyStats;
    public GameObject Target;
    public GameObject HeldItem;
    [SerializeField] protected Transform melee;//melee attacking "arm"
    [HideInInspector] public float health = 10;
    [SerializeField] protected float stamina;
    string myName;
    public List<FrameworkEvent> availableActions = new List<FrameworkEvent>();
    protected Queue<GameState.State> myGoals = new Queue<GameState.State>();
    protected Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    protected FrameworkEvent currentEvent;
    [SerializeField] protected TMP_Text myText;
    float eventMaxTime = 10; //don't spend more than 10 seconds on any given decision

    [System.Serializable]
    public class Stats {
        public float Strength;//melee strength
        public float Speed;//movement speed
        public float Range;//distance you can throw shit
        public float Accuracy;//subtract this from RangeAttack.RangeRadius to determine where bomb will and around target
        public float FoodHarvestSkill, BombHarvestSkill;
    }

    protected virtual void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        planner = FindObjectOfType<GOAPPlan>();
    }

    protected virtual void Start(){
        melee.gameObject.SetActive(false);
    }

    public virtual void Init(){
        health = 10;
        stamina = 10;
        SetStats();
        myName = NameGenerator.CreateRandomName();
        gameObject.name = myName;

        //give everyone basic skills to eat
        availableActions.Add(new Eat());//eat berry
        availableActions.Add(new MeleeAttack(6));//harvest berries from bushes
        availableActions.Add(new PickupItem(7));//harvest berries from bushes
    }

    void SetStats(){
        MyStats = new Stats();
        MyStats.Strength = 1;
        MyStats.Speed = 2;
        MyStats.Range = 10;
        MyStats.Accuracy = 1;
        MyStats.FoodHarvestSkill = 1;
        MyStats.BombHarvestSkill = 1;
    }

    protected List<GameState.State> GetCurrentState(){
        List<GameState.State> currentState = new List<GameState.State>();
        if (HeldItem == null){
            currentState.Add(GameState.State.itemNone);
        } else if (HeldItem.layer == 7){//if holding berry
            currentState.Add(GameState.State.itemBerry);
        } else if (HeldItem.layer == 9){//if holding fungus
            currentState.Add(GameState.State.itemFungus);
        } else if (HeldItem.layer == 10){//if holding bomb
            currentState.Add(GameState.State.itemBomb);
        } 
        currentState.AddRange(manager.CurrentState);
        return currentState;
    }

    protected virtual void GetPlan(){
        currentEvent = null;
        myText.text = "";
        toDo = planner.MakePlan(this,GetCurrentState(),HungryCheck());
        if (toDo == null){ //if failed to find a plan
            Invoke("Idle",Random.Range(1f,2f));
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
            PerformDecision(currentEvent);
        } else {
            GetPlan();
        }
    }

    protected virtual void PerformDecision(FrameworkEvent nextEvent){
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
                    Invoke("GetDecision",Random.Range(0.4f,.6f));
                } else {
                    StopAllCoroutines();
                    StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                }
            } else {
                Invoke("GetPlan",Random.Range(0.8f,1.2f));//if can't find any targets, get a new plan
            }
        }
    }

    public void Swing(){
        StopCoroutine(GhettoAnimations.AnimSwing(melee));
        StartCoroutine(GhettoAnimations.AnimSwing(melee));
    }

    protected List<GameState.State> HungryCheck(){
        List<GameState.State> whatToDo = new List<GameState.State>();
        if (stamina<5){
            // if (stamina<-10){
            //     Die();
            //     return null;
            // }
            whatToDo.Add(GameState.State.goalEat);
        } else {
            GameState.State goal = myGoals.Dequeue();
            whatToDo.Add(goal);//just adding one goal at a time... passing them all would be more ideal
            myGoals.Enqueue(goal);
        }
        return whatToDo;
    }

    public void Eat(){
        stamina+=20;
        HeldItem=null;
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 600);
        
        health-=damage;
        if (health <= 0){
            Die();
        }
    }

    protected void Idle(){
        myText.text = "Idle";
        StartCoroutine(Movement(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))));
    }

    void Die(){
        Debug.Log(myName + " has died");
        manager.spawner.DespawnCreature(this.gameObject);
    }

    protected IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .06f){
        // Vector3 dir = (lookLocation - transform.position).normalized;
        // if (dir != Vector3.up){
            //Debug.Log(dir);
            Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x+.001f,0,dir.z));
            int counter = 0;
            while (counter<50){
                transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
                counter++;
                yield return null;
            }
        //}
    }

    protected IEnumerator Movement(Vector3 dir){
        StartCoroutine(FaceTarget(dir));
        int counter = 0;
        rb.velocity = Vector3.zero;
        while (counter < 100){
            stamina-=0.01f;
            rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed);
            counter++;
            yield return null;
        }
        PostMovementChecks();
    }

    protected virtual void PostMovementChecks(){
        Queue<FrameworkEvent> quickCheck = planner.MakePlan(this,GetCurrentState(),HungryCheck());
        if (quickCheck != null && currentEvent != null && quickCheck.Peek() == currentEvent){ //checks if currentstate results in same currentevent
            if (currentEvent.CheckPreconditions(GetCurrentState())){//checks that currentevent can still be performed
                PerformDecision(currentEvent);//keeps performing currentevent
            } else {
                Invoke("GetPlan",Random.Range(0.4f,.6f)); //seems can no longer perform the currentevent, so getting a new plan
            }
        } else {
            toDo = quickCheck;//seems there's a new better plan available, so abandon existing plan
            Invoke("GetDecision",Random.Range(0.4f,.6f));
        }
    }

}
