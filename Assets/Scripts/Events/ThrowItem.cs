using UnityEngine;
public class ThrowItem : FrameworkEvent
{
    public Vector3 ThrowTarget;
    public ThrowItem(Vector3 mousePos, int targetLayer){
        ThrowTarget = mousePos;
        TargetLayer = targetLayer;
    }

    public override FrameworkEvent Clone(){
        ThrowItem clone = new ThrowItem(this.ThrowTarget,this.TargetLayer);
        return clone;
    }

    public override bool CheckRange(Creature agent){
        if (agent.HeldItem != null && ThrowTarget != Vector3.zero){
            float dist = Mathf.Abs(Vector3.Distance(agent.transform.position,ThrowTarget));
            if (dist > agent.MyStats.Range){
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

    public override bool CheckPreconditions(Creature agent){
        if (agent.HeldItem != null && ThrowTarget != Vector3.zero){
            return true;
        } else {
            return false;
        }
    }

    public override bool PerformEvent(Creature agent){
        float dist = Mathf.Abs(Vector3.Distance(agent.transform.position,ThrowTarget));
        float throwStrength = 1;
        if (dist > agent.MyStats.Range){
            throwStrength = agent.MyStats.Range/dist; //this really only exists for the player, cuz AI companions will always try to move into their throwing range
        }
        agent.HeldItem.GetComponent<IThrowable>().ThrowObject(ThrowTarget,throwStrength);
        agent.HeldItem = null;
        return true;
    }
}
