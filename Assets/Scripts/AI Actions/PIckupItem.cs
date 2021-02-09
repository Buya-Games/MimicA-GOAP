using UnityEngine;
using System.Collections.Generic;
public class PickupItem : GOAPAct
{
    public PickupItem(int itemLayer, bool trackMotive = true){
        Init(); 
        eventRange = 3; //distance necessary to pickup an item
        ActionLayer = itemLayer;
        Preconditions.Add(GameState.State.itemNone);
        if (ActionLayer == 7){
            Preconditions.Add(GameState.State.availBerry);
            Effects.Add(GameState.State.itemBerry);
            if (trackMotive){
                motiveHarvest++;
            }
        }
        if (ActionLayer == 9){
            Preconditions.Add(GameState.State.availFungus);
            Effects.Add(GameState.State.itemFungus);
            if (trackMotive){
                motiveReproduction++;
            }
            
        }
        if (ActionLayer == 10){
            Preconditions.Add(GameState.State.availBerryPoop);
            Effects.Add(GameState.State.itemBerryPoop);
            if (trackMotive){
                motiveHarvest++;
                motiveAttack++;
            }
        }
        if (ActionLayer == 16){
            Preconditions.Add(GameState.State.availFungusPoop);
            Effects.Add(GameState.State.itemFungusPoop);
            if (trackMotive){
                motiveReproduction++;
                motiveAttack++;
            }
        }
    }

    public override GOAPAct Clone(){
        PickupItem clone = new PickupItem(this.ActionLayer);
        return clone;
    }

    public override bool GetTarget(Creature agent){
        GameObject target = FindClosestObjectOfLayer(agent);
        if (target != null){
            agent.SetTarget(target);
            return true;
        } else {
            return false;
        }
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < eventRange){
            return true;
        } else {
            return false;
        }
    }

    protected override GameObject FindClosestObjectOfLayer(Creature agent){
        
        //in case player already targeting object of this action's layer, pass that to the search tool
        //(targeting an object removes it from the accessible targets list (to avoid conflicts where everyone targets same thing) so it's necessary
        //to include it in the search tool when double-checking that plans)
        // bool searchPlayerHeldItem = false;
        // if (agent.Target != null && agent.Target.layer == ActionLayer){
        //     searchPlayerHeldItem = true;   
        // }

        GameObject target = null;
        ITargettable fuck = null;
        if (ActionLayer == 7){
            fuck = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerries),agent.gameObject);
            if (fuck != null){
                target = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerries),agent.gameObject).gameObj;
            }
            //target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerries,agent.gameObject);
        }
        if (ActionLayer == 9){
            fuck = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungus),agent.gameObject);
            if (fuck != null){
                target = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungus),agent.gameObject).gameObj;
            }
            // target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungus,agent.gameObject);
        }
        if (ActionLayer == 10){
            fuck = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerryPoop),agent.gameObject);
            if (fuck != null){
                target = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerryPoop),agent.gameObject).gameObj;
            }
            // target = Tools.FindClosestObjectInList(manager.spawner.ActiveBerryPoop,agent.gameObject);
        }
        if (ActionLayer == 16){
            fuck = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungusPoop),agent.gameObject);
            if (fuck != null){
                target = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungusPoop),agent.gameObject).gameObj;
            }
            // target = Tools.FindClosestObjectInList(manager.spawner.ActiveFungusPoop,agent.gameObject);
        }
        return target;
    }

    public override bool PerformEvent(Creature agent){
        agent.Target.GetComponent<Item>().PickUp(agent);
        CompleteEvent(agent);
        return true;
    }
    protected override bool CompleteEvent(Creature agent){
        agent.ClearTarget();
        return true;
    }
}
