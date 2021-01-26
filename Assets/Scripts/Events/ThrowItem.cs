using UnityEngine;
using System.Collections.Generic;
public class ThrowItem : FrameworkEvent
{
    public Vector3 ThrowTarget;
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

    public override bool CheckRange(Creature agent){
        agent.Target = agent.Cow; //not sure if this needs to be here... 
        if (agent.HeldItem != null && ThrowTarget != Vector3.zero){
            //agent.LookAtLocation(ThrowTarget);
            float dist = Mathf.Abs(Vector3.Distance(agent.transform.position,ThrowTarget));
            if (dist > agent.MyStats.Range){
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
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
        float dist = Mathf.Abs(Vector3.Distance(agent.transform.position,ThrowTarget));
        float throwStrength = 1;
        if (dist > agent.MyStats.Range){
            throwStrength = agent.MyStats.Range/dist; //this really only exists for the player, cuz AI companions will always try to move into their throwing range
        }
        agent.HeldItem.GetComponent<IThrowable>().ThrowObject(ThrowTarget,throwStrength);
        CompleteEvent(agent);
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        agent.Target = null;
        agent.HeldItem = null;
        // agent.MyStats.Range++;
        return true;
    }
}
