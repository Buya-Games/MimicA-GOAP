using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//base class for stats, moving, swinging melee
public class Creature : MonoBehaviour, IHittable
{
    protected GameManager manager;
    Rigidbody rb;
    public Stats MyStats;
    public GameObject Target;
    public GameObject HeldItem;
    [SerializeField] Transform melee;//melee attacking "arm"
    [SerializeField] MeshRenderer healthBar; //i dunno why I made healthbar as a 3D cube. This adjusts the value
    MaterialPropertyBlock matBlock;
    Transform mainCamera;
    [HideInInspector] public float health = 10;
    protected string myName;
    [SerializeField] protected TMP_Text myText;
    public Transform visibleMesh;
    protected float bobSpeed;

    protected virtual void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        healthBar = healthBar.GetComponent<MeshRenderer>();
        matBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main.transform;
    }

    protected virtual void Start(){
        melee.gameObject.SetActive(false);
    }

    public virtual void Init(){
        health = 100;
        SetStats();
        myName = NameGenerator.CreateRandomName();
        gameObject.name = myName;

        bobSpeed = MyStats.Speed * 7;
    }

    void SetStats(){
        MyStats = new Stats();
        MyStats.Strength = 1;
        MyStats.Speed = Random.Range(1.9f,2.1f);//just to give everything a bit variability
        MyStats.Range = 10;
        MyStats.Accuracy = 1;
        MyStats.HarvestSkill = 1;
    }

    protected List<GameState.State> GetCurrentState(){
        List<GameState.State> currentState = new List<GameState.State>();
        if (HeldItem == null){
            currentState.Add(GameState.State.itemNone);
        } else if (HeldItem.layer == 7){//if holding berry
            currentState.Add(GameState.State.itemBerry);
        } else if (HeldItem.layer == 9){//if holding fungus
            currentState.Add(GameState.State.itemFungus);
        } else if (HeldItem.layer == 10){//if holding berry poop
            currentState.Add(GameState.State.itemBerryPoop);
        } else if (HeldItem.layer == 16){//if holding fungus poop
            currentState.Add(GameState.State.itemFungusPoop);
        } 
        currentState.AddRange(manager.CurrentState);
        return currentState;
    }

    protected virtual void Update(){
        visibleMesh.localPosition = visibleMesh.localPosition + transform.up * Mathf.Sin(Time.time * bobSpeed) * 0.015f;//mesh bobs up/down (collider doesn't move)
        health-=.015f;
        UpdateHealth();
        if (health <= 0){
            Die();
        }
    }

    void UpdateHealth(){
        if (mainCamera != null) {
            var forward = healthBar.transform.position - mainCamera.transform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, mainCamera.transform.right);
            healthBar.transform.rotation = Quaternion.LookRotation(forward, up);
        }

        healthBar.GetPropertyBlock(matBlock);
        matBlock.SetFloat("_Fill", Mathf.Clamp(health/100,0,1));
        healthBar.SetPropertyBlock(matBlock);
    }

    public void Swing(){
        StopCoroutine(GhettoAnimations.AnimSwing(melee));
        StartCoroutine(GhettoAnimations.AnimSwing(melee));
    }

    public virtual void PickUp(IThrowable item){
        if (HeldItem == null){
            HeldItem = item.ThisGameObject();
        }
    }

    public void DropItem(){
        if (HeldItem != null){
            HeldItem.GetComponent<Item>().Drop();
            HeldItem = null;
        }
    }

    public void Eat(){
        health = Mathf.Clamp(health + 50,50,100);
        HeldItem=null;
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 600);
        
        // health-=damage;
        // if (health <= 0){
        //     Die();
        // }
    }

    protected void Idle(){
        myText.text = "Idle";
        StartCoroutine(Movement(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))));
    }

    void Die(){
        StopAllCoroutines();
        Debug.Log(myName + " has died");
        manager.spawner.DespawnCreature(this.gameObject);
    }

    protected IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .06f){
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x+.001f,0,dir.z));
        int counter = 0;
        while (counter<50){
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
            counter++;
            yield return null;
        }
    }

    protected IEnumerator Movement(Vector3 dir){
        StartCoroutine(FaceTarget(dir));
        int counter = 0;
        rb.velocity = Vector3.zero;
        while (counter < 100){
            rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed);
            counter++;

            // health-=.02f;
            // UpdateHealth();
            // if (health <= 0){
            //     Die();
            // }

            yield return null;
        }
        PostMovementChecks();
    }

    protected virtual void PostMovementChecks(){
        //sometimes the bobbing clips thru floor/goes too high, so just resetting if it does
        if (visibleMesh.localPosition.y > .7f || visibleMesh.position.y < 0){
            visibleMesh.localPosition = new Vector3(0,0.25f,0);
        }
        // if (visibleMesh.transform.position.y < 0){
        //     Vector3 rePos = visibleMesh.transform.position;
        //     rePos.y = 0;
        //     visibleMesh.transform.position = rePos;
        // } else if (visibleMesh.transform.position.y < 0){
        //     Vector3 rePos = visibleMesh.transform.position;
        //     rePos.y = 0.4f;
        //     visibleMesh.transform.position = rePos;
        // }
        
    }

    [System.Serializable]
    public class Stats {
        public float Strength;//melee strength
        public float Speed;//movement speed
        public float Range;//distance you can throw shit
        public float Accuracy;//subtract this from RangeAttack.RangeRadius to determine where bomb will and around target
        public float HarvestSkill;
    }

}
