using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

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
    public Gradient HeadColor;
    [SerializeField] int playerDeathScore;
    [HideInInspector] public bool PlayerAlive = true;
    [SerializeField] Transform camFollow;
    

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
            var targets = FindObjectsOfType<MonoBehaviour>().OfType<ITargettable>();
            foreach (ITargettable target in targets){
                if (target.Owner != null){
                    CreatureLogic owner = target.Owner.GetComponent<CreatureLogic>();
                    if (owner != null && owner.Target != target.gameObj){
                        //Debug.Log(target + " is free from shackles of " + owner.name);
                        target.NotTargeted();
                    }
                }
            }
            yield return new WaitForSeconds(1);
            CheckPlayerDead();
        }
    }

    public void UpdateScore(){
        if (spawner.ActiveBuddies.Count >= playerDeathScore){
            PlayerDeath();
        } else {
            ui.textPlayerPopulation.text = "Buddy Population: " + (spawner.ActiveBuddies.Count).ToString() + " (Player Dies at " + playerDeathScore + ")";
        }
    }

    public void PlayerDeath(){
        if (PlayerAlive){
            PlayerAlive = false;
            StartCoroutine(GhettoAnimations.FallOver(player.transform));
            CheckPlayerDead();
            if (CurrentState.Contains(GameState.State.playerAlive)){
                CurrentState.Remove(GameState.State.playerAlive);
            }
            ui.DisplayMessage(cow.transform.position,"player died");
        }
        
    }

    void CheckPlayerDead(){
        if (!PlayerAlive){
            camFollow.parent = cow.transform;
            camFollow.localPosition = Vector3.zero;
            // vcam.m_Follow = cow.transform;
            // vcam.transform.position = new Vector3(0,25,-35);
            foreach (GameObject b in spawner.ActiveBuddies){
                Buddy buddy = b.GetComponent<Buddy>();
                if (buddy.learning){
                    buddy.SwitchToAILearn();
                }
            }
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
