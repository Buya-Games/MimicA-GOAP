using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//this sits on every AI companion and
//1) at outset will watch player to learn goals and actions
//2) after X actions, will use GOAPPlan to dynamically create action plans to meet goals
public class Buddy : CreatureLogic
{
    Player player;
    [HideInInspector]public bool learning;
    int learningActions;//# of player actions that creature will observe to learn, then start mimicing
    public float motiveReproduction, motiveHarvest, motiveAttack, motiveHelper;//the 4 possible goals of a creature
    List<GOAPAct> learnedActs = new List<GOAPAct>();
    
    protected override void Awake(){
        base.Awake();
        player = GameObject.FindObjectOfType<Player>();
        
    }

    void Start(){
        Init();
        
    }

    public override void Init(){ 
        base.Init();

        //if player is alive
        if (manager.PlayerAlive){
            learning = true;
            player.CheckForStudents();
            //tells companion to listen everytime Player.cs uses OnTeach, and in those cases to run Learn
            //i could implement interfaces on this, but I think this is fine since it's just a few GOAPActs to manage
            player.OnTeach += Learn;

            //give buddies the basic goal of following the player
            myGoals.Enqueue(GameState.State.goalFollowPlayer);
            availableActions.Insert(0,new Follow());//0
            availableActions.Add(new Eat(7));
            availableActions.Add(new MeleeAttack(6,1));//2
            // availableActions.Add(new MeleeAttack(8,1));//3
            // availableActions.Add(new MeleeAttack(11,1));//4
            availableActions.Add(new PickupItem(7,false));//5
            // availableActions.Add(new PickupItem(9,false));
            // availableActions.Add(new PickupItem(10,false));
            // availableActions.Add(new PickupItem(16,false));
            // availableActions.Add(new ThrowItem(Vector3.zero,7,12));
            // availableActions.Add(new ThrowItem(Vector3.zero,7,14));//10
            // availableActions.Add(new ThrowItem(Vector3.zero,9,12));
            // availableActions.Add(new ThrowItem(Vector3.zero,9,14));
            // availableActions.Add(new ThrowItem(Vector3.zero,10,4));
            // availableActions.Add(new ThrowItem(Vector3.zero,10,11));
            // availableActions.Add(new ThrowItem(Vector3.zero,16,4));//15
            // availableActions.Add(new ThrowItem(Vector3.zero,16,11));
            learningActions = manager.buddyLearningActions;//availableActions.Count + 5;//buddies can learn 9 actions from the player
        
        //if player is dead, learn from another AI
        } else {
            if (manager.spawner.ActiveBuddies.Count > 1){
                LearnFromAI();   
            } else {
                LearnRandomSkills();
            }
        }
        GetPlan();
    }

    void Learn(){
        if (!manager.Tutorial){//if live game
            if (availableActions.Count<learningActions){//listen until X actions
                if (!IsDupeAction(player.CurrentAction)){
                    availableActions.Add(player.CurrentAction.Clone());
                    manager.ui.DisplayAction(transform.position,player.CurrentAction,Color.white,
                    "<br><size=6>(" + (learningActions-availableActions.Count).ToString() + " left)");
                }    
            }
            if (availableActions.Count >= learningActions){//after X, we will stop listening and setup our lifetime goals and GET ON WITH OUR LIVES!
                SetGoals();
                availableActions.Remove(availableActions[0]);//removing the Follow basic action
                learning = false;
                player.OnTeach -= Learn;//turning off learning listener
                player.CheckForStudents();//telling player to stop teaching unless other students
                GetPlan();
            }
            // if (learnedActs.Count<learningActions){//listen until X actions
            //     learnedActs.Add(player.CurrentAction.Clone());
            //     manager.ui.DisplayAction(transform.position,player.CurrentAction, 
            //         "<br><size=6>(" + (learningActions-learnedActs.Count).ToString() + " left)");
            //     // if (!IsDupeAction(player.CurrentAction)){
            //     //     availableActions.Add(player.CurrentAction.Clone());
                
            //     // }    
            // }
            // if (learnedActs.Count >= learningActions){//after X, we will stop listening and setup our lifetime goals and GET ON WITH OUR LIVES!
            //     SetGoals();
            //     //availableActions.Remove(availableActions[0]);//removing the Follow basic action
            //     learning = false;
            //     player.OnTeach -= Learn;//turning off learning listener
            //     player.CheckForStudents();//telling player to stop teaching unless other students
            //     GetPlan();
            // }
        } else {
            if (manager.tut.Tut8TeachAny){
                manager.tut.Tut8TeachAny = false;
                manager.tut.Tut9TeachFeedPlayer = true;
                manager.tut.DisplayNextTip(9);//throw berry at me
                manager.tut.SpawnBush();
            }
            if (manager.tut.Tut9TeachFeedPlayer){
                ThrowItem test = new ThrowItem(Vector3.zero,7,13);
                if (player.CurrentAction.GetType() == test.GetType() && 
                    player.CurrentAction.ActionLayer == test.ActionLayer && player.CurrentAction.ActionLayer2 == test.ActionLayer2){
                        manager.tut.Tut9TeachFeedPlayer = false;
                        manager.tut.DisplayNextTip(10);//final message
                        manager.tut.EndTutorial();
                }
            }
        }
        //end tutorial shit
    }

