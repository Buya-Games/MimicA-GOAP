using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    protected GameManager manager;
    public Stats MyStats;
    [HideInInspector] public GameObject Target;
    [HideInInspector] public float TargetDist;
    [HideInInspector] public GameObject Cow;
    public GameObject HeldItem;
    [SerializeField] protected Transform melee;

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

    protected float GetTargetDist(Vector3 targetPos){
        return Mathf.Abs(Vector3.Distance(transform.position,targetPos));
    }

    protected GameObject FindClosestObjectOfLayer(int targetLayer){
        List<GameObject> possibleTargets = new List<GameObject>();
        if (targetLayer == 7){
            possibleTargets = manager.spawner.ActiveBerries;
        }
        if (targetLayer == 8){
            possibleTargets = manager.spawner.ActivesBushes;
        }
        if (targetLayer == 10){
            possibleTargets = manager.spawner.ActiveBombs;
        }
    
        GameObject closest = null;
        float dist = Mathf.Infinity;
        if (possibleTargets.Count>0){
            foreach (GameObject b in possibleTargets)
            {
                //if first set it as closest
                if (closest == null){
                    closest = b;
                    dist = Mathf.Abs(Vector3.Distance(closest.transform.position, this.transform.position));
                } else { //else check if closer
                    float distThis = Mathf.Abs(Vector3.Distance(b.transform.position, this.transform.position));
                    if (distThis < dist){
                        closest = b;
                        dist = distThis;
                    }
                }
            }
        }
        return closest;
    }

    protected IEnumerator FaceTarget(float turnSpeed = .03f){
        Vector3 dir = (Target.transform.position - transform.position).normalized;
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
}
