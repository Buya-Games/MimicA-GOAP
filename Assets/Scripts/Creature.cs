using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class for stats, moving, swinging melee
public class Creature : MonoBehaviour, IHittable
{
    protected GameManager manager;
    protected Rigidbody rb;
    protected GOAPPlan planner;
    public Stats MyStats;
    [HideInInspector] public GameObject Target;
    [HideInInspector] public GameObject HeldItem;
    [SerializeField] protected Transform melee;
    [HideInInspector] public float Health = 10;
    [SerializeField] public List<FrameworkEvent> availableActions = new List<FrameworkEvent>();
    string myName;

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

    protected virtual void Init(){
        Health = 10;
        SetStats();
        myName = NameGenerator.CreateRandomName();
        gameObject.name = name;
    }

    void SetStats(){
        MyStats = new Stats();
        MyStats.Strength = 1;
        MyStats.Speed = 5;
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

    public void Swing(){
        StopCoroutine(GhettoAnimations.AnimSwing(melee));
        StartCoroutine(GhettoAnimations.AnimSwing(melee));
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 600);
        
        Health-=damage;
        if (Health <= 0){
            Die();
        }
    }

    void Die(){
        Debug.Log(myName + " has died");
        manager.spawner.DespawnCreature(this.gameObject);
    }

    protected IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .06f){
        // Vector3 dir = (lookLocation - transform.position).normalized;
        // if (dir != Vector3.up){
            //Debug.Log(dir);
            Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
            int counter = 0;
            while (counter<50){
                transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
                counter++;
                yield return null;
            }
        //}
    }

}
