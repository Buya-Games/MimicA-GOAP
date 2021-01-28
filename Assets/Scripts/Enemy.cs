using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature
{
    protected override void Awake(){
        base.Awake();
    }

    public override void Init(){
        base.Init();
        MyStats.Speed = 1;

        availableActions.Add(new MeleeAttack(14));//attacking cow

        SetGoals();
        GetPlan();
    }

    void SetGoals(){
        myGoals.Enqueue(GameState.State.goalCowAttacked);
    }
   
}
