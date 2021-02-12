using UnityEngine;
using System;

//determines actions of a friendly AI 
//1) at outset will watch player to learn actions and motives
//2) after X actions, will formulate goal from motives and use GOAPPlan (via CreatureLogic) to dynamically create action plans to meet goals
public class Buddy : CreatureLogic
{
    [HideInInspector]public bool learning;
    int learningActions;//# of player actions creature will observe to learn before acting on their own
    public float motiveReproduction, motiveHarvest, motiveAttack, motiveHelper;//the 4 possible goals of a creature

    void Start(){
        Init();
    }

    public override void Init(){ 
        base.Init();

        //if player is alive
        if (manager.PlayerAlive){
            learning = true;
            learningActions = manager.buddyLearningActions;
            manager.students.Add(this);
            manager.player.CheckForStudents();

            //tells companion to listen everytime Player.cs uses OnTeach, and in those cases to run Learn
            //i could implement interfaces on this, but I think this is fine since it's just a few GOAPActs to manage
            manager.player.OnTeach += Learn;

            //give buddies core skills to survive, learn
            myGoals.Enqueue(GameState.State.goalFollowPlayer);
            availableActions.Insert(0,new Follow());
            availableActions.Add(new Eat(7));
            availableActions.Add(new MeleeAttack(6,2));
            availableActions.Add(new PickupItem(7,false));//false dictates this action should't contribute to motives (cuz urrbody got a motive to eat)
        
        //if player is dead, learn from another AI
        } else {
            if (manager.spawner.ActiveBuddies.Count > 1){ //if at least 1 other AI besides me
                LearnFromAI();   
            } else {
                LearnRandomSkills();
            }
        }
        GetPlan();
    }

    void Learn(){
        if (availableActions.Count<learningActions){//listen until X actions
            if (!IsDupeAction(manager.player.CurrentAction)){//if learning action is not a dupe
                availableActions.Add(manager.player.CurrentAction.Clone());//add it to my learned actions
                
                manager.ui.DisplayAction(transform.position,manager.player.CurrentAction,Color.white,
                "<br><size=6>(" + (learningActions-availableActions.Count).ToString() + " left)");
            }
            //tutorial shit
            
        }
        if (availableActions.Count >= learningActions){//after X, stop listening, tally motives, create lifetime goals and GET ON WITH LIFE!

            learning = false;
            manager.player.OnTeach -= Learn;//turning off learning listener
            if (manager.students.Contains(this)){
                manager.students.Remove(this);
            }
            manager.player.CheckForStudents();//telling player to stop teaching unless other students

            SetGoals();
            availableActions.Remove(availableActions[0]);//removing the Follow action cuz don't need it anymore and every unnecessary actions slows GOAPPlan

            GetPlan();

            if (availableActions.Count > 7){
                if (manager.Tutorial && manager.tut.Tut8TeachAny){
                    manager.tut.Tut8TeachAny = false;
                    //manager.tut.Tut9TeachFeedPlayer = true;
                    manager.tut.DisplayNextTip(9);//throw berry at me
                    manager.tut.SpawnBush();
                    manager.tut.EndTutorial();
                }
                //end tutorial shit
            }
        }
    }

    public void LearnFromAI(){
        //select a teacher (using queue instead of rando so we get a healthy balance of peeps)
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

        //learn one more random action from a random AI just to keep it spicy
        int teacher = UnityEngine.Random.Range(0,manager.spawner.ActiveBuddies.Count);
        CreatureLogic randoTeacher = manager.spawner.ActiveBuddies[teacher].GetComponent<CreatureLogic>();
        if (randoTeacher != null && randoTeacher.availableActions.Count > 0){
            GOAPAct a = randoTeacher.availableActions[UnityEngine.Random.Range(0,randoTeacher.availableActions.Count)];
            availableActions.Add(a);
        } else {
            LearnRandomSkills();
            return;
        }
        SetGoals();
    }

    //this is actually super OP cuz they learn all possible actions except throwing to each other
    void LearnRandomSkills(){
        availableActions.Add(new Eat(7));
        //availableActions.Add(new Eat(9));//they shouldn't eat mushroom and poop unless you explicitly teach them
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
        SetGoals();
    }

    public void SwitchToAILearn(){
        learning = false;
        manager.player.OnTeach -= Learn;//turning off learning listener
        manager.player.CheckForStudents();//telling player to stop teaching unless other students
        LearnFromAI();
    }

    bool IsDupeAction(GOAPAct newAction){//checks if I know this already
        foreach (var act in availableActions){
            if (newAction.GetType() == act.GetType()){
                if (act.ActionLayer == newAction.ActionLayer && act.ActionLayer2 == newAction.ActionLayer2){
                    learningActions--;//buddy was taught a dupe, expending a learning opportunity
                    if (manager.PlayerAlive){
                        ImproveCoreHarvestSkill(newAction);
                        manager.ui.DisplayAction(transform.position,newAction,Color.white,
                            "<br><size=6>(" + (learningActions-availableActions.Count).ToString() + " left)");
                    }
                    return true;
                }
            }
        }
        return false;
    }

    //every buddy by default know how to eat at low efficiency but if they are taught that action again
    //they will overwrite their default action with what they were taught (its how they can improve with better teaching)
    void ImproveCoreHarvestSkill(GOAPAct newActions){
        MeleeAttack hitBush = new MeleeAttack(6,1);
        if (newActions.GetType() == hitBush.GetType()){
            if (newActions.ActionLayer == hitBush.ActionLayer && newActions.ActionLayer2 == hitBush.ActionLayer2){
                availableActions[2] = newActions;
            }
        }
    }

    public void SetGoals(){
        StopAllCoroutines();

        TallyGoals();
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
    }

    void OnMouseDown(){ //displays a small GUI if you click on a buddy. kinda useless for now
        manager.ui.DisplayGUI(this,myGoals);
    }
}