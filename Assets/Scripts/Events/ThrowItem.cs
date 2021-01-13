using UnityEngine;
public class ThrowItem : FrameworkEvent
{
    static float BaseDamage = 1f;
    static float BaseRange = 10f;
    public Vector3 ThrowTarget;
    public int ThrowObjectLayer;
    public ThrowItem(Vector3 mousePos, int targetLayer){
        ThrowTarget = mousePos;
        ThrowObjectLayer = targetLayer;
    }

    public override FrameworkEvent Clone(){
        ThrowItem clone = new ThrowItem(this.ThrowTarget,this.ThrowObjectLayer);
        return clone;
        //throw new System.NotImplementedException();
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
            throwStrength = agent.MyStats.Range/dist;
        }
        agent.HeldItem.GetComponent<IThrowable>().ThrowObject(ThrowTarget,throwStrength);
        agent.HeldItem = null;
        return true;
    }
}
