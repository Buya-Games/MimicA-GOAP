using UnityEngine;
using System.Collections.Generic;
public class MeleeAttack : GOAPAct
{
    float baseSkill = 0;

    public MeleeAttack(int targetLayer, float playerAbility){
        Init();
        eventRange = 3f;//distance necessary to melee someone
        ActionLayer = targetLayer;
        baseSkill = playerAbility;//player performance during training sets the action's base effectiveness
        Preconditions.Add(GameState.State.itemNone); 
        if (ActionLayer == 6){//if attacking bush
            motiveHarvest++;
            Preconditions.Add(GameState.State.availBush);
            Effects.Add(GameState.State.availBerry);
        }
        if (ActionLayer == 8){//if attacking mushroom
            motiveReproduction++;
            Preconditions.Add(GameState.State.availMushroom);
            Effects.Add(GameState.State.availFungus);
        }
        if (ActionLayer == 11){//if attacking enemy
            motiveAttack++;
            coreCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttacked);
        }
        if (ActionLayer == 12){//if attacking buddy (for enemies)
            motiveAttack++;
            coreCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttacked);
        }
        if (ActionLayer == 14){//if attacking cow
            motiveAttack++;
            //Preconditions.Add(GameState.State.availCow);
            Effects.Add(GameState.State.goalCowAttacked);
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override GOAPAct Clone(){
        MeleeAttack clone = new MeleeAttack(this.ActionLayer, this.baseSkill);
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
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        return agent.Target != null? true : false;
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameObject target = null;
        if (ActionLayer == 6){
            target = Tools.FindClosestObjectInList(manager.spawner.ActivesBushes,agent.gameObject);
        }
        if (ActionLayer == 8){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveMushrooms,agent.gameObject);
        }
        if (ActionLayer == 11){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        }
        if (ActionLayer == 12){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBuddies,agent.gameObject);
        }
        if (ActionLayer == 13){//player
            target = Tools.FindClosestObjectOfLayer(13,agent);
        }
        if (ActionLayer == 14){//cow
            target = Tools.FindClosestObjectOfLayer(14,agent);
        }
        return target;
    }

    public override bool PerformEvent(Creature agent){
        int hitStrength = 0;
        if (agent is Player){
            Debug.Log(baseSkill);
            if (baseSkill < 1.3f){
                hitStrength = 2;
            } else {
                hitStrength = 1;
            }
            //hitStrength = (int)baseSkill;
        } else {
            float s = baseSkill * Random.Range(0.8f,1.2f);
            if (s < 1.5f){
                hitStrength = 2;
            } else {
                hitStrength = 1;
            }
            //hitStrength = (int)(baseSkill * Random.Range(0.8f,1.2f));
        }
        

        GameManager manager = GameObject.FindObjectOfType<GameManager>();
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
        agent.Swing();
        CompleteEvent(agent);
        return true;
    }

    protected override bool CompleteEvent(Creature agent){
        agent.Target = null;//is this necessary? i guess for bush or mushroom, but not enemy unless they die?
        return true;
    }
}
