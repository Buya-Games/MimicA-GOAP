using UnityEngine;
using System.Collections.Generic;

public abstract class GOAPAct //parent class for actions (melee attack, throw item, etc)
{
    public int ActionLayer, ActionLayer2;//2 is used by some actions like ThrowItem
    public List<GameState.State> Preconditions = new List<GameState.State>();//gameStates required to preform this action
    public List<GameState.State> Effects = new List<GameState.State>();//gameStates created by this action
    protected float coreCost = 1;//just the action itself, doesn't include movement (higher means lower priority)
    public float Cost;//core action + move (higher means lower priority)
    public float ActionSkill;
    public float motiveReproduction, motiveHarvest, motiveAttack, motiveHelper;//counts toward the 3 possible creature goals
    protected float eventRange;//used to calculate CheckRange
    protected GameObject player,cow;
    protected GameManager manager;

    protected void Init(){
        manager = GameObject.FindObjectOfType<GameManager>();
        CowHead cowCheck = GameObject.FindObjectOfType<CowHead>();
        if (cowCheck != null){
            cow = cowCheck.gameObject;
        } else {
            cow = null;
        }
        Player playerCheck = GameObject.FindObjectOfType<Player>();
        if (playerCheck != null){
            player = playerCheck.gameObject;
        } else {
            player = null;
        }
    }
    public void EstimateActionCost(Creature agent){
        Cost = coreCost * 1;
        GameObject estimatedClosestObj = FindClosestObjectOfLayer(agent);
        if (estimatedClosestObj != null){
            Cost += Tools.GetDist(estimatedClosestObj,agent.gameObject);
        } else {
            Cost += 25;
        }
        // float cost = 0;
        // cost += coreCost;
        // GameObject estimatedClosestObj = FindClosestObjectOfLayer(agent.gameObject);
        //if closest obj exists, add its distance to cost, else use flat value (20) to estimate theoretical distance 
        //return (estimatedClosestObj != null) ? cost += Tools.GetDist(estimatedClosestObj,agent.gameObject) : cost += 20;
    }

    public bool CheckPreconditions(List<GameState.State> currentState){ //checking if current state meets conditions to perform this action
        return (GameState.CompareStates(Preconditions,currentState)) ? true : false;
    }
    public bool CheckEffects(List<GameState.State> goalState){ //checking if action effects will meet requirement of state
        if (manager == null){
            manager = GameObject.FindObjectOfType<GameManager>();
        }
        if (manager.debug){Debug.Log(string.Format("{0}{1}-{2} checking effects",this.GetType(),ActionLayer,ActionLayer2));}
        string actionName = this.GetType().ToString() + ActionLayer + "-" + ActionLayer2;
        if (manager.debug){
            return (GameState.CompareStates(goalState,Effects,actionName)) ? true : false; //flipped from above
        } else {
            return (GameState.CompareStates(goalState,Effects)) ? true : false; //flipped from above
        }
        
    }
    public abstract GOAPAct Clone(); //Creates copy so each creature can have own values. (Without clone, we'd be ref same obj in memory & create conflicts)
    public abstract bool GetTarget(Creature agent); //check if any targets available
    public abstract bool CheckRange(Creature agent); //checking if inRange
    protected abstract GameObject FindClosestObjectOfLayer(Creature agent);
    public abstract bool PerformEvent(Creature agent);//performs the event
    protected abstract bool CompleteEvent(Creature agent);//performs the event
}