    void LearnFromAI(){
        availableActions.Clear();

        CreatureLogic learnFrom = null;
        while (learnFrom == null){
            learnFrom = manager.spawner.Teachers.Dequeue();
        }
        manager.spawner.Teachers.Enqueue(learnFrom);
        foreach (GOAPAct act in learnFrom.availableActions){
            if (!IsDupeAction(act)){
                availableActions.Add(act.Clone());
            }
        }

        int teacher = UnityEngine.Random.Range(0,manager.spawner.ActiveBuddies.Count);

        //learn one more random thing from a random person
        teacher = UnityEngine.Random.Range(0,manager.spawner.ActiveBuddies.Count);
        CreatureLogic randoTeacher = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>();
        if (randoTeacher != null && randoTeacher.availableActions.Count > 0){
            GOAPAct a = randoTeacher.availableActions[UnityEngine.Random.Range(0,randoTeacher.availableActions.Count)];
            availableActions.Add(a);
        } else {
            LearnRandomSkills();
            return;
        }
        
        // while (availableActions.Count < 10){
        //     teacher = UnityEngine.Random.Range(0,manager.spawner.ActiveBuddies.Count);
        //     CreatureLogic randoTeacher = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>();
        //     GOAPAct a = randoTeacher.availableActions[UnityEngine.Random.Range(0,randoTeacher.availableActions.Count)];
        //     // Debug.Log(string.Format("{0} learned {1}{2}-{3} from {4}",myName,a,a.ActionLayer,a.ActionLayer2,manager.spawner.ActiveBuddies[teacher]));
        //     // availableActions.Add(a);
        // }
        SetGoals();
    }

    void LearnRandomSkills(){
        //not sure if this ever occurs (or should occur), but if it does the buddy learns EVERYTHING to become super smart
        availableActions.Add(new Eat(7));//0
        //availableActions.Add(new Eat(9));
        // availableActions.Add(new Eat(10));
        // availableActions.Add(new Eat(16));
        availableActions.Add(new MeleeAttack(6,UnityEngine.Random.Range(1,3)));
        availableActions.Add(new MeleeAttack(8,UnityEngine.Random.Range(1,3)));
        availableActions.Add(new PickupItem(9,false));
        availableActions.Add(new PickupItem(10,false));
        availableActions.Add(new PickupItem(16,false));
        availableActions.Add(new ThrowItem(Vector3.zero,7,12));
        availableActions.Add(new ThrowItem(Vector3.zero,7,14));
        availableActions.Add(new ThrowItem(Vector3.zero,9,12));
        availableActions.Add(new ThrowItem(Vector3.zero,9,14));
        availableActions.Add(new ThrowItem(Vector3.zero,10,4));
        availableActions.Add(new ThrowItem(Vector3.zero,10,11));
        availableActions.Add(new ThrowItem(Vector3.zero,16,4));
        availableActions.Add(new ThrowItem(Vector3.zero,16,11));
        Debug.Log(myName + " learned random skills");
    }

    public void SwitchToAILearn(){
        Debug.Log(myName + " switching to AI learning");
        availableActions.Remove(availableActions[0]);//removing the Follow basic action
        learning = false;
        player.OnTeach -= Learn;//turning off learning listener
        player.CheckForStudents();//telling player to stop teaching unless other students
        LearnFromAI();
    }

    bool IsDupeAction(GOAPAct newAction){//checks if I know this already
        foreach (var act in availableActions){
            if (newAction.GetType() == act.GetType()){
                if (act.ActionLayer == newAction.ActionLayer && act.ActionLayer2 == newAction.ActionLayer2){
                    learningActions--;//buddy was taught a dupe, expending a learning opportunity
                    if (manager.PlayerAlive){
                        manager.ui.DisplayAction(transform.position,newAction,Color.white,
                            "<br><size=6>(" + (learningActions-availableActions.Count).ToString() + " left)");
                    }
                    ImproveCoreHarvestSkill(newAction);
                    return true;
                }
            }
        }
        return false;
    }

