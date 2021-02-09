using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : MonoBehaviour, IHittable
{
    GameManager manager;
    [SerializeField] Transform poopPos;
    [HideInInspector]public float health = 100;
    [SerializeField] MeshRenderer healthBar;
    [SerializeField] MeshRenderer birthBar;
    MaterialPropertyBlock matBlockHealth;
    MaterialPropertyBlock matBlockBirth;
    Transform mainCamera;
    float eatenFungus = 0;
    Rigidbody rb;
    [SerializeField] Transform ground;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        healthBar = healthBar.GetComponent<MeshRenderer>();
        birthBar = birthBar.GetComponent<MeshRenderer>();
        matBlockHealth = new MaterialPropertyBlock();
        matBlockBirth = new MaterialPropertyBlock();
        mainCamera = Camera.main.transform;

        birthBar.GetPropertyBlock(matBlockBirth);
        matBlockBirth.SetFloat("_Fill", Mathf.Clamp(eatenFungus/100,0,1));
        birthBar.SetPropertyBlock(matBlockBirth);

        health = 100;

        Invoke("RandomWalk",1f);
    }

    void RandomWalk(){
        //StartCoroutine(MoveNearPlayer());
        StartCoroutine(MovingAround(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))));
        //StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
        //StartCoroutine(MovingAround(manager.spawner.EmptyNearbyLocation(manager.player.transform.position,5,15)));
    }

    IEnumerator MoveNearPlayer(){
        Vector3 target;
        if (manager.player != null){
            target = manager.player.transform.position + new Vector3(Random.Range(-10f,10f),0,Random.Range(-10,10f));
        } else {
            target = new Vector3(Random.Range(-10f,10f),0,Random.Range(-10,10f));
        }
        Vector3 dir = (target - transform.position).normalized;
        StartCoroutine(FaceTarget(dir));
        int counter = Random.Range(500,1000);
        while (counter>0){
            transform.position = transform.position + dir * (Time.fixedDeltaTime * .5f);
            counter--;
            yield return null;
        }
        RandomWalk();
    }

    IEnumerator MovingAround(Vector3 dir){
        dir.Normalize();
        StartCoroutine(FaceTarget(dir));
        int counter = Random.Range(500,1000);
        while (counter>0){
            transform.position = transform.position + dir * (Time.fixedDeltaTime * .5f);
            counter--;
            yield return null;
        }
        //ground will follow the cow so we never near edge of map
        ground.position = new Vector3(transform.position.x,0,transform.position.z);
        RandomWalk();
    }

    protected IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .01f){
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
        int counter = 0;
        while (counter<250){
            transform.rotation = Quaternion.Lerp(transform.rotation,rot,turnSpeed);
            UpdateBars();
            counter++;
            yield return null;
        }
    }

    public void TakeHit(GameObject attacker, float damage){
        Vector3 dir = (transform.position - attacker.transform.position).normalized; //take hit in direction opposite to attacker
        dir.y = 0;
        rb.AddForce(dir * 300);

        health-=damage;
        healthBar.GetPropertyBlock(matBlockHealth);
        matBlockHealth.SetFloat("_Fill", Mathf.Clamp(health/100,0,1));
        healthBar.SetPropertyBlock(matBlockHealth);

        if (health <= 0){
            manager.GameOver(false);
        }
    }

    void UpdateBars(){
        if (mainCamera != null) {
            var forward = healthBar.transform.position - mainCamera.transform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, mainCamera.transform.right);
            healthBar.transform.rotation = Quaternion.LookRotation(forward, up);
            birthBar.transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }

    void EatFungus(GameObject fungus){
        fungus.GetComponent<Item>().Drop();
        manager.spawner.DespawnEnvironment(fungus,Spawner.EnvironmentType.Fungus);
        manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.FungusPoop);
        manager.particles.EatingFungus(fungus.transform.position);

        eatenFungus++;
        birthBar.GetPropertyBlock(matBlockBirth);
        matBlockBirth.SetFloat("_Fill", Mathf.Clamp(eatenFungus/10,0,1));
        birthBar.SetPropertyBlock(matBlockBirth);

        if (eatenFungus >= 10){ //once you've eaten 10 fungus you poop out a new buddy
            eatenFungus=0;
            FindObjectOfType<Spawner>().SpawnCreature(poopPos.position);
        }
    }

    void EatBerry(GameObject berry){
        berry.GetComponent<Item>().Drop();
        manager.spawner.DespawnEnvironment(berry,Spawner.EnvironmentType.Berry);
        manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.BerryPoop);
        manager.particles.EatingBerry(berry.transform.position);
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.layer == 7){ //if berry
            EatBerry(col.gameObject);
        }
        if (col.gameObject.layer == 9){ //if fungus
            EatFungus(col.gameObject);
        }
        // if (col.gameObject.layer == 13 || col.gameObject.layer == 12){ //if human or companion
        //     Creature creature = col.gameObject.GetComponent<Creature>();
        //     if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 7){//holding a berry
                
        //         manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Berry);
        //         manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.BerryPoop);
        //     }
        //     if (creature.HeldItem != null && creature.HeldItem.gameObject.layer == 9){//holding a fungus
        //         manager.spawner.DespawnEnvironment(col.gameObject.GetComponent<Creature>().HeldItem.gameObject,Spawner.EnvironmentType.Fungus);
        //         manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.FungusPoop);
        //         manager.CollectFungus();
        //     }
        // }
    }
}
