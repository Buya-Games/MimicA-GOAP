using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkGameStateVector
{
    //Holds all possible world game state variables
    //Extended by FrameworkEvent to track required game state for each Event
    //CURRENTLY NOT REALLY USED CUZ WE DON'T CHECK WORLD GAME STATE WHEN DECIDING WHICH FrameworkEvents TO PERFORM
    public enum Items {None, Bomb, Food}
    public Items MyItem;
    public enum Range {None, Melee, Bomb, Harvest, Deliver}
    public Range MyRequiredRange;
    public static float RequiredRange;
    public bool isLowHealth; //if less than 50 health
    public bool isLowAmmo, isLowFood; //when carriage is low on items
}
