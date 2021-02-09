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
    int learningActions = 10;//# of player actions that creature will observe to learn, then start mimicing
    public float motiveReproduction, motiveHarvest, motiveAttack, motiveHelper;//the 4 possible goals of a creature
    
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
            availableActions.Insert(0,new Follow());
            availableActions.Add(new Eat(7));
            availableActions.Add(new Eat(9));
            availableActions.Add(new Eat(10));
            availableActions.Add(new Eat(16));
            availableActions.Add(new PickupItem(7,false));
            availableActions.Add(new PickupItem(9,false));
            availableActions.Add(new PickupItem(10,false));
            availableActions.Add(new PickupItem(16,false));
            learningActions = availableActions.Count + 5;//buddies can learn 9 actions from the player
        
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

    void LearnFromAI(){
        availableActions.Clear();

        CreatureLogic learnFrom = null;//manager.spawner.Teachers.Dequeue();
        while (learnFrom == null){
            learnFrom = manager.spawner.Teachers.Dequeue();
        }
        manager.spawner.Teachers.Enqueue(learnFrom);
        foreach (GOAPAct act in learnFrom.availableActions){
            if (!IsDupeAction(act)){
                availableActions.Add(act.Clone());
                //Debug.Log(string.Format("{0} learned {1}{2}-{3} from {4}",myName,a,a.ActionLayer,a.ActionLayer2,manager.spawner.ActiveBuddies[teacher]));
            }
        }

        int teacher = UnityEngine.Random.Range(0,manager.spawner.ActiveBuddies.Count);
        // if (manager.spawner.ActiveBuddies[teacher] != null){
        //     List<GOAPAct> teacherActions = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>().availableActions;
        //     foreach (GOAPAct act in teacherActions){
        //         if (!IsDupeAction(act)){
        //             availableActions.Add(act.Clone());
        //             //Debug.Log(string.Format("{0} learned {1}{2}-{3} from {4}",myName,a,a.ActionLayer,a.ActionLayer2,manager.spawner.ActiveBuddies[teacher]));
        //         }
        //     }
        // }

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
        Debug.Log(myName + " learned random skills");
    }

    void Learn(){
        if (availableActions.Count<learningActions){//listen until X actions
            if (!IsDupeAction(player.CurrentEvent)){
                availableActions.Add(player.CurrentEvent.Clone());
                manager.ui.DisplayAction(transform.position,player.CurrentEvent, 
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
                        manager.ui.DisplayAction(transform.position,newAction, 
                            "<br><size=6>(" + (learningActions-availableActions.Count).ToString() + " left)");
                    }
                    //ImproveCoreHarvestSkill(newAction);
                    return true;
                }
            }
        }
        return false;
    }

    //all creatures by default know how to harvest bush at low efficiency but if they are taught that action again
    //they will overwrite their default action with new one (its how they can improve with better teaching)
    void ImproveCoreHarvestSkill(GOAPAct newHarvest){
        if (newHarvest.GetType() == harvestBerry.GetType()){
            if (newHarvest.ActionLayer == harvestBerry.ActionLayer && newHarvest.ActionLayer2 == harvestBerry.ActionLayer2){
                if (availableActions.Contains(harvestBerry)){
                    availableActions.Remove(harvestBerry);
                    //availableActions.Add(newHarvest.Clone());
                    harvestBerry = newHarvest.Clone() as MeleeAttack;
                    Debug.Log("switched for better melee");
                }
            }
        }
    }

    void SetGoals(){
        StopAllCoroutines();

        TallyGoals();
        myGoals.Clear();
        float[] motives = new float[]{motiveAttack,motiveHarvest,motiveReproduction,motiveHelper};
        Array.Sort(motives);
        string popUp = "";
        if (motives[motives.Length-1] > 0){ //##fuck it everyone just gets one goal for now. too complicated to have multiple goals
            if (motives[motives.Length-1] == motiveHarvest){
                myGoals.Enqueue(GameState.State.goalGatherFood);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.25f);//choose their hair color
                popUp = myName + " is gathering food";
            }
            if (motives[motives.Length-1] == motiveReproduction){
                myGoals.Enqueue(GameState.State.goalGatherShrooms);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.7f);
                popUp = myName + " is gathering shrooms";
            }
            if (motives[motives.Length-1] == motiveAttack){
                myGoals.Enqueue(GameState.State.goalAttackEnemies);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(0.5f);
                popUp = myName + " is defending";
            }
            if (motives[motives.Length-1] == motiveHelper){
                myGoals.Enqueue(GameState.State.goalHelpOthers);
                head.GetComponent<MeshRenderer>().material.color = manager.HeadColor.Evaluate(1);
                popUp = myName + " is helping others";
            }
        }
        Vector3 popUpPos = transform.position;
        popUpPos.y-=10;
        manager.ui.DisplayMessage(popUpPos,popUp);
    }

    void TallyGoals(){
        // availableActions.Remove(harvestBerry);
        // availableActions.Remove(pickupBerry);
        // availableActions.Remove(eatBerry);
        for (int i = 0;i<availableActions.Count;i++){
            motiveAttack+=availableActions[i].motiveAttack;
            motiveHarvest+=availableActions[i].motiveHarvest;
            motiveReproduction+=availableActions[i].motiveReproduction;
            motiveHelper+=availableActions[i].motiveHelper;
        }
        //AddCoreSkills();
    }

    void OnMouseDown(){
        manager.ui.DisplayGUI(this,myGoals);
    }
}