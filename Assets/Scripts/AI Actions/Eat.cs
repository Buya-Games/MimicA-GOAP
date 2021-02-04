using UnityEngine;
using System.Collections.Generic;
public class Eat: GOAPAct
{
    public Eat(){
        coreCost = -100;//gotta eat! so "cost" is lowest
        Preconditions.Add(GameState.State.itemBerry);
        Effects.Add(GameState.State.goalEat);
        Effects.Add(GameState.State.itemNone);
    }

    public override GOAPAct Clone(){
        Eat clone = new Eat();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = agent.gameObject;
        return agent.Target != null ? true : false;
    }

    public override bool CheckRange(Creature agent){
        return true;
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        if (agent.HeldItem != null){
            agent.Eat();
            return true;
        } else {
            return false;
        }
        
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
