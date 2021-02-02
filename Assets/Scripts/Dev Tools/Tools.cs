using System.Collections.Generic;
using UnityEngine;

//just various useful tools stored in one place
public static class Tools
{
    public static float GetDist(GameObject target, GameObject agent){
        return Mathf.Abs(Vector3.Distance(target.transform.position, agent.transform.position));
    }

    public static float GetDistVector3(Vector3 target, Vector3 agent){
        return Mathf.Abs(Vector3.Distance(target, agent));
    }

    public static GameObject FindClosestObjectInList(List<GameObject> objects, GameObject agent){
        GameObject closest = null;
        float dist = 5;//start with 5
        if (objects.Count>0){
            while (closest == null && dist < 50){//nullCheck(closest)
                // if (dist >= 100) {Debug.Log(dist);}
                foreach (GameObject b in objects){
                    float thisDist = GetDist(b,agent);
                    if (thisDist < dist){
                        closest = b;
                    }
                }
                dist+=10;
            }
        }
        return closest;
    }

    static bool nullCheck(GameObject checkMe){
        return (checkMe == null) ? true : false;
    }

    public static GameObject FindClosestColliderInGroup(Collider[] objects, Vector3 agent){
        GameObject closest = null;
        float dist = Mathf.Infinity;
        if (objects.Length>0){
            foreach (Collider b in objects){
                //if first, set it as closest
                if (closest == null){
                    closest = b.gameObject;
                    dist = GetDistVector3(b.transform.position,agent);
                } else { //else check if closer
                    float distThis = GetDistVector3(b.transform.position,agent);
                    if (distThis < dist){
                        closest = b.gameObject;
                        dist = distThis;
                    }
                }
            }
        }
        return closest;
    }

    public static GameObject FindClosestObjectOfLayer(int layer, GameObject agent){
        List<GameObject> useList = new List<GameObject>();
        Spawner spawner = GameObject.FindObjectOfType<Spawner>();
        if (layer == 6){
            useList = spawner.ActivesBushes;
        }
        if (layer == 7){
            useList = spawner.ActiveBerries;
        }
        if (layer == 8){
            useList = spawner.ActiveMushrooms;
        }
        if (layer == 9){
            useList = spawner.ActiveFungus;
        }
        if (layer == 10){
            useList = spawner.ActiveBerryPoop;
        }
        if (layer == 11){
            useList = spawner.ActiveEnemies;
        }
        if (layer == 12){
            useList = spawner.ActiveBuddies;
        }
        if (layer == 13){
            return GameObject.FindObjectOfType<Player>().gameObject;
        }
        if (layer == 14){
            return GameObject.FindObjectOfType<Cow>().gameObject;
        }
        if (layer == 16){
            useList = spawner.ActiveFungusPoop;
        }

        GameObject closest = null;
        float dist = Mathf.Infinity;
        if (useList.Count>0){
            foreach (GameObject b in useList){
                //if first, set it as closest
                if (closest == null){
                    closest = b;
                    dist = GetDist(b,agent);
                } else { //else check if closer
                    float distThis = GetDist(b,agent);
                    if (distThis < dist){
                        closest = b;
                        dist = distThis;
                    }
                }
            }
        }
        return closest;
    }

    public static List<T> ListSubset<T> (List<T> list, T removeMe) {
        List<T> newList = new List<T>();
        foreach (T t in list){
            if (!t.Equals(removeMe)){
                newList.Add(t);
            }
        }
        return newList;
    }

    public static void PrintList<T>(List<T> list){
        foreach (T item in list){
            Debug.Log(item);
        }
    }

    public static void PrintQueue<T>(Queue<T> q){
        foreach (T item in q){
            Debug.Log(item);
        }
    }
}
