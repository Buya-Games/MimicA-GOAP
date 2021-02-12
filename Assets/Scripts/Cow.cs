using System.Collections;
using UnityEngine;

//makes the cow walk around randomly, handles eating and pooping, and moves the entire play area
public class Cow : Creature //inherit from Creature for TakeHit and a few funcs, but maybe better just to be self
{
    [SerializeField] Transform poopPos;
    [SerializeField] MeshRenderer birthBar;//to indicate how much until birthing buddy
    MaterialPropertyBlock matBlockBirth;
    float eatenFungus = 0;
    [SerializeField] Transform ground;

    protected override void Awake(){
        base.Awake();
        birthBar = birthBar.GetComponent<MeshRenderer>();
        matBlockBirth = new MaterialPropertyBlock();

        birthBar.GetPropertyBlock(matBlockBirth);
        matBlockBirth.SetFloat("_Fill", Mathf.Clamp(eatenFungus/100,0,1));
        birthBar.SetPropertyBlock(matBlockBirth);

        health = 100;
        Invoke("PostMovementChecks",1f);

    }

    protected override IEnumerator Movement(Vector3 dir){//i don't even inherit this from Creature but i should
        dir.Normalize();
        StartCoroutine(FaceTarget(dir));
        int counter = Random.Range(1000,2000);
        while (counter>0){
            transform.position = transform.position + dir * (Time.fixedDeltaTime * .5f);
            counter--;
            yield return null;
        }

        //ground will follow the cow so we don't fall edge of map
        ground.position = new Vector3(transform.position.x,0,transform.position.z);
        PostMovementChecks();
    }

    protected override void PostMovementChecks()
    {
        float moveLimit = 50;
        Vector3 movePos = new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f));
        if (transform.position.x>moveLimit){
            movePos.x = Random.Range(-.8f,-1f);
        }
        if (transform.position.x<-moveLimit){
            movePos.x = Random.Range(.8f,1);
        }
        if (transform.position.z>moveLimit){
            movePos.z = Random.Range(-.8f,-1f);
        }
        if (transform.position.z<-moveLimit){
            movePos.z = Random.Range(.8f,1);
        }
        StartCoroutine(Movement(movePos));//just loops inifinitely
    }

    protected override IEnumerator FaceTarget(Vector3 dir, float turnSpeed = .01f){//again, doesn't inherit from Creature which it should
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x,0,dir.z));
        int counter = 0;
        while (counter<250){
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,turnSpeed);
            UpdateHealth();
            counter++;
            yield return null;
        }
    }

    protected override void UpdateHealth(){
        base.UpdateHealth();
        if (mainCamera != null) {
            var forward = healthBar.transform.position - mainCamera.transform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, mainCamera.transform.right);
            birthBar.transform.rotation = Quaternion.LookRotation(forward, up);
        }

    }

    void EatFungus(GameObject fungus){
        manager.spawner.DespawnEnvironment(fungus,Spawner.EnvironmentType.Fungus);
        manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.FungusPoop);
        manager.particles.EatingFungus(fungus.transform.position);
        health = Mathf.Clamp(health + 1,1,100);

        eatenFungus++;
        birthBar.GetPropertyBlock(matBlockBirth);
        matBlockBirth.SetFloat("_Fill", Mathf.Clamp(eatenFungus/10,0,1));
        birthBar.SetPropertyBlock(matBlockBirth);

        //tutorial shit
        if (manager.Tutorial && manager.tut.Tut6FeedShroomCow){
            manager.tut.Tut6FeedShroomCow = false;
            manager.tut.Tut7GiveBirth = true;
            manager.tut.DisplayNextTip(7);//do it 9 more times
        }
        //end tutorial shit

        if (eatenFungus >= manager.ShroomsForBirth){ //once you've eaten 10 fungus you poop out a new buddy
            eatenFungus=0;
            FindObjectOfType<Spawner>().SpawnCreature(poopPos.position);
            
            //tutorial shit
            if (manager.Tutorial && manager.tut.Tut7GiveBirth){
                manager.tut.Tut7GiveBirth = false;
                manager.tut.Tut8TeachAny = true;
                manager.tut.DisplayNextTip(8);//congrats teach them anything
            }
            //end tutorial shit
        }
        manager.audioManager.PlaySound("eat",0,1,Random.Range(.9f,1.1f));
    }

    void EatBerry(GameObject berry){
        manager.spawner.DespawnEnvironment(berry,Spawner.EnvironmentType.Berry);
        manager.spawner.SpawnEnvironment(poopPos.position,Spawner.EnvironmentType.BerryPoop);
        manager.particles.EatingBerry(berry.transform.position);
        manager.audioManager.PlaySound("eat",0,1,Random.Range(.9f,1.1f));
        health = Mathf.Clamp(health + 5,5,100);//cow gets a bit of health from eating

        //tutorial shit
        if (manager.Tutorial && manager.tut.Tut4ThrowBerryCow){
            manager.tut.Tut4ThrowBerryCow = false;
            manager.tut.Tut5ThrowPoop = true;
            manager.tut.DisplayNextTip(5);//throw poop to ground
        }
        //end tutorial shit
    }

    protected override void Die(){
        alive = false;
        StopAllCoroutines();
        manager.GameOver(false);
        manager.particles.DestroyingCow(transform.position);
        gameObject.SetActive(false);
    }

    //this is how cow detects berry/mushroom collisions
    void OnCollisionEnter(Collision col){
        if (col.gameObject.layer == 7){ //if berry
            EatBerry(col.gameObject);
        }
        if (col.gameObject.layer == 9){ //if fungus
            EatFungus(col.gameObject);
        }
    }
}
