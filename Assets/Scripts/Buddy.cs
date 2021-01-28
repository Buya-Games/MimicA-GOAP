using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//this sits on every AI companion and
//1) at outset will watch player to learn goals and actions
//2) after X actions, will use GOAPPlan to dynamically create action plans to meet goals
public class Buddy : Goblin
{
    Player player;
    [HideInInspector]public bool learning;
    int learningActions = 10;//# of player actions that creature will observe to learn, then start mimicing
    public float motiveReproduction, motiveHarvest, motiveAttack;//the 3 possible goals of a creature
    
    protected override void Start(){
        base.Start();
        player = GameObject.FindObjectOfType<Player>();
        manager.spawner.ActiveBuddies.Add(this.gameObject);//##REMOVE THIS AFTER YOU FINISH TESTING AND GAME STARTS WITH 0 BUDDIES
        learning = true;
        player.CheckForStudents();
        player.OnTeach += Learn;//tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
        //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
    }

    public override void Init(){ 
        base.Init();

        //give them basic goal
        myGoals.Enqueue(GameState.State.goalFollowPlayer);

        availableActions.Insert(0,new Follow());//follow player)
        // availableActions.Add(new ThrowItem(Vector3.zero,7,13));//eat berries to survive
        GetPlan();
    }

    void Learn(){
        if (availableActions.Count<learningActions){//listen until X actions
            availableActions.Add(player.CurrentEvent.Clone());
            motiveAttack+=player.CurrentEvent.motiveAttack;
            motiveHarvest+=player.CurrentEvent.motiveHarvest;
            motiveReproduction+=player.CurrentEvent.motiveReproduction;
        }
        if (availableActions.Count >= learningActions){//after X, we will stop listening and setup our lifetime goals and GET ON WITH OUR LIVES!
            SetGoals();
            availableActions.Remove(availableActions[0]);//removing the Follow basic action
            learning = false;
            player.OnTeach -= Learn;//turning off listener
            player.CheckForStudents();//telling player to stop teaching unless other students
            GetPlan();
        }
    }

    void SetGoals(){
        StopAllCoroutines();
        myGoals.Clear();
        if (motiveAttack>motiveHarvest){//i'm sure more elegant way to do this, but just grinding thru it guh
            if (motiveAttack>motiveReproduction){
                myGoals.Enqueue(GameState.State.goalAttacked);
            } else if (motiveAttack==motiveReproduction){
                myGoals.Enqueue(GameState.State.goalAttacked);
                myGoals.Enqueue(GameState.State.goalReproduced);
            } else {
                myGoals.Enqueue(GameState.State.goalReproduced);
            }
        } else if (motiveAttack==motiveHarvest && motiveAttack>motiveReproduction){
            myGoals.Enqueue(GameState.State.goalAttacked);
            myGoals.Enqueue(GameState.State.goalHarvested);
        } else if (motiveHarvest>motiveReproduction){
            myGoals.Enqueue(GameState.State.goalHarvested);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest>motiveAttack) {
            myGoals.Enqueue(GameState.State.goalHarvested);
            myGoals.Enqueue(GameState.State.goalReproduced);
        } else if (motiveHarvest==motiveReproduction && motiveHarvest==motiveAttack) {
            myGoals.Enqueue(GameState.State.goalAttacked);
            myGoals.Enqueue(GameState.State.goalHarvested);
            myGoals.Enqueue(GameState.State.goalReproduced);
        } else {
            myGoals.Enqueue(GameState.State.goalReproduced);
        }
        Debug.Log(motiveAttack + " " + motiveHarvest + " " + motiveReproduction + ", " + myGoals.Peek());
    }
}