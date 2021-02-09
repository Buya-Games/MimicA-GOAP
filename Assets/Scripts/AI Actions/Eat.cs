using UnityEngine;
using System.Collections.Generic;
public class Eat: GOAPAct
{
    public Eat(int itemLayer){
        ActionLayer = itemLayer;
        if (ActionLayer == 7){
            Preconditions.Add(GameState.State.itemBerry);
            coreCost = -100;//gotta eat! so "cost" is lowest
        }
        if (ActionLayer == 9){
            Preconditions.Add(GameState.State.itemFungus);
            coreCost = -50;//gotta eat! so "cost" is lowest
        }
        if (ActionLayer == 10){
            Preconditions.Add(GameState.State.itemBerryPoop);
            coreCost = -10;//gotta eat! so "cost" is lowest
        }
        if (ActionLayer == 16){
            Preconditions.Add(GameState.State.itemFungusPoop);
            coreCost = -10;//gotta eat! so "cost" is lowest
        }
        Effects.Add(GameState.State.goalEat);
        Effects.Add(GameState.State.itemNone);
    }

    public override GOAPAct Clone(){
        Eat clone = new Eat(this.ActionLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = agent.gameObject;
        return agent.Target != null ? true : false;
    }

    public override bool CheckRange(Creature agent){
        return true;
    }

    protected override GameObject FindClosestObjectOfLayer(Creature agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        Debug.Log("performing eat");
        if (agent.HeldItem != null){
            Debug.Log("eat: item not null");
            agent.Eat(agent.HeldItem.GetComponent<Item>().MyType);
            return true;
        } else {
            return false;
        }
        
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
