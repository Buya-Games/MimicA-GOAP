using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public abstract class FrameworkEvent //creates a bunch possible actions (melee attack, ranged attack, etc) which extend this class
{
    public int EventLayer, EventLayer2;//2 is used by some actions
    public List<GameState.State> Preconditions = new List<GameState.State>();
    public List<GameState.State> Effects = new List<GameState.State>();
    public float EventCost = 20f;
    public float motiveReproduction, motiveHarvest, motiveAttack;//counts toward the 3 possible creature goals
    public float EventRange;
    public float EstimateActionCost(Creature agent){
        float cost = 0;
        cost += EventCost;
        GameObject estimatedClosestObj = FindClosestObjectOfLayer(agent.gameObject);
        if (estimatedClosestObj != null){
            cost += Tools.GetDist(estimatedClosestObj,agent.gameObject);
        } else {
            cost += 10; //if no object exists, use a flat value to estimate theoretical cost
        }
        return cost;
    }
    //Creates clone of event so that each companion/creature can have its own values. Without clone, we'd referencing same obj in memory & creating conflicts
    public abstract FrameworkEvent Clone();
    public abstract bool GetTarget(Creature agent); //check if any targets available
    public abstract bool CheckRange(Creature agent); //checking if inRange
    public abstract GameObject FindClosestObjectOfLayer(GameObject agent);
    public abstract bool CheckPreconditions(List<GameState.State> currentState); //checking if current state meets conditions to perform this action
    public abstract bool CheckEffects(List<GameState.State> goalState); //checking if action effects will meet requirement of state
    public abstract bool PerformEvent(Creature agent);//performs the event
    protected abstract bool CompleteEvent(Creature agent);//performs the event
}
