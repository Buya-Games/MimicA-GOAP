using UnityEngine;
using System.Collections.Generic;
public class ThrowItem : FrameworkEvent
{
    GameObject throwPos;//obj for virtually tracking positions
    bool feeding = false;//used when throwing something to self to feed self
    public ThrowItem(Vector3 mousePos, int itemLayer, int targetLayer){
        throwPos = new GameObject();
        throwPos.transform.position = mousePos;
        EventLayer = itemLayer;
        EventLayer2 = targetLayer;
        if (itemLayer == 7){
            Preconditions.Add(GameState.State.itemBerry);
            if (targetLayer == 14){
                Effects.Add(GameState.State.goalHarvested);
                motiveHarvest++;
            }
            if (targetLayer == 13){
                feeding = true;
                Effects.Add(GameState.State.goalEat);
            }
        }
        if (itemLayer == 9){
            Preconditions.Add(GameState.State.itemFungus);
            Effects.Add(GameState.State.goalReproduced);
            motiveReproduction++;
        }
        if (itemLayer == 10){
            Preconditions.Add(GameState.State.itemBomb);
            if (targetLayer == 0){
                Effects.Add(GameState.State.goalReproduced);
                motiveReproduction++;
            }
            if (targetLayer == 11){
                Preconditions.Add(GameState.State.availEnemy);
                Effects.Add(GameState.State.goalAttacked);
                motiveAttack++;
            }
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override FrameworkEvent Clone(){
        ThrowItem clone = new ThrowItem(Vector3.zero,this.EventLayer, this.EventLayer2);//Vector3.zero means where player threw (relative to cow, etc) not tracked
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null){
            float dist = Mathf.Abs(Vector3.Distance(agent.Target.transform.position,agent.transform.position));
            return (dist > agent.MyStats.Range) ? false : true;
        } else {
            return false;
        }
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (EventLayer == 7){//if berry
            if (EventLayer2 == 14){//and target is cow
                target = GameObject.FindObjectOfType<Cow>().gameObject;
            }
            if (EventLayer2 == 13){//and target is player/buddy
                target = agent.gameObject;
            }
        }
        if (EventLayer == 9){//if fungus, throw it to the cow
            target = GameObject.FindObjectOfType<Cow>().gameObject;
        }
        if (EventLayer == 10){//if bomb
            if (EventLayer2 == 0){//and target is null
                throwPos.transform.position = manager.spawner.EmptyNearbyLocation(agent.transform.position);//throw at some random open area
                target = throwPos;
            }
            if (EventLayer2 == 11){//and target is enemy
                target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
            }
        }
        return target;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        return (GameState.CompareStates(Preconditions,currentState)) ? true : false;
    }

    public override bool CheckEffects(List<GameState.State> goalState){
        return (GameState.CompareStates(goalState,Effects)) ? true : false;
    }

    public override bool PerformEvent(Creature agent){
        Vector3 throwHere = Vector3.zero;
        if (agent is Player || EventLayer2 == 0){//if its the player, then just throw where the mousePos is
            throwHere = throwPos.transform.position;
        } else { //if AI, then throw at whatever target we gave it
            throwHere = agent.Target.transform.position;
        }

        float dist = Tools.GetDistVector3(agent.transform.position,throwHere);//calculating throwing strength (only for player cuz AI will move into range)
        float throwStrength = 1;
        if (dist > agent.MyStats.Range){
            throwStrength = agent.MyStats.Range/dist; 
        }

        if (agent.HeldItem != null){
            agent.HeldItem.GetComponent<IThrowable>().ThrowObject(throwHere,throwStrength, feeding);
            CompleteEvent(agent);
            return true;
        } else {
            Debug.Log("couldn't complete event cuz " + agent + "'s HeldItem was null");
            return false;
        }
    }
    protected override bool CompleteEvent(Creature agent){
        agent.Target = null;
        agent.HeldItem = null;
        return true;
    }
}
