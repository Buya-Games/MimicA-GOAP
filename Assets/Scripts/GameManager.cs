using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public AudioManager audioManager;
    [HideInInspector] public Player player;
    [HideInInspector] public Spawner spawner;
    [HideInInspector] public UI ui;
    [HideInInspector] public Tutorial tut;
    [SerializeField] public List<GameState.State> CurrentState = new List<GameState.State>();
    [HideInInspector] public ParticleManager particles;
    [HideInInspector] public Cow cow;
    [HideInInspector] public bool Tutorial = true;
    [HideInInspector] public bool GameLive;
    public bool debug;
    public Gradient HeadColor;//should really be in Creature.cs
    [SerializeField] int winScore;
    public int ShroomsForBirth, buddyLearningActions;
    [HideInInspector] public bool PlayerAlive = false;
    [SerializeField] Transform camFollow; //target of camera. I move it between player/cow to switch camera focus before/after player death
    public Material HitMaterial;//red color used to indicate when a creature was hit. should really be in Creature.cs
    public Gradient swingNormal, swingHard;//should also be in Creature.cs
    float roundTime;

    //Awake is called before Start
    void Awake(){
        player = FindObjectOfType<Player>();
        spawner = GetComponent<Spawner>();
        ui = GetComponent<UI>();
        particles = GetComponent<ParticleManager>();
        cow = FindObjectOfType<Cow>();
        audioManager = FindObjectOfType<AudioManager>();
        tut = GetComponent<Tutorial>();
        CurrentState.Add(GameState.State.playerAlive);
        ui.ToggleTutorial.onValueChanged.AddListener(ToggleTutorial);
    }

    //Start is called before first frame
    void Start(){
        player.gameObject.SetActive(false);
        SpawnRandomTrees();
    }

    public void StartGame(){//called by menu button
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        if (GameLive){
            EndGame();
        }
        ui.StartGame();
        if (Tutorial){
            StartTutorial();
        } else {
            if (!PlayerAlive){
                Vector3 playerSpawnPos = cow.transform.position;
                playerSpawnPos.z -= 10;
                player.transform.position = playerSpawnPos;
                player.gameObject.SetActive(true);
                PlayerAlive = true;        

                camFollow.parent = player.transform;
                camFollow.localPosition = Vector3.zero;
            }

            GameLive = true;
            StartCoroutine(SpawnBushes());
            StartCoroutine(SpawnMushrooms());
            StartCoroutine(SpawnEnemy());
            StartCoroutine(CheckStuff());
            roundTime = Time.time;
        }
        UpdateScore();
    }

    void StartTutorial(){
        Vector3 playerSpawnPos = cow.transform.position;
        playerSpawnPos.z -= 10;
        player.transform.position = playerSpawnPos;
        player.gameObject.SetActive(true);
        PlayerAlive = true;        

        camFollow.parent = player.transform;
        camFollow.localPosition = Vector3.zero;

        tut.SpawnBush();
        tut.DisplayNextTip(0);//walk to bush and hit it
    }

    public void EndGame(){
        GameLive = false;
        spawner.CleanUp();
        player.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public void PauseGame(){
        if (GameLive){
            GameLive = false;
            Time.timeScale = 0;
        } else {
            GameLive = true;
            Time.timeScale = 1;
        }
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        ui.PauseGame(!GameLive);
    }

    public void PlayerDeath(){
        if (PlayerAlive){
            PlayerAlive = false;
            particles.DestroyingBuddy(player.transform.position);
            player.gameObject.SetActive(false);
            CheckPlayerDead();
            if (CurrentState.Contains(GameState.State.playerAlive)){
                CurrentState.Remove(GameState.State.playerAlive);
            }
            ui.DisplayMessage(cow.transform.position,"player died",Color.black);
        }
        
    }

    void CheckPlayerDead(){
        if (!PlayerAlive){
            camFollow.parent = cow.transform;
            camFollow.localPosition = Vector3.zero;
            foreach (GameObject b in spawner.ActiveBuddies){
                Buddy buddy = b.GetComponent<Buddy>();
                if (buddy.learning){
                    buddy.SwitchToAILearn();
                }
            }
        }
    }

    public void GameOver(bool win = true){
        roundTime = Time.time-roundTime;
        ui.EndGame(win,roundTime);
    }

    public void ToggleTutorial(bool value){
        Tutorial = value;
    }

    IEnumerator SpawnBushes(){
        int counter = 0;
        while (counter < 10000 && GameLive){
            counter++;
            spawner.SpawnEnvironment(spawner.EmptyLocation(), Spawner.EnvironmentType.Bush);
            yield return new WaitForSeconds(Random.Range(15,25));
        }
    }

    IEnumerator SpawnMushrooms(){
        int counter = 0;
        while (counter < 10000 && GameLive){
            counter++;
            spawner.SpawnEnvironment(spawner.EmptyLocation(), Spawner.EnvironmentType.Mushroom);
            yield return new WaitForSeconds(Random.Range(15,25));
        }
    }
    IEnumerator SpawnEnemy(){
        yield return new WaitForSeconds(Random.Range(30,45));
        int counter = 0;
        while (counter < 10000 && GameLive){
            counter++;
            int posNegX = Random.Range(0,2)*2-1;
            int posNegZ = Random.Range(0,2)*2-1;
            Vector3 spawnPos = cow.transform.position + new Vector3(Random.Range(15,25)*posNegX,0,Random.Range(15,20)*posNegZ);
            spawner.SpawnCreature(spawnPos, true);
            yield return new WaitForSeconds(Random.Range(30,45));
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

            //targetted items are excluded as targets for other creatures
            //but sometimes creatures stop targeting items and don't reset them
            //so this goes throgh all the targettable items and resets them if they are no longer a target
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
        if (spawner.ActiveBuddies.Count > winScore){
            GameOver(true);
        }
        ui.textPlayerPopulation.text = "Goblin Population: " + spawner.ActiveBuddies.Count + " (Win at " + winScore + ")";
    }

    //just for looks
    void SpawnRandomTrees(){
        Vector3 firstTree = cow.transform.position;
        firstTree.x += 5;
        firstTree.z += 5;
        spawner.SpawnEnvironment(firstTree, Spawner.EnvironmentType.Tree);

        float mapSize = 80;//i think this is the ground scale
        for (int i = 0;i<9;i++){
            Vector3 randomSpotOnMap = new Vector3(Random.Range(-mapSize,mapSize),0,Random.Range(-mapSize,mapSize));
            spawner.SpawnEnvironment(randomSpotOnMap, Spawner.EnvironmentType.Tree);
        }
    }

    public void QuitGame(){
        Application.Quit();
    }

    // public void SpeedUpTime(){
    //     gameTime *= 2;
    //     Time.timeScale = gameTime;
    //     Time.fixedDeltaTime = Time.timeScale * 0.02f;
    //     ui.textGameTime.text = "Game Speed: " + gameTime + "x";
    // }

    // public void SlowDownTime(){
    //     if (gameTime > 1){
    //         gameTime /= 2;
    //         Time.timeScale = gameTime;
    //         Time.fixedDeltaTime = Time.timeScale * 0.02f;
    //         ui.textGameTime.text = "Game Speed: " + gameTime + "x";
    //     }
    // }
}
