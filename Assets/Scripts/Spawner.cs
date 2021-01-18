using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum EnvironmentType {Tree, Bush, Rock, Berry, Bomb}
    GameManager manager;
    [SerializeField] GameObject treePrefab, bushPrefab, rockPrefab, companionPrefab, enemyPrefab, berryPrefab, bombPrefab;
    Queue<GameObject> treeQueue = new Queue<GameObject>();
    Queue<GameObject> rockQueue = new Queue<GameObject>();
    Queue<GameObject> bushQueue = new Queue<GameObject>();
    Queue<GameObject> enemyQueue = new Queue<GameObject>();
    Queue<GameObject> berryQueue = new Queue<GameObject>();
    Queue<GameObject> bombQueue = new Queue<GameObject>();

    [HideInInspector]public List<GameObject> ActivesBushes =  new List<GameObject>();
    [HideInInspector]public List<GameObject> ActiveBerries =  new List<GameObject>();
    [HideInInspector]public List<GameObject> ActiveBombs =  new List<GameObject>();

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

            GameObject newRock = Instantiate(rockPrefab);
            newRock.transform.SetParent(transform);
            newRock.SetActive(false);
            rockQueue.Enqueue(newRock);

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

    public void SpawnCompanion(Vector3 spawnPos){
        GameObject newBuddy = Instantiate(companionPrefab);
        spawnPos.z-=2;
        newBuddy.transform.position = spawnPos;
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
        } else if (type == EnvironmentType.Berry){
            useQueue = berryQueue;
            usePrefab = berryPrefab;
            useList = ActiveBerries;
        } else if (type == EnvironmentType.Rock){
            useQueue = rockQueue;
            usePrefab = rockPrefab;
        } else if (type == EnvironmentType.Bomb){
            useQueue = bombQueue;
            usePrefab = bombPrefab;
            useList = ActiveBombs;
        }
        if (useQueue.Count > 0){
            newItem = useQueue.Dequeue();
        } else {
            newItem = Instantiate(usePrefab);
            newItem.transform.SetParent(transform);
        }
        useList.Add(newItem);

        // if (type == EnvironmentType.Tree){
        //     if (treeQueue.Count > 0){
        //         newItem = treeQueue.Dequeue();
        //     } else {
        //         newItem = Instantiate(treePrefab);
        //         newItem.transform.SetParent(transform);
        //     }
        //     newItem.AddComponent<Resource>();
        // } else if (type == EnvironmentType.Bush){
        //     if (bushQueue.Count > 0){
        //         newItem = bushQueue.Dequeue();
        //     } else {
        //         newItem = Instantiate(bushPrefab);
        //         newItem.transform.SetParent(transform);
        //     }
        // } else if (type == EnvironmentType.Rock){
        //     if (rockQueue.Count > 0){
        //         newItem = rockQueue.Dequeue();
        //     } else {
        //         newItem = Instantiate(rockPrefab);
        //         newItem.transform.SetParent(transform);
        //     }
        // } else if (type == EnvironmentType.Berry){
        //     if (berryQueue.Count > 0){
        //         newItem = rockQueue.Dequeue();
        //     } else {
        //         newItem = Instantiate(rockPrefab);
        //         newItem.transform.SetParent(transform);
        //     }
        // }

        newItem.transform.position = spawnPos;
        
        Vector3 randoRot = newItem.transform.rotation.eulerAngles;
        randoRot.z = Random.Range(-360,360); //random rotation just to add some diversity
        newItem.transform.rotation = Quaternion.Euler(randoRot);

        if (type == EnvironmentType.Berry){
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
        } else if (type == EnvironmentType.Rock){
            rockQueue.Enqueue(item);
        } else if (type == EnvironmentType.Berry){
            berryQueue.Enqueue(item);
            ActiveBerries.Remove(item);
        } else if (type == EnvironmentType.Bomb){
            bombQueue.Enqueue(item);
            ActiveBombs.Remove(item);
        }
    }
}
