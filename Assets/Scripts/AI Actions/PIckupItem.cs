using UnityEngine;
public class PickupItem : GOAPAct
{
    public PickupItem(int itemLayer, bool trackMotive = true){
        Init(); 
        eventRange = 3; //distance necessary to pickup an item
        ActionLayer = itemLayer;
        Preconditions.Add(GameState.State.itemNone);//hands must be free
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
        GameObject target = null;
        ITargettable itarget = null;
        if (ActionLayer == 7){
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerries),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
        }
        if (ActionLayer == 9){
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungus),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
        }
        if (ActionLayer == 10){
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBerryPoop),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
        }
        if (ActionLayer == 16){
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveFungusPoop),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
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
