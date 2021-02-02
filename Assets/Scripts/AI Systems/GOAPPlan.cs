using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPPlan : MonoBehaviour
{

    [SerializeField] bool debug;

    protected class Node {
		public Node parent;
		public float costBenefit; //lower is better
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
        CreatureLogic agent, 
        List<GameState.State> worldState, 
        List<GameState.State> goals)
    {
        List<Node> leaves = new List<Node>();
        Node goalNode = new Node(null,0,goals,null);
        bool success = buildBackwardGraph(goalNode, leaves, agent.availableActions, worldState, agent);

        if (!success){ //if no action can get us to the goal, then see what actions are possible and judge based on their cost/benefit
            if (debug){Debug.Log("can't get to main goal (" + goals[0] + ") with current actions, so looking for best secondary actions");}
            bool lastResort = false;
            foreach (FrameworkEvent action in agent.availableActions){
                if (GameState.CompareStates(action.Preconditions,worldState)){//if it can be performed in the current world state
                    Node branchNode = new Node(null, action.EstimateActionCost(agent), action.Preconditions, action);
                    leaves.Add(branchNode);
                    lastResort = true;
                }
            }
            if (!lastResort){
                return null;
            }
        }

        Node cheapest = null;
        if (debug) {Debug.Log("length length " + leaves.Count);}
        for (int i = 0;i<leaves.Count;i++){
            if (cheapest == null){
                cheapest = leaves[i];
                if(debug){Debug.Log("cheapest set to " + cheapest.action);}
            }
            if(debug){Debug.Log("next up " + leaves[i].action + "( " + leaves[i].costBenefit + ") vs cheapest " + cheapest.action + " (" + cheapest.costBenefit + ")");}
            if (leaves[i].costBenefit < cheapest.costBenefit){
                cheapest = leaves[i];
            }
        }
		// foreach (Node leaf in leaves) {
		// 	if (cheapest == null)
		// 		cheapest = leaf;
        //         if(debug){Debug.Log("cheapest set to " + cheapest.action);}
		// 	else {
		// 		if (leaf.costBenefit < cheapest.costBenefit)
        //             if(debug){Debug.Log("new cheapest is " + leaf.action + "( " + leaf.costBenefit + "), old was " + cheapest.action + " (" + cheapest.costBenefit + ")");}
		// 			cheapest = leaf;
		// 	}
		// }

        // get its node and work back through the parents
		List<FrameworkEvent> result = new List<FrameworkEvent> ();
		Node n = cheapest;
		while (n != null) {
			if (n.action != null) {
                if (debug){Debug.Log("adding " + n.action + " to the queue");}
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
    bool buildBackwardGraph(Node currentNode, List<Node> leaves, List<FrameworkEvent> availActions, List<GameState.State> worldState, Creature agent){
        bool foundOne = false;
        if (debug){
            Debug.Log("------------------available actions------------------");
            Tools.PrintList(availActions);
            Debug.Log("------------------goal state------------------");
            Tools.PrintList(currentNode.state);
            Debug.Log("------------------world state------------------");
            Tools.PrintList(worldState);
        }
        List<FrameworkEvent> effectiveActions = getEffectiveActions(availActions,currentNode.state);//actions with Effects that meets goal
        foreach (FrameworkEvent action in effectiveActions){
            if (GameState.CompareStates(action.Preconditions,worldState)){
                Node goodNode = new Node(currentNode, action.EstimateActionCost(agent), null, action);//creating new node cuz currentNode gets recycled
                if (debug){Debug.Log(action + "(" + goodNode.action + ") can be performed right now! so adding it to leaves. cost: " + goodNode.costBenefit);}
                leaves.Add(goodNode);
                foundOne = true;
            } else {
                if (debug){Debug.Log(action + " could not be performed but looking at other actions which can meet my preconditions");}
                //make new node representing the world state required to run this action
                Node childNode = new Node(currentNode, 0, action.Preconditions, null);
                //if new node is possible in current world state, then that is the action we need to do
                bool found = buildBackwardGraph(childNode,leaves,Tools.ListSubset(availActions,action),worldState,agent);
                if (found){
                    //this action's cost is itself + child.. could be double counting (move to melee, move to pickup same thing)
                    currentNode.costBenefit = action.EstimateActionCost(agent) + childNode.costBenefit;
                    currentNode.action = action;
                    foundOne = true;
                }
            }
            if (debug){
                Debug.Log("------------------list of current leaves------------------");
                for (int i = 0;i<leaves.Count;i++){
                    Debug.Log(leaves[i].action + ": " + leaves[i].costBenefit);
                }
            }
        }
        return foundOne;
    }

    List<FrameworkEvent> getEffectiveActions(List<FrameworkEvent> actions, List<GameState.State> state){
        List<FrameworkEvent> usableActions = new List<FrameworkEvent>();
        foreach (FrameworkEvent a in actions){
            if (a.CheckEffects(state)){
                usableActions.Add(a);
            }
        }
        return usableActions;
    }

    List<GameState.State> combineStates(List<GameState.State> stateA, List<GameState.State> stateB){
        stateA.AddRange(stateB);
        return stateA;
    }

    float calculateCost(Buddy agent,FrameworkEvent action){
        //float benefit = Mathf.Max(agent.motiveReproduction,Mathf.Max(agent.motiveAttack,agent.motiveHarvest));
        return action.EstimateActionCost(agent);
        //return benefit/cost;
        //need to include movement cost in here somewhere
        // float repro = action.motiveReproduction - agent.motiveReproduction;
        // float harvest = action.motiveHarvest - agent.motiveHarvest;
        // float attack = action.motiveReproduction - agent.motiveReproduction;
        // return Mathf.Min(repro,Mathf.Min(harvest,attack));
    }

    float calculateBenefit(Buddy agent){
        return Mathf.Max(agent.motiveReproduction,Mathf.Max(agent.motiveAttack,agent.motiveHarvest));
    }
    
}
