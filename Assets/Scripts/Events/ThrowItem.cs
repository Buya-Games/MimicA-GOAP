using UnityEngine;
using System.Collections.Generic;
public class ThrowItem : FrameworkEvent
{
    public Vector3 ThrowTarget;//not currently used but will be once we start throwing RELATIVE to the cow/enemy and stuff
    public ThrowItem(Vector3 mousePos, int itemLayer){
        ThrowTarget = mousePos;
        EventLayer = itemLayer;
        if (EventLayer == 7){
            Preconditions.Add(GameState.State.itemBerry);
            Effects.Add(GameState.State.goalHarvested);
            motiveHarvest++;
        }
        if (EventLayer == 9){
            Preconditions.Add(GameState.State.itemFungus);
            Effects.Add(GameState.State.goalReproduced);
            motiveReproduction++;
        }
        if (EventLayer == 11){
            Preconditions.Add(GameState.State.itemBomb);
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttacked);
            motiveAttack++;
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override FrameworkEvent Clone(){
        ThrowItem clone = new ThrowItem(this.ThrowTarget,this.EventLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null){
            //agent.LookAtLocation(ThrowTarget);
            float dist = Mathf.Abs(Vector3.Distance(agent.Target.transform.position,agent.transform.position));
            if (dist > agent.MyStats.Range){
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (EventLayer == 7 || EventLayer == 9){//if berry or fungus, throw it to the cow
            target = GameObject.FindObjectOfType<Cow>().gameObject;
        }
        if (EventLayer == 10){//if bomb, throw it at an enemy
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        }
        return target;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        if (GameState.CompareStates(Preconditions,currentState)){
            return true;
        } else {
            return false;
        }
    }

    public override bool CheckEffects(List<GameState.State> goalState){
        if (GameState.CompareStates(goalState,Effects)){//inverted from above
            return true;
        } else {
            return false;
        }
    }

    public override bool PerformEvent(Creature agent){
        Vector3 throwHere = Vector3.zero;
        if (agent is Player){//if its the player, then just throw where the mousePos is
            throwHere = ThrowTarget;
        } else { //if AI, then throw at whatever target we gaveit
            throwHere = agent.Target.transform.position;
        }
        float dist = Mathf.Abs(Vector3.Distance(agent.transform.position,throwHere));//calculating throwing strength
        Debug.Log(dist);
        float throwStrength = 1;
        if (dist > agent.MyStats.Range){
            throwStrength = agent.MyStats.Range/dist; //this really only exists for the player, cuz AI companions will always try to move into their throwing range
        }
        if (agent.HeldItem != null){
            agent.HeldItem.GetComponent<IThrowable>().ThrowObject(throwHere,throwStrength);
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
        // agent.MyStats.Range++;
        return true;
    }
}
