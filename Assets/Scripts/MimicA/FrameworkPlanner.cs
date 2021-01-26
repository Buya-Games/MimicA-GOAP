using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FrameworkPlanner : MonoBehaviour
{

    protected class Node {
		public Node parent;
		public float costBenefit; //higher is better
		public List<GameState.State> state;
		public FrameworkEvent action;

		public Node(Node parent, float costBenefit, List<GameState.State> state, FrameworkEvent action) {
			this.parent = parent;
			this.costBenefit = costBenefit;
			this.state = state;
			this.action = action;
        }
	}
    
    public Queue<FrameworkEvent> MakePlan(
        FrameworkCompanionLogic agent, 
        List<GameState.State> worldState, 
        List<GameState.State> goals)
    {
        //List<Node> leaves = new List<Node>();
        List<Node> leaves = new List<Node>();
        Node goalNode = new Node(null,0,goals,null);
        bool success = buildBackwardGraph(goalNode, leaves, agent.availableActions, worldState, agent);

        if (!success){ //if no action can get us to the goal, then see what actions are possible and judge based on their cost/benefit
            //Debug.Log("can't get to main goal with current actions, so looking for best secondary actions");
            bool lastResort = false;
            foreach (FrameworkEvent action in agent.availableActions){
                if (GameState.CompareStates(action.Preconditions,worldState)){//if it can be performed in the current world state
                    Node branchNode = new Node(null, action.EstimateActionCost(agent), action.Preconditions, action);
                    leaves.Add(branchNode);
                    lastResort = true;
                }
            }
            if (lastResort == false){
                Debug.Log("Couldn't find ANY plans wtf");
                return null;
            }
        }

        Node cheapest = null;
		foreach (Node leaf in leaves) {
			if (cheapest == null)
				cheapest = leaf;
			else {
				if (leaf.costBenefit < cheapest.costBenefit)
					cheapest = leaf;
			}
		}
        //Debug.Log("cheapest one was " + cheapest.action + " with cost of " + cheapest.costBenefit);

        // get its node and work back through the parents
		List<FrameworkEvent> result = new List<FrameworkEvent> ();
		Node n = cheapest;
		while (n != null) {
			if (n.action != null) {
                //Debug.Log("adding " + n.action + " to the queue");
				result.Insert(result.Count, n.action); // insert the action at the back
			}
			n = n.parent;
		}

        Queue<FrameworkEvent> plan = new Queue<FrameworkEvent>();
		foreach (FrameworkEvent e in result) {
			plan.Enqueue(e);
		}
        return plan;
    }

    //goal->current graph (backwards)
    bool buildBackwardGraph(Node currentNode, List<Node> leaves, List<FrameworkEvent> availActions, List<GameState.State> worldState, FrameworkCompanionLogic agent){
        bool foundOne = false;
        // Debug.Log("available actions:");
        // ListIt(availActions);
        // Debug.Log("goal state:");
        // ListIt(currentNode.state);
        List<FrameworkEvent> usableActions = getUsableActions(availActions,currentNode.state);
        foreach (FrameworkEvent action in usableActions){//just add the cost in here... so that action closest to currentworldstate has best benefit/lowest cost
            if (GameState.CompareStates(action.Preconditions,worldState)){
                currentNode.action = action;
                //currentNode.costBenefit = calculateBenefit(agent)/calculateCost(agent,currentNode.action);
                //Debug.Log(action + " can be performed right now! so adding it to leaves");
                leaves.Add(currentNode);
                foundOne = true;
            } else {
                //Debug.Log(action + " could not be performed but looking at child actions we can perform to meet MY preconditions");
                //make new node representing the world state required to run this action
                Node childNode = new Node(currentNode, 0, action.Preconditions, null);
                //if new node is possible in current world state, then that is the action we need to do
                bool found = buildBackwardGraph(childNode,leaves,listSubset(availActions,action),worldState,agent);
                if (found){
                    //##why would i ever need to know the cost of this chippokochin that needs its child to run anyway?
                    //currentNode.costBenefit = calculateBenefit(agent)/(calculateCost(agent,action) + calculateCost(agent,childNode.action));
                    currentNode.action = action;
                    foundOne = true;
                }
            }
        }
        return foundOne;
    }

    // current->goal graph. Not using this cuz exponentially more calculations
    // bool buildGraph(Node parent, List<Node> leaves, List<FrameworkEvent> availActions, List<GameState.State> goal, FrameworkCompanionLogic agent){
    //     bool foundOne = false;
    //     List<FrameworkEvent> usableActions = getUsableActions(availActions,parent.state);
    //     ListIt(usableActions);
    //     foreach (FrameworkEvent u in usableActions){
    //         Debug.Log(u);
    //         if (GameState.CompareStates(u.Preconditions,parent.state)){
    //             float costAfterMotive = calculateCost(agent,u);
    //             //need to insert cost of moving to X here somewhere
    //             Node newNode = new Node(parent, parent.runningCost + costAfterMotive, combineStates(parent.state,u.Effects), u);

    //             if (newNode.state == goal || GameState.CompareStates(goal,newNode.state)){
    //                 leaves.Add(newNode);
    //                 foundOne = true;
    //             } else {
    //                 // test all the remaining actions and branch out the tree
	// 				List<FrameworkEvent> subset = actionSubset(getUsableActions(agent.availableActions,newNode.state), u);
	// 				bool found = buildGraph(newNode, leaves, subset, goal, agent);
	// 				if (found)
	// 					foundOne = true;
    //             }
    //         }
    //     }
    //     return foundOne;
    // }

    List<FrameworkEvent> getUsableActions(List<FrameworkEvent> actions, List<GameState.State> state){
        List<FrameworkEvent> usableActions = new List<FrameworkEvent>();
        foreach (FrameworkEvent a in actions){
            if (a.CheckEffects(state)){
                usableActions.Add(a);
            }
        }
        return usableActions;
    }

    public static void ListIt<T>(List<T> list){
        foreach (T state in list){
            Debug.Log(state);
        }
    }

    List<GameState.State> combineStates(List<GameState.State> stateA, List<GameState.State> stateB){
        stateA.AddRange(stateB);
        //List<FrameworkGameStateVector.GameState> result = stateA.Union(stateB).ToList();
        return stateA;
    }

    float calculateCost(FrameworkCompanionLogic agent,FrameworkEvent action){
        //float benefit = Mathf.Max(agent.motiveReproduction,Mathf.Max(agent.motiveAttack,agent.motiveHarvest));
        return action.EstimateActionCost(agent);
        //return benefit/cost;
        //need to include movement cost in here somewhere
        // float repro = action.motiveReproduction - agent.motiveReproduction;
        // float harvest = action.motiveHarvest - agent.motiveHarvest;
        // float attack = action.motiveReproduction - agent.motiveReproduction;
        // return Mathf.Min(repro,Mathf.Min(harvest,attack));
    }

    float calculateBenefit(FrameworkCompanionLogic agent){
        return Mathf.Max(agent.motiveReproduction,Mathf.Max(agent.motiveAttack,agent.motiveHarvest));
    }

    // List<FrameworkEvent> actionSubset(List<FrameworkEvent> list, FrameworkEvent item){
    //     List<FrameworkEvent> newList = list;
    //     newList.Remove(item);
    //     // foreach (FrameworkEvent e in currentList){
    //     //     if (e != currentAction){
    //     //         newList.Add(e);
    //     //     }
    //     // }
    //     return newList;
    // }

    // List<GameState.State> goalSubset (List<GameState.State> list, GameState.State item){
    //     List<GameState.State> newList = list;
    //     newList.Remove(item);
    //     return newList;
    // }

    List<T> listSubset<T> (List<T> list, T removeMe) {
        List<T> newList = new List<T>();
        foreach (T t in list){
            if (!t.Equals(removeMe)){
                newList.Add(t);
            }
        }
        return newList;
    }
}
