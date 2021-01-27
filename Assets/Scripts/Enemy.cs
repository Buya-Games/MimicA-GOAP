using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature
{
    Cow cow;
    Queue<GameState.State> MyGoals = new Queue<GameState.State>();
    Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    FrameworkEvent currentEvent;
    float eventMaxTime = 10; //don't spend more than 10 seconds on any given decision

    protected override void Awake(){
        base.Awake();
        cow = FindObjectOfType<Cow>();
        Init();
    }

    protected override void Init(){
        base.Init();
        MyStats.Speed = 2;

        MeleeAttack melee = new MeleeAttack(14);//attacking cow
        availableActions.Add(melee);

        SetGoals();
        GetPlan();
    }

    void SetGoals(){
        MyGoals.Enqueue(GameState.State.goalCowAttacked);
    }

    List<GameState.State> ToList(Queue<GameState.State> q){
        List<GameState.State> toList = new List<GameState.State>();
        GameState.State s = q.Dequeue();
        toList.Add(s);
        q.Enqueue(s);
        return toList;
    }

    void GetPlan(){
        toDo = planner.MakePlan(this,GetCurrentState(),ToList(MyGoals));

        if (toDo != null){
            GetDecision();
        } else {
            Invoke("GetPlan",1f);
        }
    }

    void GetDecision(){
        currentEvent = null;
        if (toDo.Count>0){
            currentEvent = toDo.Dequeue();
            PerformDecision(currentEvent);
        } else {
            GetPlan();
        }
    }

    void PerformDecision(FrameworkEvent nextEvent){
        if (Time.time > eventMaxTime){
            eventMaxTime+=Time.time;
            GetDecision();
        } else {
            if (nextEvent.GetTarget(this)){
                if (nextEvent.CheckRange(this)){
                    if (!nextEvent.PerformEvent(this)){
                        Debug.Log(nextEvent + " FAILED");//if failed, just say it and move on
                    }
                    Invoke("GetDecision",.5f);//once this action is complete (both success and fail)
                } else {//if not in range
                    StopAllCoroutines();
                    StartCoroutine(Movement((Target.transform.position - transform.position).normalized));
                }
            } else {
                Invoke("GetPlan",1f);//if can't find any targets, get a new plan
            }
        }
    }

    IEnumerator Movement(Vector3 dir){
        StartCoroutine(FaceTarget(dir));
        int counter = 0;
        rb.velocity = Vector3.zero;
        while (counter < 50){
            rb.MovePosition(rb.position + dir * Time.fixedDeltaTime * MyStats.Speed/2);
            counter++;
            yield return null;
        }
        PostMovementChecks();
    }

    void PostMovementChecks(){
        PerformDecision(currentEvent);
        // Queue<FrameworkEvent> quickCheck = planner.MakePlan(this,GetCurrentState(),ToList(MyGoals));
        // if (currentEvent != null && quickCheck.Peek() == currentEvent){
        //     if (currentEvent.CheckPreconditions(GetCurrentState())){
        //         PerformDecision(currentEvent);
        //     } else {
        //         Invoke("GetPlan",0.5f); //seems can no longer perform the current event, so getting a new plan
        //     }
        // } else {
        //     toDo = quickCheck;
        //     GetDecision();
        // }
    }
   
}
