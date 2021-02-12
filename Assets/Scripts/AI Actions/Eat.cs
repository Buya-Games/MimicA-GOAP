using UnityEngine;
public class Eat: GOAPAct
{
    public Eat(int itemLayer){
        ActionLayer = itemLayer;
        if (ActionLayer == 7){
            Preconditions.Add(GameState.State.itemBerry);
            coreCost = -200;//gotta eat! so "cost" is lowest. and berry is best thing to eat so eat that
        }
        if (ActionLayer == 9){
            Preconditions.Add(GameState.State.itemFungus);
            coreCost = -50; 
        }
        if (ActionLayer == 10){
            Preconditions.Add(GameState.State.itemBerryPoop);
            coreCost = 30;//eating poop is pretty bad so try not to do it but if you got no other choice...
        }
        if (ActionLayer == 16){
            Preconditions.Add(GameState.State.itemFungusPoop);
            coreCost = 30;//eating poop is pretty bad so try not to do it  but if you got no other choice...
        }
        //eating allows ALL goals. MUST STAY NOURISHED!!!!
        Effects.Add(GameState.State.goalEat);
        // Effects.Add(GameState.State.goalFollowPlayer);
        // Effects.Add(GameState.State.goalGatherFood);
        // Effects.Add(GameState.State.goalGatherShrooms);
        // Effects.Add(GameState.State.goalHelpOthers);
        // Effects.Add(GameState.State.goalAttackEnemies);
        // Effects.Add(GameState.State.goalAttackCow);
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
        if (agent.HeldItem != null){
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
