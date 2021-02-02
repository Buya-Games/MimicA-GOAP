using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PlayerControl player;
    [HideInInspector] public Spawner spawner;
    int collectedFungus;
    public List<GameState.State> CurrentState = new List<GameState.State>();
    [HideInInspector] public ParticleManager particles;
    [HideInInspector] public Cow cow;
    public bool debug;

    //Awake is called before Start
    void Awake(){
        player = FindObjectOfType<PlayerControl>();
        spawner = GetComponent<Spawner>();
        particles = GetComponent<ParticleManager>();
        cow = FindObjectOfType<Cow>();
    }

    //Start is called before first frame
    void Start(){
        StartCoroutine(SpawnStuff());
        spawner.SpawnCreature(player.transform.position);
    }

    void SpawnEnvironment(int howMany, Spawner.EnvironmentType type){
        for (int i = 0;i<howMany;i++){
            spawner.SpawnEnvironment(spawner.EmptyLocation(), type);
        }
    }

    void SpawnEnemy(int howMany){
        for (int i = 0;i<howMany;i++){
            int posNegX = Random.Range(0,2)*2-1;
            int posNegZ = Random.Range(0,2)*2-1;
            Vector3 spawnPos = cow.transform.position + new Vector3(Random.Range(15,25)*posNegX,0,Random.Range(15,20)*posNegZ);
            spawner.SpawnCreature(spawnPos, true);
        }
    }

    IEnumerator SpawnStuff(){
        int counter = 0;
        int enemyCounter = 10;
        while (counter < 1000){
            // if (counter > enemyCounter){
            //     enemyCounter+=Random.Range(5,15);
            //     SpawnEnemy(1);
            // }
            counter++;
            SpawnEnvironment(1,Spawner.EnvironmentType.Bush);
            SpawnEnvironment(1,Spawner.EnvironmentType.Mushroom);
            yield return new WaitForSeconds(Random.Range(15,30));
        }
    }

    public void GameOver(bool win = true){
        if (win){
            Debug.Log("you win!");
        } else {
            Debug.Log("you lost");
        }
    }
}
