using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkGameData: MonoBehaviour
{
    //keeps track of game states and player actions
    public Dictionary<FrameworkEvent, FrameworkGameStateVector> PerformedGameStates = new Dictionary<FrameworkEvent, FrameworkGameStateVector>();
    Player player;
    GameManager manager;
    public FrameworkGameStateVector CurrentState;


    void Awake(){
        player = FindObjectOfType<Player>();
        player.OnTeach += AddGameStateVector;//listen everytime PlayerControl performs an event and adds that event + current world game state to dictionary
        manager = FindObjectOfType<GameManager>();
    }
    
    public void AddGameStateVector(){
        PerformedGameStates.Add(player.CurrentEvent, CurrentState);
    }
}
