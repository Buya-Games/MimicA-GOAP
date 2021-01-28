using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum EnvironmentType {Tree, Bush, Mushroom, Fungus, Berry, Bomb}
    GameManager manager;
    [SerializeField] GameObject treePrefab, bushPrefab, mushroomPrefab, buddyPrefab, enemyPrefab, berryPrefab, bombPrefab, fungusPrefab;
    Queue<GameObject> treeQueue = new Queue<GameObject>();
    Queue<GameObject> mushroomQueue = new Queue<GameObject>();
    Queue<GameObject> fungusQueue = new Queue<GameObject>();
    Queue<GameObject> bushQueue = new Queue<GameObject>();
    Queue<GameObject> enemyQueue = new Queue<GameObject>();
    Queue<GameObject> berryQueue = new Queue<GameObject>();
    Queue<GameObject> bombQueue = new Queue<GameObject>();
    public List<GameObject> ActivesBushes =  new List<GameObject>();
    public List<GameObject> ActiveBerries =  new List<GameObject>();
    public List<GameObject> ActiveBombs =  new List<GameObject>();
    public List<GameObject> ActiveMushrooms =  new List<GameObject>();
    public List<GameObject> ActiveFungus =  new List<GameObject>();
    public List<GameObject> ActiveEnemies =  new List<GameObject>();
    [HideInInspector]public List<GameObject> ActiveBuddies =  new List<GameObject>();
    [SerializeField] LayerMask spawnCheckLM;

    void Awake(){
        manager = GetComponent<GameManager>();
        InitializeQueues();
    }
    
    void InitializeQueues(){
        for (int i = 0;i<5;i++){
            GameObject newTree = Instantiate(treePrefab);
            newTree.transform.SetParent(transform);
            newTree.SetActive(false);
            treeQueue.Enqueue(newTree);

            GameObject newBush = Instantiate(bushPrefab);
            newBush.transform.SetParent(transform);
            newBush.SetActive(false);
            bushQueue.Enqueue(newBush);

            GameObject newBerry = Instantiate(berryPrefab);
            newBerry.transform.SetParent(transform);
            newBerry.SetActive(false);
            berryQueue.Enqueue(newBerry);

            GameObject newMushroom = Instantiate(mushroomPrefab);
            newMushroom.transform.SetParent(transform);
            newMushroom.SetActive(false);
            mushroomQueue.Enqueue(newMushroom);

            GameObject newFungus = Instantiate(fungusPrefab);
            newFungus.transform.SetParent(transform);
            newFungus.SetActive(false);
            fungusQueue.Enqueue(newFungus);

            GameObject newBomb = Instantiate(bombPrefab);
            newBomb.transform.SetParent(transform);
            newBomb.SetActive(false);
            bombQueue.Enqueue(newBomb);

            GameObject newEnemy = Instantiate(enemyPrefab);
            newEnemy.transform.SetParent(transform);
            newEnemy.SetActive(false);
            enemyQueue.Enqueue(newEnemy);
        }
    }

    public void SpawnCreature(Vector3 spawnPos, bool enemy = false){
        GameObject newCreature = null;
        if (!enemy){//if not enemy, spawn a buddy
            newCreature = Instantiate(buddyPrefab);
            spawnPos.z-=2;
            newCreature.transform.position = spawnPos;
            ActiveBuddies.Add(newCreature);
            manager.CurrentState.Add(GameState.State.availBuddy);
        } else {
            newCreature = Instantiate(enemyPrefab);
            newCreature.transform.position = spawnPos;
            ActiveEnemies.Add(newCreature);
            manager.CurrentState.Add(GameState.State.availEnemy);
        }
        newCreature.GetComponent<Creature>().Init();
    }

    public void DespawnCreature(GameObject who){
        if (who.GetComponent<Buddy>() != null){ //if its a buddy ## DOES THIS WORK??
            Debug.Log("despawned a buddy successfully");
            ActiveBuddies.Remove(who);
            manager.CurrentState.Remove(GameState.State.availBuddy);
        } else {
            ActiveEnemies.Remove(who);
            manager.CurrentState.Remove(GameState.State.availEnemy);
        }
        Destroy(who);//should I make this into a queue to avoid creating/destroying all the time?
    }

    public void SpawnEnvironment(Vector3 spawnPos, EnvironmentType type){
        GameObject newItem = null;
        Queue<GameObject> useQueue = new Queue<GameObject>();
        List<GameObject> useList = new List<GameObject>();
        GameObject usePrefab = treePrefab;
        if (type == EnvironmentType.Tree){
            useQueue = treeQueue;
            usePrefab = treePrefab;
        } else if (type == EnvironmentType.Bush){
            useQueue = bushQueue;
            usePrefab = bushPrefab;
            useList = ActivesBushes;
            manager.CurrentState.Add(GameState.State.availBush);
        } else if (type == EnvironmentType.Berry){
            useQueue = berryQueue;
            usePrefab = berryPrefab;
            useList = ActiveBerries;
            manager.CurrentState.Add(GameState.State.availBerry);
        } else if (type == EnvironmentType.Mushroom){
            useQueue = mushroomQueue;
            usePrefab = mushroomPrefab;
            useList = ActiveMushrooms;
            manager.CurrentState.Add(GameState.State.availMushroom);
        } else if (type == EnvironmentType.Fungus){
            useQueue = fungusQueue;
            usePrefab = fungusPrefab;
            useList = ActiveFungus;
            manager.CurrentState.Add(GameState.State.availFungus);
        } else if (type == EnvironmentType.Bomb){
            useQueue = bombQueue;
            usePrefab = bombPrefab;
            useList = ActiveBombs;
            manager.CurrentState.Add(GameState.State.availBomb);
        }
        if (useQueue.Count > 0){
            newItem = useQueue.Dequeue();
        } else {
            newItem = Instantiate(usePrefab);
            newItem.transform.SetParent(transform);
        }
        useList.Add(newItem);

        newItem.transform.position = spawnPos;
        Vector3 randoRot = newItem.transform.rotation.eulerAngles;

        if (type == EnvironmentType.Mushroom){
            randoRot.y = Random.Range(-360,360); //random rotation just to add some diversity
        } else {
            randoRot.z = Random.Range(-360,360); //random rotation just to add some diversity
        }
        newItem.transform.rotation = Quaternion.Euler(randoRot);

        if (type == EnvironmentType.Berry || type == EnvironmentType.Fungus){
            Vector3 rando = new Vector3(Random.Range(-.1f,.1f),1,Random.Range(-.1f,.1f));
            newItem.GetComponent<Rigidbody>().velocity = rando * 15;
        }

        if (type == EnvironmentType.Bomb){
            Vector3 poopedOut = new Vector3(Random.Range(-.2f,.2f),0,-1);
            newItem.GetComponent<Rigidbody>().velocity = poopedOut * Random.Range(3,8);
        }

        newItem.SetActive(true);
    }

    public void DespawnEnvironment(GameObject item, EnvironmentType type){
        item.SetActive(false);
        item.transform.SetParent(transform);
        if (type == EnvironmentType.Tree){
            treeQueue.Enqueue(item);
        } else if (type == EnvironmentType.Bush){
            bushQueue.Enqueue(item);
            ActivesBushes.Remove(item);
            manager.CurrentState.Remove(GameState.State.availBush);
        } else if (type == EnvironmentType.Mushroom){
            mushroomQueue.Enqueue(item);
            ActiveMushrooms.Remove(item);
            manager.CurrentState.Remove(GameState.State.availMushroom);
        } else if (type == EnvironmentType.Fungus){
            fungusQueue.Enqueue(item);
            ActiveFungus.Remove(item);
            manager.CurrentState.Remove(GameState.State.availFungus);
        } else if (type == EnvironmentType.Berry){
            berryQueue.Enqueue(item);
            ActiveBerries.Remove(item);
            manager.CurrentState.Remove(GameState.State.availBerry);
        } else if (type == EnvironmentType.Bomb){
            bombQueue.Enqueue(item);
            ActiveBombs.Remove(item);
            manager.CurrentState.Remove(GameState.State.availBomb);
        }
    }

    public void RemoveFlyingObject(GameObject item, EnvironmentType type, bool add = false){
        if (!add){
            if (type == EnvironmentType.Fungus && ActiveFungus.Contains(item)){
                ActiveFungus.Remove(item);
                manager.CurrentState.Remove(GameState.State.availFungus);
            } else if (type == EnvironmentType.Berry && ActiveBerries.Contains(item)){
                ActiveBerries.Remove(item);
                manager.CurrentState.Remove(GameState.State.availBerry);
            } else if (type == EnvironmentType.Bomb && ActiveBombs.Contains(item)){
                ActiveBombs.Remove(item);
                manager.CurrentState.Remove(GameState.State.availBomb);
            }
        } else {
            if (type == EnvironmentType.Fungus && !ActiveFungus.Contains(item)){
                ActiveFungus.Add(item);
                manager.CurrentState.Add(GameState.State.availFungus);
            } else if (type == EnvironmentType.Berry && !ActiveBerries.Contains(item)){
                ActiveBerries.Add(item);
                manager.CurrentState.Add(GameState.State.availBerry);
            } else if (type == EnvironmentType.Bomb && !ActiveBombs.Contains(item)){
                ActiveBombs.Add(item);
                manager.CurrentState.Add(GameState.State.availBomb);
            }
        }
    }
    public Vector3 EmptyLocation(){
        int posNegX = Random.Range(0,2)*2-1;
        int posNegZ = Random.Range(0,2)*2-1;
        Vector3 pos = manager.cow.transform.position + new Vector3(Random.Range(0,25)*posNegX,0,Random.Range(0,20)*posNegZ);
        while (!CheckIfLocationClear(pos)){
            int posNegX2 = Random.Range(0,2)*2-1;
            int posNegZ2 = Random.Range(0,2)*2-1;
            pos = manager.cow.transform.position + new Vector3(Random.Range(0,25)*posNegX,0,Random.Range(0,20)*posNegZ);
        }
        return pos;
    }

    public Vector3 EmptyNearbyLocation(Vector3 nearby){
        int x = 10; 
        int z = 10;
        int posNegX = Random.Range(0,2)*2-1;
        int posNegZ = Random.Range(0,2)*2-1;
        Vector3 pos = nearby + new Vector3(Random.Range(0,x)*posNegX,0,Random.Range(0,z)*posNegZ);
        while (!CheckIfLocationClear(pos)){
            x++;z++;
            int posNegX2 = Random.Range(0,2)*2-1;
            int posNegZ2 = Random.Range(0,2)*2-1;
            pos = nearby + new Vector3(Random.Range(0,x)*posNegX,0,Random.Range(0,z)*posNegZ);
        }
        Debug.Log("had to look " + (x-10).ToString() + "times to find an open spot");
        return pos;
    }

    bool CheckIfLocationClear(Vector3 where){
        Collider[] nearbyObjects = Physics.OverlapSphere(where,2,spawnCheckLM);
        return (nearbyObjects.Length > 0) ? false : true;
    }
}
