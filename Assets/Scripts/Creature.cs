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
    protected MaterialPropertyBlock matBlock;
    protected Transform mainCamera;
    public float health;
    protected string myName;
    //[SerializeField] protected TMP_Text myText;
    public MeshRenderer visibleMesh;
    protected float bobSpeed;
    [SerializeField] protected Transform head;
    public bool alive = true;
    bool enemy = false;
    bool cow = false;
    Material origMat;
    TrailRenderer trail;

    protected virtual void Awake(){
        if (this is Enemy){
            enemy = true;
        }
        if (this is Cow){
            cow = true;
        }
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        healthBar = healthBar.GetComponent<MeshRenderer>();
        matBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main.transform;
        visibleMesh = visibleMesh.GetComponent<MeshRenderer>();
        origMat = visibleMesh.material;
        if (!cow){
            trail = GetComponentInChildren<TrailRenderer>();
            melee.gameObject.SetActive(false);
        }
    }

    public virtual void Init(){
        health = 100;
        SetStats();
        myName = NameGenerator.CreateRandomName();
        if (this is Enemy){
            myName = myName + " (enemy)";
        }
        gameObject.name = myName;
        if (this is Buddy){
            manager.ui.DisplayMessage(transform.position,myName + " was born!",Color.white);
        }

        bobSpeed = MyStats.Speed * 7;
    }

    void SetStats(){
        MyStats = new Stats();
        MyStats.Strength = 1;
        MyStats.Speed = Random.Range(1.9f,2.1f);//just to give everything a bit variability
        MyStats.Range = 15;
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
            if (!cow){
                //mesh bobs up/down (collider doesn't move)
                Vector3 bobPos = visibleMesh.transform.localPosition + transform.up * Mathf.Sin(Time.time * bobSpeed) * 0.015f;
                bobPos.y = Mathf.Clamp(bobPos.y,0,.7f);
                visibleMesh.transform.localPosition = bobPos;
                if (manager.GameLive){
                    health-=.01f;
                }
            }
            
            UpdateHealth();
            if (health <= 0){
                Die();
            }
        }
    }

    protected virtual void UpdateHealth(){
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
        if (!enemy){
            if (Target != null){
                ITargettable oldTarget = Target.GetComponent<ITargettable>();
                if (oldTarget != null){
                    oldTarget.NotTargeted();
                }
            }
        }

        Target = targetedObj;
        if (!enemy){
            ITargettable newTarget = Target.GetComponent<ITargettable>();
            if (newTarget != null){
                newTarget.Targeted(gameObject);
            }
        }
    }

    public void ClearTarget(){
        Target = null;
    }

    public void Swing(int strength = 1){
        if (strength > 1){
            manager.audioManager.PlaySound("swing",0,.7f,2f);
            trail.colorGradient = manager.swingHard;
        } else {
            trail.colorGradient = manager.swingNormal;
            manager.audioManager.PlaySound("swing",0,.7f,Random.Range(.9f,1.1f));
        }
        StopCoroutine(GhettoAnimations.AnimSwing(melee));
        StartCoroutine(GhettoAnimations.AnimSwing(melee));
        
    }

    public virtual void PickUp(IThrowable item){
        if (HeldItem == null){
            HeldItem = item.ThisGameObject();
            manager.audioManager.PlaySound("pickup",0,1,Random.Range(.9f,1.1f));
        }
    }

    public void DropItem(){
        if (HeldItem != null){
            HeldItem.GetComponent<Item>().Drop();
            HeldItem = null;
        }
    }

    public void Eat(Spawner.EnvironmentType type){
        float eatBenefit = 0;
        if (type == Spawner.EnvironmentType.Berry){
            eatBenefit = 50;
            manager.particles.EatingBerry(HeldItem.transform.position);
        }
        if (type == Spawner.EnvironmentType.Fungus){
            eatBenefit = 25;
            manager.particles.EatingFungus(HeldItem.transform.position);
        }
        if (type == Spawner.EnvironmentType.BerryPoop || type == Spawner.EnvironmentType.FungusPoop){
            eatBenefit = 10;
            manager.particles.BombExplosion(HeldItem.transform.position);
        }
        health = Mathf.Clamp(health + eatBenefit,10,100);
        HeldItem.GetComponent<Item>().Drop();//not using DropItem cuz it causes a conflict.... it's a turd i know i need to clean it up
        manager.spawner.DespawnEnvironment(HeldItem,type);
        HeldItem = null;
        manager.ui.DisplayMessage(transform.position,"+" + eatBenefit.ToString("F0"),Color.white);
        manager.audioManager.PlaySound("eat",0,1,Random.Range(.9f,1.1f));
    }

    public void TakeHit(GameObject attacker, float damage){
        health -=damage;
        // Vector3 dir = (transform.position - attacker.transform.position).normalized; //pushes me in direction opposite to attacker
        // dir.y = 0;
        // rb.AddForce(dir * 600);

        //flashes red to indicate a hit
        visibleMesh.material = manager.HitMaterial;
        StartCoroutine(EndHit());
        manager.audioManager.PlaySound("melee",0,.6f,Random.Range(.9f,1.1f));
    }

    IEnumerator EndHit(){
        yield return new WaitForSeconds(0.08f);
        visibleMesh.material = origMat;
    }

    protected void Idle(){
        //myText.text = "Idle";
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
        //myText.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        manager.spawner.DespawnCreature(this.gameObject);
    }

    protected virtual IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .06f){
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x+.001f,0,dir.z));
        int counter = 0;
        while (counter<50){
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
            counter++;
            yield return null;
        }
    }

    protected virtual IEnumerator Movement(Vector3 dir){
        if (alive){
            StartCoroutine(FaceTarget(dir));
            int counter = 0;
            rb.velocity = Vector3.zero;
            while (counter < 100){
                rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed);
                counter++;

                yield return null;
            }
            PostMovementChecks();
        }
    }

    protected virtual void PostMovementChecks(){
        //i dont know why this happens sometimes, but AI gets stuck with deactivated items as its HeldItem or Target
        if (HeldItem != null){
            if (!HeldItem.activeSelf){
                DropItem();
            }
        }
        if (Target != null){
            if (!Target.activeSelf){
                ClearTarget();
            }
        }
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
