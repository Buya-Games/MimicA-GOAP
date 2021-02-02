using UnityEngine;
using System.Collections.Generic;
public class Follow : FrameworkEvent
{
    Transform player;
    public Follow(){
        EventCost = 500;//they should prioritze eating over following me to survive
        EventRange = 5; //minimum distance required to satisfy following state
        player = GameObject.FindObjectOfType<Player>().transform;
        Effects.Add(GameState.State.goalFollowPlayer);
    }

    public override FrameworkEvent Clone(){
        Follow clone = new Follow();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = player.gameObject;
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < EventRange){
            return true;
        } else {
            return false;
        }
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        return null;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        return true;
    }

    public override bool CheckEffects(List<GameState.State> goalState){
        return true;
    }

    public override bool PerformEvent(Creature agent){
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
