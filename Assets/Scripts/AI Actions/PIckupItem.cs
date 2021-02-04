using UnityEngine;
using System.Collections.Generic;
public class PickupItem : GOAPAct
{
    public PickupItem(int itemLayer){ 
        eventRange = 3; //distance necessary to pickup an item
        ActionLayer = itemLayer;
        Preconditions.Add(GameState.State.itemNone);
        if (ActionLayer == 7){
            Preconditions.Add(GameState.State.availBerry);
            Effects.Add(GameState.State.itemBerry);
            motiveHarvest++;
        }
        if (ActionLayer == 9){
            Preconditions.Add(GameState.State.availFungus);
            Effects.Add(GameState.State.itemFungus);
            motiveReproduction++;
        }
        if (ActionLayer == 10){
            Preconditions.Add(GameState.State.availBerryPoop);
            Effects.Add(GameState.State.itemBerryPoop);
            // motiveHarvest++;
            // motiveAttack++;
        }
        if (ActionLayer == 16){
            Preconditions.Add(GameState.State.availFungusPoop);
            Effects.Add(GameState.State.itemFungusPoop);
            // motiveReproduction++;
            // motiveAttack++;
        }
    }

    public override GOAPAct Clone(){
        PickupItem clone = new PickupItem(this.ActionLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        if (agent.Target != null){
            return true;
        } else {
            Debug.Log("couldn't find eventlayer " + ActionLayer);
            return false;
        }
        //return agent.Target != null? true : false;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < eventRange){
            return true;
        } else {
            return false;
        }
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (ActionLayer == 7){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerries,agent.gameObject);
        }
        if (ActionLayer == 9){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungus,agent.gameObject);
        }
        if (ActionLayer == 10){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerryPoop,agent.gameObject);
        }
        if (ActionLayer == 16){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungusPoop,agent.gameObject);
        }
        return target;
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
