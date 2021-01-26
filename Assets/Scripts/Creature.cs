using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    protected GameManager manager;
    public Stats MyStats;
    [HideInInspector] public GameObject Target;
    //[HideInInspector] public float TargetDist;
    [HideInInspector] public GameObject Cow;
    public GameObject HeldItem;
    [SerializeField] protected Transform melee;
    [SerializeField] public List<FrameworkEvent> availableActions = new List<FrameworkEvent>();
    public float motiveReproduction, motiveHarvest, motiveAttack;//the 3 possible goals of a creature

    [System.Serializable]
    public class Stats {
        public float Strength;
        public float Speed;
        public float Range;//required range to throw a bomb
        public float Accuracy;//subtract this from RangeAttack.RangeRadius to determine where bomb will and around target
        public float FoodHarvestSkill, BombHarvestSkill;
    }

    protected virtual void Awake(){
        manager = FindObjectOfType<GameManager>();
        Cow = FindObjectOfType<Cow>().gameObject;
        SetStats();
    }

    protected virtual void Start(){
        melee.gameObject.SetActive(false);
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

    public void TakeHit(float damage){

    }

    public void Swing(){
        StopCoroutine(SwingAnimation());
        StartCoroutine(SwingAnimation());
    }

    IEnumerator SwingAnimation(){
        float rot = -120;
        Vector3 meleeRot = new Vector3(0,rot,0);
        melee.gameObject.SetActive(true);
        while (rot <120){
            rot+=10;
            meleeRot = new Vector3(0,rot,0);
            melee.localRotation = Quaternion.Euler(meleeRot);
            yield return null;
        }
        melee.gameObject.SetActive(false);
    }

    // protected float GetTargetDist(Vector3 targetPos){
    //     return Mathf.Abs(Vector3.Distance(transform.position,targetPos));
    // }

    // protected GameObject FindClosestObjectOfLayer(int targetLayer){
    //     List<GameObject> possibleTargets = new List<GameObject>();
    //     if (targetLayer == 6){
    //         possibleTargets = manager.spawner.ActivesBushes;
    //     }
    //     if (targetLayer == 7){
    //         possibleTargets = manager.spawner.ActiveBerries;
    //     }
    //     if (targetLayer == 8){
    //         possibleTargets = manager.spawner.ActiveMushrooms;
    //     }
    //     if (targetLayer == 9){
    //         possibleTargets = manager.spawner.ActiveFungus;
    //     }
    //     if (targetLayer == 10){
    //         possibleTargets = manager.spawner.ActiveBombs;
    //     }
    //     Debug.Log("looking for closest among " + possibleTargets.Count + " targets");
    
    //     GameObject closest = null;
    //     float dist = Mathf.Infinity;
    //     if (possibleTargets.Count>0){
    //         foreach (GameObject b in possibleTargets){
    //             if (b.layer != 15){ //if object not flying in mid-air
    //                 //if first set it as closest
    //                 if (closest == null){
    //                     closest = b;
    //                     dist = Mathf.Abs(Vector3.Distance(closest.transform.position, this.transform.position));
    //                 } else { //else check if closer
    //                     float distThis = Mathf.Abs(Vector3.Distance(b.transform.position, this.transform.position));
    //                     if (distThis < dist){
    //                         closest = b;
    //                         dist = distThis;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     return closest;
    // }

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

    protected IEnumerator FaceTarget(Vector3 lookLocation, float turnSpeed = .03f){
        Vector3 dir = (lookLocation - transform.position).normalized;
        if (dir != Vector3.up){
            //Debug.Log(dir);
            Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
            int counter = 0;
            while (counter<50){
                transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
                counter++;
                yield return null;
            }
        }
    }

    protected Dictionary<string, bool> getWorldState(){
        Dictionary<string,bool> state = new Dictionary<string, bool>();
        return state;
    }
    protected Dictionary<string, bool> createGoalState(){
        Dictionary<string,bool> goal = new Dictionary<string, bool>();
        goal.Add("damagePlayer",true);
        goal.Add("stayAlive",true);
        return goal;
    }
}
