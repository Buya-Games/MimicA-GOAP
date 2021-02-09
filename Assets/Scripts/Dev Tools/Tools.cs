using System.Collections.Generic;
using UnityEngine;

//just various useful tools stored in one place
public static class Tools
{
    static float maxDistance = 100;
    public static float GetDist(GameObject target, GameObject agent){
        return Mathf.Abs(Vector3.Distance(target.transform.position, agent.transform.position));
    }

    public static float GetDistVector3(Vector3 target, Vector3 agent){
        return Mathf.Abs(Vector3.Distance(target, agent));
    }

    public static GameObject FindClosestObjectInList(List<GameObject> objects, GameObject agent, bool searchPlayerItem = false){
        GameObject closest = null;

        //if player is already holding item of the targeted layer, then start search from there
        if (searchPlayerItem){
            closest = agent.GetComponent<Creature>().Target;
        }

        if (objects.Count>0){
            float dist = 5;//starting distance to search through
            while (closest == null && dist < maxDistance){
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

    public static ITargettable FindClosestTargetInList(List<ITargettable> objects, GameObject agent, bool searchPlayerItem = false){
        ITargettable closest = null;

        //if player is already holding item of the targeted layer, then start search from there
        if (searchPlayerItem){
            closest = agent.GetComponent<Creature>().Target.GetComponent<ITargettable>();
        }

        if (objects.Count>0){
            float dist = 5;//starting distance to search through
            while (closest == null && dist < maxDistance){
                // if (dist >= 100) {Debug.Log(dist);}
                foreach (ITargettable b in objects){
                    //Debug.Log(b.TargetBy);
                    if (b.Owner == null || b.Owner == agent){
                        float thisDist = GetDist(b.gameObj,agent);
                        if (thisDist < dist){
                            closest = b;
                        }
                    }
                }
                dist+=10;
            }
        }
        return closest;
    }

    public static List<ITargettable> ConvertToITargettable(List<GameObject> objects){
        List<ITargettable> targets = new List<ITargettable>();
        foreach (GameObject o in objects){
            targets.Add(o.GetComponent<ITargettable>());
        }
        return targets;

    }

    public static List<GameObject> FindNearbyObjects(List<GameObject> objects, GameObject agent){
        List<GameObject> closest = new List<GameObject>();
        if (objects.Count>0){
            float dist = 5;//starting distance to search through
            while (closest == null && dist < maxDistance){
                // if (dist >= 100) {Debug.Log(dist);}
                foreach (GameObject b in objects){
                    float thisDist = GetDist(b,agent);
                    if (thisDist < dist){
                        closest.Add(b);
                    }
                }
                dist+=10;
            }
        }
        return closest;
    }

    public static GameObject FindWeakestAndClosestCreature(List<GameObject> objects, GameObject agent){
        if (objects.Contains(agent)){//don't throw to yourself
            objects.Remove(agent);
        }
        GameObject weakest = null;
        if (objects.Count>0){
            float closestAndLowest = Mathf.Infinity;//starting distance to search through
            foreach (GameObject c in objects){
                float thisDist = GetDist(c.gameObject,agent);
                thisDist+=c.GetComponent<Creature>().health;
                if (thisDist < closestAndLowest){
                    weakest = c;
                }
            }
        }
        return weakest;
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
            useList = spawner.ActiveBushes;
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

    public static List<T> CopyList<T> (List<T> copyThis){
        List<T> newList = new List<T>();
        foreach (T t in copyThis){
            newList.Add(t);
        }
        return newList;
    }

    public static void PrintList<T>(string preface1, string preface2, List<T> list){
        foreach (T item in list){
            Debug.Log("[" + preface1 + "] " + preface2 + ": " + item);
        }
    }

    public static void PrintQueue<T>(string preface1, string preface2, Queue<T> q){
        foreach (T item in q){
            Debug.Log("[" + preface1 + "] " + preface2 + ": " + item);
        }
    }

    public static bool ContainsLayer(this LayerMask mask, int layer)
     {
         return mask == (mask | (1 << layer));
     }
}
