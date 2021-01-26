using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkGameStateVector
{
    //Holds all possible world game state variables
    //Extended by FrameworkEvent to track required game state for each Event
    //CURRENTLY NOT REALLY USED CUZ WE DON'T CHECK WORLD GAME STATE WHEN DECIDING WHICH FrameworkEvents TO PERFORM
    public enum State {
        itemNone, 
        itemBerry, 
        itemFungus, 
        itemBomb, 
        availBerry, 
        availFungus, 
        availBomb, 
        availBush, 
        availMushroom, 
        availEnemy, 
        availCow, 
        goalAttacked,
        goalReproduced,
        goalHarvested
    }
    public enum Range {None, Melee, Bomb, Harvest, Deliver}
    public Range MyRequiredRange;
    public bool isLowHealth; //if less than 50 health
    public bool isLowAmmo, isLowFood; //when carriage is low on items
}
