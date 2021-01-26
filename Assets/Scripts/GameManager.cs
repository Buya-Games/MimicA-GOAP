using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    FrameworkGameData gameData;
    [HideInInspector]public int Food, Bombs;
    [HideInInspector]public List<Enemy> ActiveEnemies = new List<Enemy>();
    [HideInInspector]public List<Human> ActiveHumans = new List<Human>(); //includes player and companions
    [HideInInspector]public List<Human> ActiveCompanions = new List<Human>(); //includes player and companions
    PlayerControl player;
    [HideInInspector] public Spawner spawner;
    int collectedFungus;
    public List<GameState.State> CurrentState = new List<GameState.State>();

    //Awake is called before Start
    void Awake(){
        gameData = GetComponent<FrameworkGameData>();
        player = FindObjectOfType<PlayerControl>();
        spawner = GetComponent<Spawner>();
    }

    //Start is called before first frame
    void Start(){
        ActiveHumans.Add(player.GetComponent<Human>());
        StartCoroutine(SpawnBushes());
        spawner.SpawnCompanion(player.transform.position);
        //SpawnEnvironment(5, Spawner.EnvironmentType.Tree);
        //SpawnEnvironment(10, Spawner.EnvironmentType.Bush);
        //SpawnEnvironment(3, Spawner.EnvironmentType.Rock);
    }

    void SpawnEnvironment(int howMany, Spawner.EnvironmentType type){
        for (int i = 0;i<howMany;i++){
            int posNeg = Random.Range(0,2)*2-1;
            Vector3 spawnPos = new Vector3(Random.Range(0,20)*posNeg,0,player.transform.position.z + Random.Range(5,25));
            //Vector3 spawnPos = new Vector3(Random.Range(15,50)*posNeg,0,player.transform.position.z + Random.Range(5,20));
            spawner.SpawnEnvironment(spawnPos, type);
        }
    }

    IEnumerator SpawnBushes(){
        int counter = 0;
        while (counter < 100){
            counter++;
            SpawnEnvironment(1,Spawner.EnvironmentType.Bush);
            SpawnEnvironment(1,Spawner.EnvironmentType.Mushroom);
            yield return new WaitForSeconds(3);
        }
    }

    public void CollectFungus(){
        collectedFungus++;
        if (collectedFungus>10){ //if you collect 10 fungus
            collectedFungus-=10;
            Vector3 spawnPos = player.transform.position;
            spawnPos.z-=5;
            FindObjectOfType<Spawner>().SpawnCompanion(spawnPos); //a new companion will be created by the player
        }
    }

    // public void UpdateBombs(int howMany){
    //     Bombs+=howMany;
    //     if (Bombs < 3){
    //         gameData.CurrentState.isLowAmmo = true;
    //     } else {
    //         gameData.CurrentState.isLowAmmo = false;
    //     }   
    // }
    // public void UpdateFood(int howMany){
    //     Food+=howMany;
    //     if (Food < 10){
    //         gameData.CurrentState.isLowFood = true;
    //     } else {
    //         gameData.CurrentState.isLowFood = false;
    //     }   
    // }
}
