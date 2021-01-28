using UnityEngine;
using System.Collections.Generic;
public class MeleeAttack : FrameworkEvent
{
    static float BaseDamage = 1f;
    // public System.Type TargetType;

    public MeleeAttack(int targetLayer){
        EventRange = 3f;//distance necessary to melee someone
        EventLayer = targetLayer;
        Preconditions.Add(GameState.State.itemNone); 
        if (EventLayer == 6){//if attacking bush
            motiveHarvest++;
            Preconditions.Add(GameState.State.availBush);
            Effects.Add(GameState.State.availBerry);
        }
        if (EventLayer == 8){//if attacking mushroom
            motiveReproduction++;
            Preconditions.Add(GameState.State.availMushroom);
            Effects.Add(GameState.State.availFungus);
        }
        if (EventLayer == 11){//if attacking enemy
            motiveAttack++;
            EventCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
            Preconditions.Add(GameState.State.availEnemy);
            Effects.Add(GameState.State.goalAttacked);
        }
        // if (EventLayer == 12){//if attacking buddy
        //     motiveAttack++;
        //     EventCost*=2;//cost of attacking an enemy with melee should be higher than attacking them with bomb
        //     Preconditions.Add(GameState.State.availEnemy);
        //     Effects.Add(GameState.State.goalAttacked);
        // }
        if (EventLayer == 14){//if attacking cow
            motiveAttack++;
            //Preconditions.Add(GameState.State.availCow);
            Effects.Add(GameState.State.goalCowAttacked);
        }
        Effects.Add(GameState.State.itemNone);
    }

    public override FrameworkEvent Clone(){
        MeleeAttack clone = new MeleeAttack(this.EventLayer);
        return clone;
    }

    public override bool CheckRange(Creature agent){
        if (agent.Target != null && Tools.GetDist(agent.Target,agent.gameObject) < EventRange){
            return true;
        } else {
            return false;
        }
    }

    public override bool GetTarget(Creature agent){
        agent.Target = FindClosestObjectOfLayer(agent.gameObject);
        return agent.Target != null? true : false;
    }

    public override GameObject FindClosestObjectOfLayer(GameObject agent){
        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        GameObject target = null;
        if (EventLayer == 6){
            target = Tools.FindClosestObjectInList(manager.spawner.ActivesBushes,agent.gameObject);
        }
        if (EventLayer == 8){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveMushrooms,agent.gameObject);
        }
        if (EventLayer == 11){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveEnemies,agent.gameObject);
        }
        if (EventLayer == 12){
            target = Tools.FindClosestObjectInList(manager.spawner.ActiveBuddies,agent.gameObject);
        }
        if (EventLayer == 13){//player
            target = Tools.FindClosestObjectOfLayer(13,agent);
        }
        if (EventLayer == 14){//cow
            target = Tools.FindClosestObjectOfLayer(14,agent);
        }
        return target;
    }

    public override bool CheckPreconditions(List<GameState.State> currentState){
        if (GameState.CompareStates(Preconditions,currentState)){
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
        //TargetLayer = agent.Target.gameObject.layer;
        // TargetType = agent.Target.GetType();

        GameManager manager = GameObject.FindObjectOfType<GameManager>();
        if (EventLayer == 6){//if harvesting Bush
            manager.spawner.SpawnEnvironment(agent.Target.transform.position,Spawner.EnvironmentType.Berry);
            manager.spawner.DespawnEnvironment(agent.Target,Spawner.EnvironmentType.Bush);
            manager.particles.DestroyingBush(agent.Target.transform.position);
        }
        if (EventLayer == 8){//if harvesting Mushroom
            manager.spawner.SpawnEnvironment(agent.Target.transform.position,Spawner.EnvironmentType.Fungus);
            manager.spawner.DespawnEnvironment(agent.Target,Spawner.EnvironmentType.Mushroom);
            manager.particles.DestroyingMushroom(agent.Target.transform.position);
        }
        if (EventLayer == 11 || EventLayer == 12 || EventLayer == 13 || EventLayer == 14){//if attacking a creature or the cow
            agent.Target.GetComponent<IHittable>().TakeHit(agent.gameObject,BaseDamage);
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
