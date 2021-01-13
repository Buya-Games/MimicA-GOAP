using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    protected GameManager manager;
    protected Animator anim;
    public Stats MyStats;
    [HideInInspector] public GameObject Target;
    [HideInInspector] public float TargetDist;
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
        anim = GetComponentInChildren<Animator>();
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
        //anim.SetTrigger("Swing");
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
}
