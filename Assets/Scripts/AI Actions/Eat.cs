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
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        if (agent.HeldItem != null){
            manager.particles.EatingBerry(agent.HeldItem.transform.position);
            manager.spawner.ThrowOrPickUpObject(agent.HeldItem,Spawner.EnvironmentType.Berry,true);
            manager.spawner.DespawnEnvironment(agent.HeldItem,Spawner.EnvironmentType.Berry);
            agent.Eat();
            CompleteEvent(agent);
            return true;
        } else {
            return false;
        }
        
    }
    protected override bool CompleteEvent(Creature agent){
        //agent.HeldItem.GetComponent<IThrowable>().ThrowObject(Vector3.zero,0,true);
        agent.HeldItem = null;
        agent.Target = null;
        return true;
    }
}
