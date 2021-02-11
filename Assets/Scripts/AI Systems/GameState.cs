using System.Collections.Generic;

//Holds all possible game world states
[System.Serializable]
public class GameState
{
    public enum State {
        playerAlive,
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
        goalAttackEnemies,
        goalGatherShrooms,
        goalGatherFood,
        goalAttackCow,
        goalFollowPlayer,
        goalHelpOthers,
        goalEat
    }

    public static bool CompareStates(List<State> compareThis, List<State> againstThis, string actionName = ""){
        foreach (var state in compareThis){
            if (!againstThis.Contains(state)){
                UnityEngine.Debug.Log(string.Format("{0}{1} wasn't found",actionName,state));
                return false;
            }
            UnityEngine.Debug.Log(string.Format("{0}{1} was found",actionName,state));
        }
        return true;
    }

    public static List<GameState.State> CombineStates(List<GameState.State> stateA, List<GameState.State> stateB){
        stateA.AddRange(stateB);//doesn't check for dupes
        return stateA;
    }
}
