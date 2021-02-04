using UnityEngine;
using System.Collections.Generic;
public class ThrowItem : GOAPAct
{
    GameObject throwTargetPos;//virtual obj for tracking throwing target
    public ThrowItem(Vector3 mousePos, int heldItemLayer, int targetedLayer){
        throwTargetPos = new GameObject();
        throwTargetPos.transform.position = mousePos;
        ActionLayer = heldItemLayer;
        ActionLayer2 = targetedLayer;
        if (heldItemLayer == 7){//if holding a berry
            Preconditions.Add(GameState.State.itemBerry);
            if (targetedLayer == 14){//and targeting the cow
                Effects.Add(GameState.State.goalHarvested);
                motiveHarvest++;
            }
        }
        if (heldItemLayer == 9){//if holding fungus
            Preconditions.Add(GameState.State.itemFungus);
            Effects.Add(GameState.State.goalReproduced);
            motiveReproduction++;
        }
        if (heldItemLayer == 10){//if holding berry poop
            Preconditions.Add(GameState.State.itemBerryPoop);
            if (targetedLayer == 0){//and targeting ground
                
                Effects.Add(GameState.State.availBush);
                motiveHarvest++;
            }
            if (targetedLayer == 11){//and targeting enemy
                Preconditions.Add(GameState.State.availEnemy);
                Effects.Add(GameState.State.goalAttacked);
                motiveAttack++;
            }
        }
        if (heldItemLayer == 16){//if holding fungus poop
            Preconditions.Add(GameState.State.itemFungusPoop);
            if (targetedLayer == 0){//and targeting ground
                Effects.Add(GameState.State.availMushroom);
                motiveReproduction++;
            }
            if (targetedLayer == 11){//and targeting enemy
                Preconditions.Add(GameState.State.availEnemy);
                Effects.Add(GameState.State.goalAttacked);
                motiveAttack++;
            }
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override GOAPAct Clone(){
        ThrowItem clone = new ThrowItem(Vector3.zero,this.ActionLayer, this.ActionLayer2);//Vector3.zero = where player threw (currently not tracked)
        return clone;
    }

    public override bool GetTarget(Creature agent){
        if (agent.Target == null){
            agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        }
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null){
            float dist = Tools.GetDist(agent.Target,agent.gameObject);
            return (dist > agent.MyStats.Range) ? false : true;
        } else {
            return false;
        }
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (ActionLayer2 == 0){//target is null (ground)
            if (throwTargetPos == null){
                throwTargetPos = new GameObject();
            }
            //throw at random open area near cow
            throwTargetPos.transform.position = manager.spawner.EmptyNearbyLocation(GameObject.FindObjectOfType<Cow>().transform.position,0,10);
            Debug.Log(agent.name + "tried to throw to " + throwTargetPos.transform.position);
            target = throwTargetPos;
        }
        if (ActionLayer2 == 14){//target is cow
            target = GameObject.FindObjectOfType<Cow>().gameObject;
        }
        if (ActionLayer2 == 12){//target is buddy
            target = Tools.FindWeakestAndClosestCreature(manager.spawner.ActiveBuddies,agent.gameObject);//target the nearest, weakest buddy
        }
        if (ActionLayer2 == 13){//target is player
            target = agent.gameObject;
        }
        if (ActionLayer2 == 11){//target is enemy
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        }

        // if (EventLayer == 7){//if berry
        //     if (EventLayer2 == 14){//and target is cow
        //         target = GameObject.FindObjectOfType<Cow>().gameObject;
        //     }
        //     // if (EventLayer2 == 13){//and target is player/buddy
        //     //     target = agent.gameObject;
        //     // }
        // }
        // if (EventLayer == 9){//if fungus, throw it to the cow
        //     target = GameObject.FindObjectOfType<Cow>().gameObject;
        // }
        // if (EventLayer == 10 || EventLayer == 16){//if poop
        //     if (EventLayer2 == 0){//and target is null
        //         throwPos.transform.position = manager.spawner.EmptyNearbyLocation(agent.transform.position,5,10);//throw at some random open area
        //         target = throwPos;
        //     }
        //     if (EventLayer2 == 11){//and target is enemy
        //         target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        //     }
        // }
        return target;
    }

    public override bool PerformEvent(Creature agent){
        Vector3 throwHere = Vector3.zero;
        if (agent is Player || ActionLayer2 == 0){//if its the player, then just throw where the mousePos is
            throwHere = throwTargetPos.transform.position;
            if (agent is Player){
                GameObject.Destroy(throwTargetPos); //just doing some memory cleanup cuz Unity doesn't seem to catch this in GC
            }
        } else { //if AI, then throw at whatever target we gave it
            throwHere = agent.Target.transform.position;
        }

        float dist = Tools.GetDistVector3(agent.transform.position,throwHere);//calculating throwing strength (only for player cuz AI will move into range)
        float throwStrength = 1;
        if (dist > agent.MyStats.Range){
            throwStrength = Mathf.Max(1,agent.MyStats.Range/dist); 
        }

        if (agent.HeldItem != null){
            agent.HeldItem.GetComponent<IThrowable>().ThrowObject(throwHere,throwStrength);
            CompleteEvent(agent);
            return true;
        } else {
            CompleteEvent(agent);
            Debug.Log("couldn't complete event cuz " + agent + "'s HeldItem was null");
            return false;
        }
    }
    protected override bool CompleteEvent(Creature agent){
        throwTargetPos = null;//remove this if you ever make them throw relative to cow or specific locations
        agent.Target = null;
        agent.HeldItem = null;
        if (throwTargetPos != null){
            GameObject.Destroy(throwTargetPos);
        }
        return true;
    }
}
