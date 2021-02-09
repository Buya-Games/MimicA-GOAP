using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    
    public enum EnvironmentType {Tree, Bush, Mushroom, Fungus, Berry, BerryPoop, FungusPoop}
    GameManager manager;
    [SerializeField] GameObject treePrefab, bushPrefab, mushroomPrefab, buddyPrefab, enemyPrefab, berryPrefab, berryPooprefab, fungusPrefab, fungusPoopPrefab;
    Queue<GameObject> treeQueue = new Queue<GameObject>();
    Queue<GameObject> mushroomQueue = new Queue<GameObject>();
    Queue<GameObject> fungusQueue = new Queue<GameObject>();
    Queue<GameObject> bushQueue = new Queue<GameObject>();
    Queue<GameObject> enemyQueue = new Queue<GameObject>();
    Queue<GameObject> berryQueue = new Queue<GameObject>();
    Queue<GameObject> berryPoopQueue = new Queue<GameObject>();
    Queue<GameObject> fungusPoopQueue = new Queue<GameObject>();
    public List<GameObject> ActiveBushes =  new List<GameObject>();
    public List<GameObject> ActiveBerries =  new List<GameObject>();
    public List<GameObject> ActiveBerryPoop =  new List<GameObject>();
    public List<GameObject> ActiveFungusPoop =  new List<GameObject>();
    public List<GameObject> ActiveMushrooms =  new List<GameObject>();
    public List<GameObject> ActiveFungus =  new List<GameObject>();
    public List<GameObject> ActiveEnemies =  new List<GameObject>();
    public List<GameObject> ActiveBuddies =  new List<GameObject>();
    [SerializeField] LayerMask spawnCheckLM;
    public Queue<CreatureLogic> Teachers = new Queue<CreatureLogic>();

    void Awake(){
        manager = GetComponent<GameManager>();
        InitializeQueues();
    }
    
    void InitializeQueues(){
        for (int i = 0;i<5;i++){
            ActualSpawn(bushPrefab,bushQueue);
            ActualSpawn(berryPrefab,berryQueue);
            ActualSpawn(mushroomPrefab,mushroomQueue);
            ActualSpawn(fungusPrefab,fungusQueue);
            ActualSpawn(berryPooprefab,berryPoopQueue);
            ActualSpawn(fungusPoopPrefab,fungusPoopQueue);
        }
    }

    void ActualSpawn(GameObject prefab, Queue<GameObject> thingQueue){
        GameObject newThing = Instantiate(prefab);
        newThing.transform.SetParent(transform);
        newThing.SetActive(false);
        thingQueue.Enqueue(newThing);
    }

    public void SpawnCreature(Vector3 spawnPos, bool enemy = false){
        GameObject newCreature = null;
        if (!enemy){//if not enemy, spawn a buddy
            newCreature = Instantiate(buddyPrefab);
            spawnPos.z-=2;
            newCreature.transform.position = spawnPos;
            ActiveBuddies.Add(newCreature);
            manager.CurrentState.Add(GameState.State.availBuddy);
            Teachers.Enqueue(newCreature.GetComponent<CreatureLogic>());
        } else {
            newCreature = Instantiate(enemyPrefab);
            newCreature.transform.position = spawnPos;
            ActiveEnemies.Add(newCreature);
            manager.CurrentState.Add(GameState.State.availEnemy);
            //manager.ui.textEnemyPopulation.text = "Human Population: " + ActiveEnemies.Count;
        }
        manager.UpdateScore();
    }

    public void DespawnCreature(GameObject who){
        if (who.GetComponent<Buddy>() != null){ //if its a buddy
            ActiveBuddies.Remove(who);
            manager.CurrentState.Remove(GameState.State.availBuddy);
        } else { //if its an enemy
            ActiveEnemies.Remove(who);
            manager.CurrentState.Remove(GameState.State.availEnemy);
            //manager.ui.textEnemyPopulation.text = "Human Population: " + ActiveEnemies.Count;
        }
        manager.ui.DisplayMessage(who.transform.position,who.name + " died!");
        manager.UpdateScore();
        StartCoroutine(GhettoAnimations.FallOver(who.transform));
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
            useList = ActiveBushes;
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
        } else if (type == EnvironmentType.BerryPoop){
            useQueue = berryPoopQueue;
            usePrefab = berryPooprefab;
            useList = ActiveBerryPoop;
            manager.CurrentState.Add(GameState.State.availBerryPoop);
        } else if (type == EnvironmentType.FungusPoop){
            useQueue = fungusPoopQueue;
            usePrefab = fungusPoopPrefab;
            useList = ActiveFungusPoop;
            manager.CurrentState.Add(GameState.State.availFungusPoop);
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

        if (type == EnvironmentType.BerryPoop || type == EnvironmentType.FungusPoop){
            Vector3 poopedOut = new Vector3(Random.Range(-.2f,.2f),0,-1);
            newItem.GetComponent<Rigidbody>().velocity = poopedOut * Random.Range(3,8);
        }
        newItem.SetActive(true);

        if (type == EnvironmentType.Bush || type == EnvironmentType.Mushroom){
            Vector3 origScale = newItem.transform.localScale;
            newItem.transform.localScale = Vector3.zero;
            newItem.transform.DOScale(origScale,0.01f).OnComplete(() => {
                newItem.transform.DOPunchScale(origScale*0.6f,0.5f,10,0.1f);
            });
        }
    }

    public void DespawnEnvironment(GameObject item, EnvironmentType type){
        if (type == EnvironmentType.Tree){
            treeQueue.Enqueue(item);
        } else if (type == EnvironmentType.Bush){
            bushQueue.Enqueue(item);
            ActiveBushes.Remove(item);
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
        } else if (type == EnvironmentType.BerryPoop){
            berryPoopQueue.Enqueue(item);
            ActiveBerryPoop.Remove(item);
            manager.CurrentState.Remove(GameState.State.availBerryPoop);
        } else if (type == EnvironmentType.FungusPoop){
            fungusPoopQueue.Enqueue(item);
            ActiveFungusPoop.Remove(item);
            manager.CurrentState.Remove(GameState.State.availFungusPoop);
        }
        item.SetActive(false);
        item.transform.SetParent(transform);
    }

    // public void ThrowOrPickUpObject(GameObject item, EnvironmentType type, bool add = false){
    //     if (add){
    //         if (type == EnvironmentType.Fungus && !ActiveFungus.Contains(item)){
    //             ActiveFungus.Add(item);
    //             manager.CurrentState.Add(GameState.State.availFungus);
    //         } else if (type == EnvironmentType.Berry && !ActiveBerries.Contains(item)){
    //             ActiveBerries.Add(item);
    //             manager.CurrentState.Add(GameState.State.availBerry);
    //         } else if (type == EnvironmentType.BerryPoop && !ActiveBerryPoop.Contains(item)){
    //             ActiveBerryPoop.Add(item);
    //             manager.CurrentState.Add(GameState.State.availBerryPoop);
    //         } else if (type == EnvironmentType.FungusPoop && !ActiveFungusPoop.Contains(item)){
    //             ActiveFungusPoop.Add(item);
    //             manager.CurrentState.Add(GameState.State.availFungusPoop);
    //         } else if (type == EnvironmentType.Bush && !ActiveBushes.Contains(item)){
    //             ActiveBushes.Add(item);
    //             manager.CurrentState.Add(GameState.State.availBush);
    //         } else if (type == EnvironmentType.Mushroom && !ActiveMushrooms.Contains(item)){
    //             ActiveMushrooms.Add(item);
    //             manager.CurrentState.Add(GameState.State.availMushroom);
    //         }
    //     } else {
    //         if (type == EnvironmentType.Fungus && ActiveFungus.Contains(item)){
    //             ActiveFungus.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availFungus);
    //         } else if (type == EnvironmentType.Berry && ActiveBerries.Contains(item)){
    //             ActiveBerries.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availBerry);
    //         } else if (type == EnvironmentType.BerryPoop && ActiveBerryPoop.Contains(item)){
    //             ActiveBerryPoop.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availBerryPoop);
    //         } else if (type == EnvironmentType.FungusPoop && ActiveFungusPoop.Contains(item)){
    //             ActiveFungusPoop.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availFungusPoop);
    //         } else if (type == EnvironmentType.Bush && ActiveBushes.Contains(item)){
    //             ActiveBushes.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availBush);
    //         } else if (type == EnvironmentType.Mushroom && ActiveMushrooms.Contains(item)){
    //             ActiveMushrooms.Remove(item);
    //             manager.CurrentState.Remove(GameState.State.availMushroom);
    //         }
    //     }
    // }
    public Vector3 EmptyLocation(){
        int posNegX = Random.Range(0,2)*2-1;
        int posNegZ = Random.Range(0,2)*2-1;
        int maxRadius = 1- + ActiveBuddies.Count;
        Vector3 pos = manager.cow.transform.position + new Vector3(Random.Range(5,maxRadius)*posNegX,0,Random.Range(5,maxRadius)*posNegZ);//relative to the cow
        while (!CheckIfLocationClear(pos)){
            int posNegX2 = Random.Range(0,2)*2-1;
            int posNegZ2 = Random.Range(0,2)*2-1;
            pos = manager.cow.transform.position + new Vector3(Random.Range(5,maxRadius)*posNegX,0,Random.Range(5,maxRadius)*posNegZ);//relative to the cow
        }
        pos.y = 0;
        return pos;
    }

    public Vector3 EmptyNearbyLocation(Vector3 center, int minRadius, int maxRadius){
        int posNegX = Random.Range(0,2)*2-1;
        int posNegZ = Random.Range(0,2)*2-1;
        Vector3 pos = center + new Vector3(Random.Range(minRadius,maxRadius)*posNegX,0,Random.Range(minRadius,maxRadius)*posNegZ);
        while (!CheckIfLocationClear(pos) && maxRadius < 50){//##hard number just to limit the while loops
            minRadius++;maxRadius++;
            int posNegX2 = Random.Range(0,2)*2-1;
            int posNegZ2 = Random.Range(0,2)*2-1;
            pos = center + new Vector3(Random.Range(minRadius,maxRadius)*posNegX,0,Random.Range(minRadius,maxRadius)*posNegZ);
        }
        return pos;
    }

    bool CheckIfLocationClear(Vector3 where){
        Collider[] nearbyObjects = Physics.OverlapSphere(where,1,spawnCheckLM);
        return (nearbyObjects.Length > 0) ? false : true;
    }
}
