using UnityEngine;
using System.Collections.Generic;
public class PickupItem : FrameworkEvent
{
    public PickupItem(int itemLayer){
        EventRange = 2; //distance necessary to pickup an item
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
            Preconditions.Add(GameState.State.availBomb);
            Effects.Add(GameState.State.itemBomb);
            motiveAttack++;
        }
    }

    public override FrameworkEvent Clone(){
        PickupItem clone = new PickupItem(this.EventLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        return agent.Target != null? true : false;
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
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBombs,agent.gameObject);
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
        agent.HeldItem = agent.Target;
        agent.Target.GetComponent<Item>().PickUp(agent.transform);
        // Vector3 pos = agent.transform.position;
        // pos.y += 3;
        // agent.Target.transform.position = pos;
        // agent.Target.transform.SetParent(agent.transform);
        // agent.Target.transform.rotation = Quaternion.identity;
        // agent.Target.transform.localScale = new Vector3(0.5f,0.25f,0.5f);
        // agent.Target.GetComponent<Rigidbody>().isKinematic = true;
        CompleteEvent(agent);
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        agent.Target.GetComponent<Item>().FlyingOrPickedUp();
        agent.Target = null;
        return true;
    }
}
