using UnityEngine;
public class MeleeAttack : GOAPAct
{
    public MeleeAttack(int targetLayer, float playerAbility){
        Init();
        eventRange = 3f;//distance necessary to melee someone
        ActionLayer = targetLayer;
        ActionSkill = playerAbility;//player performance during training sets the action's base effectiveness
        Preconditions.Add(GameState.State.itemNone); 
        if (ActionLayer == 6){//if attacking bush
            Preconditions.Add(GameState.State.availBush);
            Effects.Add(GameState.State.availBerry);
            motiveHarvest++;
        }
        if (ActionLayer == 8){//if attacking mushroom
            motiveReproduction++;
            Preconditions.Add(GameState.State.availMushroom);
            Effects.Add(GameState.State.availFungus);
        }
        if (ActionLayer == 11){//if attacking enemy
            motiveAttack++;
            ActionSkill*=2;//inflict more damage to an enemy (EZ mode)
            //coreCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttackEnemies);
        }
        if (ActionLayer == 12){//if attacking buddy (for enemies)
            motiveAttack++;
            //coreCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttackEnemies);
        }
        if (ActionLayer == 14){//if attacking cow
            motiveAttack++;
            //Preconditions.Add(GameState.State.availCow);
            Effects.Add(GameState.State.goalAttackCow);
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override GOAPAct Clone(){
        MeleeAttack clone = new MeleeAttack(this.ActionLayer, this.ActionSkill);
        return clone;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < eventRange){
            return true;
        } else {
            return false;
        }
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

    protected override GameObject FindClosestObjectOfLayer(Creature agent){
        GameObject target = null;
        ITargettable itarget = null;
        if (ActionLayer == 6){
            //need to use this thing to search for closest + check ownership of item. its kinda messy but works
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveBushes),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
        }
        if (ActionLayer == 8){
            itarget = Tools.FindClosestTargetInList(Tools.ConvertToITargettable(manager.spawner.ActiveMushrooms),agent.gameObject);
            if (itarget != null){
                target = itarget.gameObj;
            }
        }
        if (ActionLayer == 11){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        }
        if (ActionLayer == 12){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBuddies,agent.gameObject);
        }
        if (ActionLayer == 13){//player
            target = player;
        }
        if (ActionLayer == 14){//cow
            target = cow;
        }
        return target;
    }

    public override bool PerformEvent(Creature agent){
        int hitStrength = 0;
        if (ActionSkill < 1.3f){
            hitStrength = 2;
        } else {
            hitStrength = 1;
        }

        //tutorial shit
        if (agent is Player){
            if (manager.Tutorial && manager.tut.Tut3HardMelee){
                if (hitStrength > 1){
                    manager.tut.Tut3HardMelee = false;
                    manager.tut.Tut4ThrowBerryCow = true;
                    manager.tut.DisplayNextTip(4);//throw berry at cow
                } else {
                    manager.tut.TryAgain();
                }
            }
        }
        //end tutorial shit
        
        
        if (ActionLayer == 6){//if harvesting Bush
            for (int i = 0;i<hitStrength;i++){
                manager.spawner.SpawnEnvironment(agent.Target.transform.position,Spawner.EnvironmentType.Berry);
            }
            manager.spawner.DespawnEnvironment(agent.Target,Spawner.EnvironmentType.Bush);
            manager.particles.DestroyingBush(agent.Target.transform.position);
        }
        if (ActionLayer == 8){//if harvesting Mushroom
            for (int i = 0;i<hitStrength;i++){
                manager.spawner.SpawnEnvironment(agent.Target.transform.position,Spawner.EnvironmentType.Fungus);
            }
            manager.spawner.DespawnEnvironment(agent.Target,Spawner.EnvironmentType.Mushroom);
            manager.particles.DestroyingMushroom(agent.Target.transform.position);
        }
        if (ActionLayer == 11 || ActionLayer == 12 || ActionLayer == 13 || ActionLayer == 14){//if attacking a creature or the cow
            agent.Target.GetComponent<IHittable>().TakeHit(agent.gameObject,hitStrength);
        }
        agent.Swing(hitStrength);
        CompleteEvent(agent);
        return true;
    }

    protected override bool CompleteEvent(Creature agent){
        agent.ClearTarget();//is this necessary? i guess for bush or mushroom, but not enemy unless they die?
        return true;
    }
}
