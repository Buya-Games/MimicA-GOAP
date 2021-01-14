using UnityEngine;
public class PickupBerry : FrameworkEvent
{
    static float PickupRange = 2f;

    public override FrameworkEvent Clone(){
        PickupBerry clone = new PickupBerry();
        clone.MyItem = this.MyItem;
        clone.MyRequiredRange = this.MyRequiredRange;
        clone.TargetLayer = this.TargetLayer;
        return clone;
    }
    public override bool CheckRange(Creature agent){
        if (agent.Target != null && agent.TargetDist < PickupRange){
            return true;
        } else {
            return false;
        }
    }

    public override bool CheckPreconditions(Creature agent){
        if (agent.HeldItem == null){
            return true;
        } else {
            return false;
        }
    }
    public override bool PerformEvent(Creature agent){
        agent.HeldItem = agent.Target;
        Vector3 pos = agent.transform.position;
        pos.y += 3;
        agent.Target.transform.position = pos;
        agent.Target.transform.SetParent(agent.transform);
        agent.Target.transform.rotation = Quaternion.identity;
        agent.Target.transform.localScale = new Vector3(0.5f,0.25f,0.5f);
        agent.Target.GetComponent<Rigidbody>().isKinematic = true;
        CompleteEvent(agent);
        Debug.Log("finished picking up berry");
        return true;
    }

    protected override bool CompleteEvent(Creature agent){
        agent.Target = null;
        return true;
    }
}
