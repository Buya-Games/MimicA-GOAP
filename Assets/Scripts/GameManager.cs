using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public PlayerControl player;
    [HideInInspector] public Spawner spawner;
    [HideInInspector] public UI ui;
    int collectedFungus;
    [SerializeField] public List<GameState.State> CurrentState = new List<GameState.State>();
    [HideInInspector] public ParticleManager particles;
    [HideInInspector] public Cow cow;
    public bool debug;
    [SerializeField] CinemachineVirtualCamera vcam;
    

    //Awake is called before Start
    void Awake(){
        player = FindObjectOfType<PlayerControl>();
        spawner = GetComponent<Spawner>();
        ui = GetComponent<UI>();
        particles = GetComponent<ParticleManager>();
        cow = FindObjectOfType<Cow>();
        vcam = vcam.GetComponent<CinemachineVirtualCamera>();
        CurrentState.Add(GameState.State.playerAlive);
    }

    //Start is called before first frame
    void Start(){
        StartCoroutine(SpawnStuff());
        StartCoroutine(CheckStuff());
        //StartCoroutine(Countdown());
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
        //int enemyCounter = 10;
        while (counter < 1000){
            // if (counter > enemyCounter){
            //     enemyCounter+=Random.Range(5,15);
            //     SpawnEnemy(1);
            // }
            counter++;
            SpawnEnvironment(1,Spawner.EnvironmentType.Bush);
            SpawnEnvironment(1,Spawner.EnvironmentType.Mushroom);
            yield return new WaitForSeconds(Random.Range(15,25));
        }
    }

    //running this slowly in the backgrond to catch all the stupid bugs I made
    IEnumerator CheckStuff(){
        int counter = 0;
        while (counter < 1000){
            foreach (GameObject berry in spawner.ActiveBerries){
                if (!berry.activeSelf){
                    spawner.ActiveBerries.Remove(berry);
                }
            }
            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator Countdown(){
        int counter = 10000;
        while (counter > 0){
            counter--;
            ui.textTimer.text = counter.ToString();
            yield return null;
        }
        Destroy(player.gameObject);
    }

    public void PlayerDeath(){
        player.gameObject.SetActive(false);
        StartCoroutine(DestroyPlayer());
        vcam.m_Follow = cow.transform;
    }

    IEnumerator DestroyPlayer(){
        yield return new WaitForSeconds(1);
        Destroy(player.gameObject);
    }

    public void GameOver(bool win = true){
        if (win){
            Debug.Log("you win!");
        } else {
            Debug.Log("you lost");
        }
    }

}
