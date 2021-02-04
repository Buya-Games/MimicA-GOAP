using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//this sits on every AI companion and
//1) at outset will watch player to learn goals and actions
//2) after X actions, will use GOAPPlan to dynamically create action plans to meet goals
public class Buddy : CreatureLogic
{
    Player player;
    [HideInInspector]public bool learning;
    int learningActions = 10;//# of player actions that creature will observe to learn, then start mimicing
    public float motiveReproduction, motiveHarvest, motiveAttack;//the 3 possible goals of a creature
    
    protected override void Awake(){
        base.Awake();
        player = GameObject.FindObjectOfType<Player>();
    }

    void Start(){
        Init();
    }

    public override void Init(){ 
        base.Init();

        if (player != null){
            learning = true;
            player.CheckForStudents();
            //tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
            //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
            player.OnTeach += Learn;

            //give them basic goal
            myGoals.Enqueue(GameState.State.goalFollowPlayer);
            availableActions.Insert(0,new Follow());//follow player)
            AddCoreSkills();
            learningActions = availableActions.Count + 7;
            Debug.Log(myName + " availActionsCount " + availableActions.Count + ", learning " + learningActions);
        } else {
            Debug.Log(myName + "learning from ai");
            //if player is dead, learn from random person
            LearnFromAI();
        }
        GetPlan();
        
    }

    void LearnFromAI(){
        availableActions.Clear();

        int teacher = Random.Range(0,manager.spawner.ActiveBuddies.Count);
        if (manager.spawner.ActiveBuddies[teacher] != null){
            List<GOAPAct> teacherActions = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>().availableActions;
            foreach (GOAPAct a in teacherActions){
                if (!IsDupeAction(a)){
                    availableActions.Add(a.Clone());
                    //Debug.Log(string.Format("{0} learned {1}{2}-{3} from {4}",myName,a,a.ActionLayer,a.ActionLayer2,manager.spawner.ActiveBuddies[teacher]));
                }
            }
        }
        while (availableActions.Count < 10){
            teacher = Random.Range(0,manager.spawner.ActiveBuddies.Count);
            CreatureLogic randoTeacher = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>();
            GOAPAct a = randoTeacher.availableActions[Random.Range(0,randoTeacher.availableActions.Count)];
            // Debug.Log(string.Format("{0} learned {1}{2}-{3} from {4}",myName,a,a.ActionLayer,a.ActionLayer2,manager.spawner.ActiveBuddies[teacher]));
            // availableActions.Add(a);
        }
        SetGoals();
    }

    void Learn(){
        if (availableActions.Count<learningActions){//listen until X actions
            if (!IsDupeAction(player.CurrentEvent)){
                availableActions.Add(player.CurrentEvent.Clone());
                // motiveAttack+=player.CurrentEvent.motiveAttack;
                // motiveHarvest+=player.CurrentEvent.motiveHarvest;
                // motiveReproduction+=player.CurrentEvent.motiveReproduction;
                Debug.Log(availableActions.Count + ", added " + player.CurrentEvent);
            }
        }
        if (availableActions.Count >= learningActions){//after X, we will stop listening and setup our lifetime goals and GET ON WITH OUR LIVES!
            SetGoals();
            Debug.Log("removing: " + availableActions[0]);
            availableActions.Remove(availableActions[0]);//removing the Follow basic action
            learning = false;
            player.OnTeach -= Learn;//turning off learning listener
            player.CheckForStudents();//telling player to stop teaching unless other students
            GetPlan();
        }
    }

    bool IsDupeAction(GOAPAct newAction){//checks if I know this already
        foreach (var act in availableActions){
            if (newAction.GetType() == act.GetType()){
                if (act.ActionLayer == newAction.ActionLayer && act.ActionLayer2 == newAction.ActionLayer2){
                    learningActions--;//buddy was taught a dupe, expending a learning opportunity
                    Debug.Log(newAction + " was dupe");
                    return true;
                }
            }
        }
        return false;
    }

    void SetGoals(){
        StopAllCoroutines();

        TallyGoals();
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

    void TallyGoals(){
        availableActions.Remove(harvestBerry);
        availableActions.Remove(pickupBerry);
        availableActions.Remove(eatBerry);
        for (int i = 0;i<availableActions.Count;i++){
            motiveAttack+=availableActions[i].motiveAttack;
            motiveHarvest+=availableActions[i].motiveHarvest;
            motiveReproduction+=availableActions[i].motiveReproduction;
        }
        AddCoreSkills();
    }
}