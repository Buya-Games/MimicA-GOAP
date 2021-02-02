using UnityEngine;
using System.Collections.Generic;
public class PickupItem : FrameworkEvent
{
    public PickupItem(int itemLayer){ 
        EventRange = 3; //distance necessary to pickup an item
        EventLayer = itemLayer;
        Preconditions.Add(GameState.State.itemNone);
        if (EventLayer == 7){
            Preconditions.Add(GameState.State.availBerry);
            Effects.Add(GameState.State.itemBerry);
            motiveHarvest++;
        }
        if (EventLayer == 9){
            Preconditions.Add(GameState.State.availFungus);
            Effects.Add(GameState.State.itemFungus);
            motiveReproduction++;
        }
        if (EventLayer == 10){
            Preconditions.Add(GameState.State.availBerryPoop);
            Effects.Add(GameState.State.itemBerryPoop);
            // motiveHarvest++;
            // motiveAttack++;
        }
        if (EventLayer == 16){
            Preconditions.Add(GameState.State.availFungusPoop);
            Effects.Add(GameState.State.itemFungusPoop);
            // motiveReproduction++;
            // motiveAttack++;
        }
    }

    public override FrameworkEvent Clone(){
        PickupItem clone = new PickupItem(this.EventLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        if (agent.Target != null){
            return true;
        } else {
            Debug.Log("couldn't find eventlayer " + EventLayer);
            return false;
        }
        //return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < EventRange){
            return true;
        } else {
            return false;
        }
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (EventLayer == 7){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerries,agent.gameObject);
        }
        if (EventLayer == 9){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungus,agent.gameObject);
        }
        if (EventLayer == 10){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerryPoop,agent.gameObject);
        }
        if (EventLayer == 16){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungusPoop,agent.gameObject);
        }
        return target;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        if (GameState.CompareStates(Preconditions,currentState)){// && agent.Target != null){
            return true;
        } else {
            return false;
        }
    }

    public override bool CheckEffects(List<GameState.State> goalState){
        if (GameState.CompareStates(goalState,Effects)){//inverted from above
            return true;
        } else {
            return false;
        }
    }

    public override bool PerformEvent(Creature agent){
        agent.Target.GetComponent<Item>().PickUp(agent);
        CompleteEvent(agent);
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        agent.Target.GetComponent<Item>().FlyingOrPickedUp();
        agent.Target = null;
        return true;
    }
}
