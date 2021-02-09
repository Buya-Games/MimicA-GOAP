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
    [SerializeField] protected MeshRenderer healthBar; //healthbar is just a simple quad. we adjust its value via shader material in meshRenderer
    MaterialPropertyBlock matBlock;
    Transform mainCamera;
    [HideInInspector] public float health = 10;
    protected string myName;
    [SerializeField] protected TMP_Text myText;
    public Transform visibleMesh;
    protected float bobSpeed;
    [SerializeField] protected Transform head;
    public bool alive = true;


    protected virtual void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        healthBar = healthBar.GetComponent<MeshRenderer>();
        matBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main.transform;
        melee.gameObject.SetActive(false);
    }

    public virtual void Init(){
        health = 100;
        SetStats();
        myName = NameGenerator.CreateRandomName();
        gameObject.name = myName;
        if (this is Buddy){
            manager.ui.DisplayMessage(transform.position,myName + " was born!");
        }

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
        if (alive){
            Vector3 bobPos = visibleMesh.localPosition + transform.up * Mathf.Sin(Time.time * bobSpeed) * 0.015f;//mesh bobs up/down (collider doesn't move)
            bobPos.y = Mathf.Clamp(bobPos.y,0,.7f);
            visibleMesh.localPosition = bobPos;
            health-=.015f;
            UpdateHealth();
            if (health <= 0){
                Die();
            }
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

    public void SetTarget(GameObject targetedObj){
        if (Target != null){
            ITargettable oldTarget = Target.GetComponent<ITargettable>();
            if (oldTarget != null){
                oldTarget.NotTargeted();
            }
        }

        Target = targetedObj;
        ITargettable newTarget = Target.GetComponent<ITargettable>();
        if (newTarget != null){
            newTarget.Targeted(gameObject);
        }
    }

    public void ClearTarget(){
        Target = null;
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

    public void Eat(Spawner.EnvironmentType type){
        HeldItem.GetComponent<Item>().Drop();//not using DropItem cuz it causes a conflict.... it's a turd i know i need to clean it up
        if (type == Spawner.EnvironmentType.Berry){
            health = Mathf.Clamp(health + 50,50,100);
            manager.particles.EatingBerry(HeldItem.transform.position);
        }
        if (type == Spawner.EnvironmentType.Fungus){
            health = Mathf.Clamp(health + 25,50,100);
            manager.particles.EatingFungus(HeldItem.transform.position);
        }
        if (type == Spawner.EnvironmentType.BerryPoop || type == Spawner.EnvironmentType.FungusPoop){
            health = Mathf.Clamp(health + 10,0,100);
            manager.particles.BombExplosion(HeldItem.transform.position);
            
        }
        manager.spawner.DespawnEnvironment(HeldItem,type);
        HeldItem = null;
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 600);
    }

    protected void Idle(){
        myText.text = "Idle";
        StartCoroutine(Movement(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))));
    }

    protected virtual void Die(){
        if (HeldItem != null){
            DropItem();
        }
        if (Target != null){
            ClearTarget();
        }
        alive = false;
        StopAllCoroutines();
        Debug.Log(myName + " has died");
        myText.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        GhettoAnimations.FallOver(transform);
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
        if (alive){
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
    }

    protected virtual void PostMovementChecks(){
        //sometimes the bobbing clips thru floor/goes too high, so just resetting if it does
        // if (visibleMesh.localPosition.y > .7f || visibleMesh.localPosition.y < 0){
        //     visibleMesh.localPosition = new Vector3(0,0.25f,0);
        // }
        //i dont know why this happens sometimes, but AI gets stuck with deactivated items as its HeldItem or Target
        if (HeldItem != null){
            if (!HeldItem.activeSelf){
                HeldItem = null;
            }
        }
        if (Target != null){
            if (!Target.activeSelf){
                Target = null;
            }
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
