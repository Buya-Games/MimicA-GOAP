using UnityEngine;
using System.Collections.Generic;
public class Eat : FrameworkEvent
{
    public Eat(){
        EventCost = -100;//gotta eat! so "cost" is lowest
        Preconditions.Add(GameState.State.itemBerry);
        Effects.Add(GameState.State.goalEat);
    }

    public override FrameworkEvent Clone(){
        Follow clone = new Follow();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = agent.gameObject;
        return agent.Target != null ? true : false;
    }

    public override bool CheckRange(Creature agent){
        return true;
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        return null;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        return (GameState.CompareStates(Preconditions,currentState)) ? true : false;
    }

    public override bool CheckEffects(List<GameState.State> goalState){
        return (GameState.CompareStates(goalState,Effects)) ? true : false;
    }

    public override bool PerformEvent(Creature agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        if (agent.HeldItem != null){
            manager.particles.EatingBerry(agent.HeldItem.transform.position);
            manager.spawner.DespawnEnvironment(agent.HeldItem,Spawner.EnvironmentType.Berry);
            agent.Eat();

            //agent.HeldItem.GetComponent<IThrowable>().ThrowObject(Vector3.zero,0,true);
            agent.HeldItem = null;
            agent.Target = null;
        }
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
