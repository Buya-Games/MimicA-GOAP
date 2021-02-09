using UnityEngine;
using System.Collections.Generic;
public class Follow : GOAPAct
{
    public Follow(){
        Init();
        ActionLayer = 13;
        eventRange = Random.Range(5,8); //minimum distance required to satisfy following state
        Preconditions.Add(GameState.State.playerAlive);
        Effects.Add(GameState.State.goalFollowPlayer);
    }

    public override GOAPAct Clone(){
        Follow clone = new Follow();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = player;
        return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < eventRange){
            return true;
        } else {
            return false;
        }
    }

    protected override GameObject FindClosestObjectOfLayer(Creature agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        agent.ClearTarget();
        return true;
    }
}
