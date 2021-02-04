using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CreatureLogic
{
    protected override void Awake(){
        base.Awake();
    }

    public override void Init(){
        base.Init();

        availableActions.Add(new MeleeAttack(14,1));//attacking cow
        AddCoreSkills();

        SetGoals();
        GetPlan();
    }

    void SetGoals(){
        myGoals.Enqueue(GameState.State.goalCowAttacked);
    }
   
}
