using System.Collections.Generic;

//Holds all possible game world states
[System.Serializable]
public class GameState
{
    public enum State {
        itemNone, 
        itemBerry, 
        itemFungus, 
        itemBerryPoop, 
        itemFungusPoop, 
        availBerry, 
        availFungus, 
        availBerryPoop, 
        availFungusPoop, 
        availBush,
        availMushroom, 
        availEnemy,
        availBuddy,
        availCow, 
        goalAttacked,
        goalReproduced,
        goalHarvested,
        goalCowAttacked,
        goalFollowPlayer,
        goalEat
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
