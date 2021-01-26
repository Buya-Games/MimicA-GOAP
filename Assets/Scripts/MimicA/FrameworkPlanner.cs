using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FrameworkPlanner : MonoBehaviour
{

    protected class Node {
		public Node parent;
		public float runningCost;
		public List<GameState.State> state;
		public FrameworkEvent action;

		public Node(Node parent, float runningCost, List<GameState.State> state, FrameworkEvent action) {
			this.parent = parent;
			this.runningCost = runningCost;
			this.state = state;
			this.action = action;
        }
	}
    
    public Queue<FrameworkEvent> MakePlan(
        FrameworkCompanionLogic agent, 
        List<GameState.State> worldState, 
        List<GameState.State> goal)
    {
        //List<Node> leaves = new List<Node>();
        List<FrameworkEvent> leaves = new List<FrameworkEvent>();
        Node goalNode = new Node(null,0,goal,null);
        
        bool success = buildBackwardGraph(goalNode, leaves, agent.availableActions, worldState, agent);
        if (!success){
            Debug.Log("NO PLAN"); //Should cycle to the next goal
            return null;
        }

        // Node cheapest = null;
		// foreach (Node leaf in leaves) {
		// 	if (cheapest == null)
		// 		cheapest = leaf;
		// 	else {
		// 		if (leaf.runningCost < cheapest.runningCost)
		// 			cheapest = leaf;
		// 	}
		// }

        // // get its node and work back through the parents
		// List<FrameworkEvent> result = new List<FrameworkEvent> ();
		// Node n = cheapest;
		// while (n != null) {
		// 	if (n.action != null) {
		// 		result.Insert(0, n.action); // insert the action in the front
		// 	}
		// 	n = n.parent;
		// }

        Queue<FrameworkEvent> plan = new Queue<FrameworkEvent>();
		foreach (FrameworkEvent e in leaves) {
			plan.Enqueue(e);
		}
        return plan;
    }

    bool buildBackwardGraph(Node goal, List<FrameworkEvent> leaves, List<FrameworkEvent> availActions, List<GameState.State> worldState, FrameworkCompanionLogic agent){
        bool foundOne = false;
        Debug.Log("available actions:");
        ListIt(availActions);
        Debug.Log("goal state:");
        ListIt(goal.state);
        List<FrameworkEvent> usableActions = getUsableActions(availActions,goal.state);
        foreach (FrameworkEvent action in usableActions){
            if (GameState.CompareStates(action.Preconditions,worldState)){
                Debug.Log(action + " can be performed right now! so adding it to leaves");
                leaves.Add(action);
                foundOne = true;
            } else {
                Debug.Log(action + " could not be performed but looking at next action we can perform that meets its preconditions");
                float costAfterMotive = calculateCost(agent,action);
                //make new node representing the world state required to run this action
                Node childNode = new Node(goal, goal.runningCost + costAfterMotive, action.Preconditions, action);
                //if new node is possible in current world state, then that is the action we need to do
                bool found = buildBackwardGraph(childNode,leaves,availActions,worldState,agent);
                if (found){
                    foundOne = true;
                }
            }
        }
        return foundOne;
    }

    bool buildGraph(Node parent, List<Node> leaves, List<FrameworkEvent> availActions, List<GameState.State> goal, FrameworkCompanionLogic agent){
        bool foundOne = false;
        List<FrameworkEvent> usableActions = getUsableActions(availActions,parent.state);
        ListIt(usableActions);
        foreach (FrameworkEvent u in usableActions){
            Debug.Log(u);
            if (GameState.CompareStates(u.Preconditions,parent.state)){
                float costAfterMotive = calculateCost(agent,u);
                //need to insert cost of moving to X here somewhere
                Node newNode = new Node(parent, parent.runningCost + costAfterMotive, combineStates(parent.state,u.Effects), u);

                if (newNode.state == goal || GameState.CompareStates(goal,newNode.state)){
                    leaves.Add(newNode);
                    foundOne = true;
                } else {
                    // test all the remaining actions and branch out the tree
					List<FrameworkEvent> subset = actionSubset(getUsableActions(agent.availableActions,newNode.state), u);
					bool found = buildGraph(newNode, leaves, subset, goal, agent);
					if (found)
						foundOne = true;
                }
            }
        }
        return foundOne;
    }

    List<FrameworkEvent> getUsableActions(List<FrameworkEvent> actions, List<GameState.State> state){
        List<FrameworkEvent> usableActions = new List<FrameworkEvent>();
        foreach (FrameworkEvent a in actions){
            if (a.CheckEffects(state)){
                usableActions.Add(a);
            }
        }
        return usableActions;
    }

    public static void ListIt<T>(List<T> rist){
        foreach (T state in rist){
            Debug.Log(state);
        }
    }

    List<GameState.State> combineStates(List<GameState.State> stateA, List<GameState.State> stateB){
        stateA.AddRange(stateB);
        //List<FrameworkGameStateVector.GameState> result = stateA.Union(stateB).ToList();
        return stateA;
    }

    float calculateCost(FrameworkCompanionLogic agent,FrameworkEvent action){
        //need to include movement cost in here somewhere
        float repro = action.motiveReproduction - agent.motiveReproduction;
        float harvest = action.motiveHarvest - agent.motiveHarvest;
        float attack = action.motiveReproduction - agent.motiveReproduction;
        return Mathf.Min(repro,Mathf.Min(harvest,attack));
    }

    List<FrameworkEvent> actionSubset(List<FrameworkEvent> currentList, FrameworkEvent currentAction){
        List<FrameworkEvent> newList = new List<FrameworkEvent>();
        foreach (FrameworkEvent e in currentList){
            if (e != currentAction){
                newList.Add(e);
            }
        }
        return newList;
    }
}