    //all creatures by default know how to harvest bush at low efficiency but if they are taught that action again
    //they will overwrite their default action with new one (its how they can improve with better teaching)
    void ImproveCoreHarvestSkill(GOAPAct newHarvest){
        MeleeAttack hitBush = new MeleeAttack(6,1);
        if (newHarvest.GetType() == hitBush.GetType()){
            if (newHarvest.ActionLayer == hitBush.ActionLayer && newHarvest.ActionLayer2 == hitBush.ActionLayer2){
                availableActions[2] = newHarvest;
                // if (availableActions.Contains(harvestBerry)){
                //     availableActions.Remove(harvestBerry);
                //     //availableActions.Add(newHarvest.Clone());
                //     harvestBerry = newHarvest.Clone() as MeleeAttack;
                //     Debug.Log("switched for better melee");
                // }
            }
        }
    }

    //if player teaches buddy to hit more effectively, then learn that
    void LearnHigherSkill(){
        MeleeAttack hitBush = new MeleeAttack(6,1);
        MeleeAttack hitShroom = new MeleeAttack(8,1);
        MeleeAttack hitEnemy = new MeleeAttack(11,1);
        for (int i = 0;i<learnedActs.Count;i++){
            if (learnedActs[i].GetType() == hitBush.GetType()){
                availableActions[2] = learnedActs[i];
            }
            if (learnedActs[i].GetType() == hitShroom.GetType()){
                availableActions[3] = learnedActs[i];
            }
            if (learnedActs[i].GetType() == hitEnemy.GetType()){
                availableActions[4] = learnedActs[i];
            }
        }
    }

    void SetGoals(){
        StopAllCoroutines();

        TallyGoals();
        //LearnHigherSkill();
        myGoals.Clear();
        float[] motives = new float[]{motiveAttack,motiveHarvest,motiveReproduction,motiveHelper};
        Array.Sort(motives);
        string popUpMessage = "";
        Color popUpColor = Color.white;
        if (motives[motives.Length-1] > 0){ //##fuck it everyone just gets one goal for now. too complicated to have multiple goals
            if (motives[motives.Length-1] == motiveHarvest){
                myGoals.Enqueue(GameState.State.goalGatherFood);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.25f);//choose their hair color
                popUpMessage = myName + " is gathering food";
                popUpColor = manager.HeadColor.Evaluate(0.25f);
            }
            if (motives[motives.Length-1] == motiveReproduction){
                myGoals.Enqueue(GameState.State.goalGatherShrooms);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.7f);
                popUpMessage = myName + " is gathering shrooms";
                popUpColor = manager.HeadColor.Evaluate(0.7f);
            }
            if (motives[motives.Length-1] == motiveAttack){
                myGoals.Enqueue(GameState.State.goalAttackEnemies);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.5f);
                popUpMessage = myName + " is defending";
                popUpColor = manager.HeadColor.Evaluate(0.5f);
            }
            if (motives[motives.Length-1] == motiveHelper){
                myGoals.Enqueue(GameState.State.goalHelpOthers);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(1);
                popUpMessage = myName + " is helping others";
                popUpColor = manager.HeadColor.Evaluate(1f);
            }
        }
        Vector3 popUpPos = transform.position;
        popUpPos.y-=5;
        manager.ui.DisplayMessage(popUpPos,popUpMessage,popUpColor);
    }

    void TallyGoals(){
        for (int i = 0;i<availableActions.Count;i++){
            motiveAttack+=availableActions[i].motiveAttack;
            motiveHarvest+=availableActions[i].motiveHarvest;
            motiveReproduction+=availableActions[i].motiveReproduction;
            motiveHelper+=availableActions[i].motiveHelper;
        }
        // for (int i = 0;i<learnedActs.Count;i++){
        //     motiveAttack+=learnedActs[i].motiveAttack;
        //     motiveHarvest+=learnedActs[i].motiveHarvest;
        //     motiveReproduction+=learnedActs[i].motiveReproduction;
        //     motiveHelper+=learnedActs[i].motiveHelper;
        // }
    }

    void OnMouseDown(){
        manager.ui.DisplayGUI(this,myGoals);
    }
}