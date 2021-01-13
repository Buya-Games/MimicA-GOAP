using UnityEngine;
public class PickupFood : FrameworkEvent
{
    public static float PickupRange = 2f;
    public float HarvestAmount = 1f;
    public PickupFood(){
        EventName = "Pickup Food";
        MyItem = Items.Food;
        MyRequiredRange = Range.Harvest;
    }

    public override FrameworkEvent Clone(){
        PickupFood clone = new PickupFood();
        clone.EventName = this.EventName;
        clone.MyItem = this.MyItem;
        clone.MyRequiredRange = this.MyRequiredRange;
        clone.HarvestAmount = this.HarvestAmount;
        return clone;
        //throw new System.NotImplementedException();
    }

    public override bool CheckPreconditions(Creature agent){
        if (agent.Target != null && agent.TargetDist < PickupRange){
            return true;
        } else {
            return false;
        }
    }
    public override bool PerformEvent(Creature agent){
        int harvestAmount = (int)(agent.MyStats.FoodHarvestSkill * HarvestAmount);
        agent.HeldItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 spawnPos = agent.transform.position;
        spawnPos.y = 2;
        agent.HeldItem.transform.position = spawnPos;
        return true;
    }
}
