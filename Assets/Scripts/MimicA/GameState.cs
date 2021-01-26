using System.Collections.Generic;
using UnityEngine;
//Holds all possible game world states
public class GameState
{
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

    public static bool CompareStates(List<State> compareThis, List<State> againstThis){
        foreach (var state in compareThis){
            if (!againstThis.Contains(state)){
                return false;
            }
        }
        return true;
    }
}
