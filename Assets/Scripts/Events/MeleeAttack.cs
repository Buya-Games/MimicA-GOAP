using UnityEngine;
public class MeleeAttack : FrameworkEvent
{
    static float AttackRange = 3f;
    static float BaseDamage = 1f;
    // public System.Type TargetType;

    public override FrameworkEvent Clone(){
        MeleeAttack clone = new MeleeAttack();
        clone.MyItem = this.MyItem;
        clone.MyRequiredRange = this.MyRequiredRange;
        clone.TargetLayer = this.TargetLayer;
        return clone;
    }
    public override bool CheckRange(Creature agent){
        if (agent.Target != null && agent.TargetDist < AttackRange){
            return true;
        } else {
            return false;
        }
    }

    public override bool CheckPreconditions(Creature agent){
        if (agent.Target != null && agent.TargetDist < AttackRange){
            return true;
        } else {
            return false;
        }
    }
    public override bool PerformEvent(Creature agent){
        //TargetLayer = agent.Target.gameObject.layer;
        // TargetType = agent.Target.GetType();

        if (TargetLayer == 6){//if attacking Enemy
            agent.Target.GetComponent<Enemy>().TakeHit(BaseDamage);

        }
        if (TargetLayer == 7){//if harvesting Rock

        }
        if (TargetLayer == 8){//if harvesting Bush
            GameObject.FindObjectOfType<Spawner>().SpawnEnvironment(agent.Target.transform.position,Spawner.EnvironmentType.Berry);
            GameObject.FindObjectOfType<Spawner>().DespawnEnvironment(agent.Target,Spawner.EnvironmentType.Bush);
        }
        agent.Swing();
        CompleteEvent(agent);
        return true;
    }

    protected override bool CompleteEvent(Creature agent){
        agent.Target = null;
        return true;
    }
}
