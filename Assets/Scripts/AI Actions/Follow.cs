using UnityEngine;
using System.Collections.Generic;
public class Follow : GOAPAct
{
    Transform player;
    public Follow(){
        //EventCost = 500;//lowering priority
        eventRange = Random.Range(3,6); //minimum distance required to satisfy following state
        player = GameObject.FindObjectOfType<Player>().transform;
        Preconditions.Add(GameState.State.playerAlive);
        Effects.Add(GameState.State.goalFollowPlayer);
    }

    public override GOAPAct Clone(){
        Follow clone = new Follow();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = player.gameObject;
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < eventRange){
            return true;
        } else {
            return false;
        }
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
