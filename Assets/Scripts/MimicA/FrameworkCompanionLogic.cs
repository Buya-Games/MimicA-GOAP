using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameworkCompanionLogic : Creature
{
    //this sits on every companion and
    //1) at outset will listen for possibleEvents
    //2) returns getDecision when need to figure out what to do
    
    FrameworkEvent defaultEvent;//what to run in case no instructions... it will be just idle
    [SerializeField] List<FrameworkEvent> possibleEvents = new List<FrameworkEvent>();//using List instead of HashSet cuz List size is only 10, otherwise better to use HashSet
    Queue<FrameworkEvent> toDo = new Queue<FrameworkEvent>();
    Player player;
    public IEnumerator currentCoroutine;
    
    protected override void Awake(){
        player = GameObject.FindObjectOfType<Player>();
        player.OnTeach += Learn;//tells companion to listen everytime PlayerControl uses OnTeach, and in those cases to run Learn
        //i could implement interfaces on this, but I think this is fine since it's just like a dozen FrameworkEvents to learn
    }

    void Learn(){ 
        if (possibleEvents.Count < 10){//companion will only learn the first 10 FrameworkEvents a player performs
            possibleEvents.Add(player.CurrentEvent);
            ListEmUp();
        }
    }

    void ListEmUp(){
        for (int i = 0;i<possibleEvents.Count;i++){
            Debug.Log(i + " :" + possibleEvents[i].GetType());
        }
    }

    //returns the best action to take depending on current game state, as well as a queue of actions to perform after that
    //ideally this should take the game state when deciding, instead of just randomly choosing from possibleEvents
    
    public void GetDecision(){
    }

    //checking if we need to move in order to fulfill next FrameworkEvent in the queue
    void MovementCheck(){
        FrameworkEvent nextEvent = toDo.Peek();
        if (nextEvent != null && nextEvent.MyRequiredRange != FrameworkGameStateVector.Range.None){//if next Event requires me to be in range 
            //check if I am in distance of said range
            //if not, add move to the queue and perform it as a coroutine

        }

    }

    Queue<FrameworkEvent> FrameWorkEventsToDo(){
        toDo = new Queue<FrameworkEvent>();
        return toDo;
    }

    public FrameworkEvent GetNextEvent(){
        return toDo.Dequeue();
    }
}
