using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public abstract class FrameworkEvent: FrameworkGameStateVector
{
    //this class creates a bunch possible events (melee attack, ranged attack, etc) that extend this class
    //public string EventName { get; protected set; }
    public int TargetLayer;
    public Dictionary<string, bool> preconditions;
    public Dictionary<string, bool> effects;
    public float cost = 1f;

    public void addPrecondition(string key, bool value){
		preconditions.Add(key, value);
	}
    public void addEffect(string key, bool value){
		effects.Add(key, value);
	}
    
    //Creates a copy of the event so that each companion/creature can have their own and can set their own values (like inRange, etc). 
    //Without this, we'll just be referencing the same object in memory and modifying the same value, creating potential conflicts
    public abstract FrameworkEvent Clone();
    public abstract bool CheckRange(Creature agent); //checking if inRange
    public abstract bool CheckPreconditions(Creature agent); //checking if conditions necessary to do perform event
    public abstract bool PerformEvent(Creature agent);//performs the event
    protected abstract bool CompleteEvent(Creature agent);//performs the event

}
