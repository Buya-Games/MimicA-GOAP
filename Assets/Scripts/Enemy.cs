//assigns enemy goals/actions. basically they just eat stuff and when full they will attack the cow
public class Enemy : CreatureLogic
{
    protected override void Awake(){
        base.Awake();
        Init();
    }

    public override void Init(){
        base.Init();

        health = 50;//setting low health so they find food first before attacking cow
        availableActions.Add(new MeleeAttack(14,1));//attacking cow
        availableActions.Add(new Eat(7));//enemies will eat ANYTHING even poop
        availableActions.Add(new Eat(9));
        availableActions.Add(new Eat(10));
        availableActions.Add(new Eat(16));
        availableActions.Add(new PickupItem(7,false));
        availableActions.Add(new PickupItem(9,false));
        availableActions.Add(new PickupItem(10,false));
        availableActions.Add(new PickupItem(16,false));

        MyStats.Speed/=2;//didnt want them moving too fast

        SetGoals();
        GetPlan();
    }

    void SetGoals(){
        myGoals.Enqueue(GameState.State.goalAttackCow);
    }
   
}
