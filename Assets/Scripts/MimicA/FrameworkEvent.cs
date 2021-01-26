using System.Collections.Generic;
[System.Serializable]
public abstract class FrameworkEvent
{
    //this class creates a bunch possible events (melee attack, ranged attack, etc) that extend this class
    //public string EventName { get; protected set; }
    public int EventLayer;
    public List<GameState.State> Preconditions = new List<GameState.State>();
    public List<GameState.State> Effects = new List<GameState.State>();
    public float Cost = 10f;
    public float motiveReproduction, motiveHarvest, motiveAttack;//counts toward the 3 possible creature goals
    public float EventRange;
    // public enum Range {None, Melee, Bomb, Harvest, Deliver}
    // public Range MyRequiredRange;

    //Creates clone of event so that each companion/creature can have its own values. Without clone, we'd referencing same obj in memory creating conflicts
    public abstract FrameworkEvent Clone();
    public abstract bool CheckRange(Creature agent); //checking if inRange
    public abstract bool CheckPreconditions(List<GameState.State> currentState); //checking if current state meets conditions to perform this action
    public abstract bool CheckEffects(List<GameState.State> goalState); //checking if action effects will meet requirement of state
    public abstract bool PerformEvent(Creature agent);//performs the event
    protected abstract bool CompleteEvent(Creature agent);//performs the event

}
