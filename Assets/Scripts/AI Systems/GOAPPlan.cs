using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPPlan : MonoBehaviour
{
    GameManager manager;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
    }

    protected class Node {
		public Node parent;
		public float costBenefit; //lower is better
		public List<GameState.State> goalState;
		public GOAPAct action;

		public Node(Node parent, float costBenefit, List<GameState.State> goalState, GOAPAct action) {
			this.parent = parent;
			this.costBenefit = costBenefit;
			this.goalState = goalState;
			this.action = action;
        }
	}
    
    public Queue<GOAPAct> MakePlan(
        CreatureLogic agent, 
        List<GameState.State> worldState, 
        List<GameState.State> goals)
    {
        //do all action cost estimates at outset so don't repeat them during path building
        //ideally move this to another function that runs every 1-2 seconds to reduce call freq cuz world dont change that quickly
        foreach (GOAPAct a in agent.availableActions){
            a.EstimateActionCost(agent);
            if (manager.debug){Debug.Log(string.Format("{0}{1}-{2} cost is {3}",a,a.ActionLayer,a.ActionLayer2,a.Cost));}
        }
        
        List<Node> leaves = new List<Node>();
        Node fakeParentNode = new Node(null,1000,goals,new Fake());//a fake node so we don't trigger alerts in debug.log. it's stupid just delete it after debugging
        Node startNode = new Node(fakeParentNode,0,goals,null);
        bool success = buildPath(startNode, leaves, agent.availableActions, worldState, agent);//try to build path directly to goal

        if (!success){ //if no action can get us to the goal, see what actions are possible and judge based on motive alignment, etc
            List<GOAPAct> possibleActions = getPossibleActions(agent.availableActions,worldState);
            if (manager.debug){Debug.Log(string.Format("[{0}] no direct path to goal ({1}) but looking at possible secondary actions",agent.name,goals[0]));}
            if (manager.debug) {Tools.PrintList(agent.name, "POSSIBLE SECONDARY ACTIONS", possibleActions);}
            bool secondaryActions = false;
            foreach (GOAPAct action in possibleActions){
                float alignmentBonus = 0;
                if (agent is Buddy){//only buddies have motives
                    alignmentBonus = checkGoalAlignment((Buddy)agent,action);
                }
                if (manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} alignment bonus: {4}",
                            agent.name,action,action.ActionLayer,action.ActionLayer2,alignmentBonus));}
                if (alignmentBonus > 0){//if it's a task that at least somewhat aligns with agent's motivation
                
                    Node secondaryActionNode = new Node(fakeParentNode, action.Cost, action.Preconditions, action);
                    secondaryActionNode.costBenefit -= (alignmentBonus * 10);//bonus from action that aligns with motive

                    //additional benefit if more actions become possible after this
                    List<GOAPAct> futureActions = getPossibleActions(agent.availableActions,GameState.CombineStates(worldState,action.Effects));
                    if (futureActions.Count > 0){
                        if (manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} leads to {4} more actions",
                            agent.name,action,action.ActionLayer,action.ActionLayer2,futureActions.Count));}
                        secondaryActionNode.costBenefit -= (futureActions.Count * 5);
                    }
                    leaves.Add(secondaryActionNode);
                    secondaryActions = true;
                }
            }
            if (!secondaryActions){//in absolute worst case, just look at anything you can do and what actions it will lead to 
                bool lastResort = false;
                foreach (GOAPAct action in possibleActions){
                    Node lastResortNode = new Node(fakeParentNode, action.Cost, action.Preconditions, action);

                    //additional benefit if more actions become possible after this
                    List<GOAPAct> futureActions = getPossibleActions(agent.availableActions,GameState.CombineStates(worldState,action.Effects));
                    if (futureActions.Count > 0){
                        if (manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} leads to {4} more actions",
                            agent.name,action,action.ActionLayer,action.ActionLayer2,futureActions.Count));}
                        lastResortNode.costBenefit -= (futureActions.Count * 5);
                    }
                    leaves.Add(lastResortNode);
                    lastResort = true;
                }
                if (!lastResort){
                    return null;
                }
            }
        }

        //if we were successful in finding a direct path, adding a big premium to theoretical events so we prefer actual executable ones
        if (success){
            for (int i = 0;i<leaves.Count;i++){
                if (leaves[i].costBenefit == 26){
                    leaves[i].costBenefit+=100;
                }
            }
        }

        tallyParentCosts(leaves,agent);

        // get its node and work back through the parents
		List<GOAPAct> result = new List<GOAPAct> ();
		Node n = FindCheapestNode(leaves,agent);
		while (n != null && n != fakeParentNode) {
			if (n.action != null) {
				result.Insert(result.Count, n.action); // insert the action at the back
			}
			n = n.parent;
		}

        Queue<GOAPAct> plan = new Queue<GOAPAct>();
		foreach (GOAPAct e in result) {
            if (manager.debug){Debug.Log(string.Format("[{0}] adding {1}{2}-{3} to the queue", agent.name, e,e.ActionLayer,e.ActionLayer2));}
			plan.Enqueue(e);
		}
        return plan;
    }

    bool buildPath(Node node, List<Node> leaves, List<GOAPAct> availActions, List<GameState.State> worldState, CreatureLogic agent){
        bool foundPath = false;
        List<GOAPAct> goalActions = getGoalActions(availActions,node.goalState);//actions with Effects that satisfy goalState
        if (manager.debug){
            Debug.Log(string.Format("[{0}] BUILD PATH",agent.name));
            Tools.PrintList(agent.name, "GOAL", node.goalState);
            //Tools.PrintList(agent.name, "AVAIL ACTIONS", availActions);
            //Tools.PrintList(agent.name, "WORLD STATE", worldState);
            Tools.PrintList(agent.name, "ACTIONS SATISFYING GOAL STATE", goalActions);
        }
        foreach (GOAPAct action in goalActions){
            //create new node for each action (necessary if goalActions has more than 1 actions, otherwise they'll overwrite each other)
            Node thisNode = new Node(node.parent,action.Cost,node.goalState,action);
            
            //just for avoiding debugging crap
            if (manager.debug){
                node.action = action;
                node.costBenefit = action.Cost;
            }
            
            //otherwise, this action has to be cheaper than parent (##WE MAKE A BIG ASSUMPTION THAT CHEAPER KIDS LEAD TO CHEAPER PARENTS)
            if (leaves.Count == 0 || thisNode.costBenefit < node.parent.costBenefit){
                if (GameState.CompareStates(action.Preconditions,worldState)){//if can be performed in current state
                    if (manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) can be performed now, so addding it to leaves",
                        agent.name, action, action.ActionLayer,action.ActionLayer2,thisNode.costBenefit));}
                    leaves.Add(thisNode);
                    foundPath = true;
                } //else {

                    //create new node to check if other actions can allow current action at cheaper cost
                    //honestly we don't need this transitionNode cuz it's never actually used, but I'm too tired to fix my spaghetti
                    Node transitionNode = new Node(thisNode, 0, action.Preconditions, null);

                    //if cheaper actions do exist, then create a new path with a new cost
                    if (buildPath(transitionNode,leaves,Tools.ListSubset(availActions,action),worldState,agent)){
                        //Node newPathNode = new Node(node.parent,childNode.costBenefit,node.goalState,action);
                        if (manager.debug){Debug.Log(string.Format("[{0}] new path found {1}{2}-{3} ({4}) via child {5}{6}-{7} ({8})",
                            agent.name,thisNode.action,thisNode.action.ActionLayer,thisNode.action.ActionLayer2,thisNode.costBenefit,
                            transitionNode.action,transitionNode.action.ActionLayer,transitionNode.action.ActionLayer2,transitionNode.costBenefit));}
                        foundPath = true;
                    }
                //}
            } else {
                if (manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) wasn't cheaper than parent {5}{6}-{7} ({8})",
                    agent.name, action, action.ActionLayer,action.ActionLayer2,thisNode.costBenefit,
                    thisNode.parent.action,thisNode.parent.action.ActionLayer,thisNode.parent.action.ActionLayer2,thisNode.parent.costBenefit));}
            }
        }
        // if (debug){
        //     Debug.Log(string.Format("------------------ {0} leaves ------------------",leaves.Count));
        //     for (int i = 0;i<leaves.Count;i++){
        //         Debug.Log(string.Format("[{0}] {1}{2}-{3} : {4}",agent.name,leaves[i].action,
        //             leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));
        //     }
        // }
        return foundPath;
    }

    void tallyParentCosts(List<Node> leaves, CreatureLogic agent){
        for (int i = 0;i<leaves.Count;i++){
            // if (debug) {Debug.Log(string.Format("[{0}] {1}{2}-{3} starting cost: {4}",
            //     agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));}
            
            //we estimate that total path cost will be a multiple of the initial (child) action cost
            //ie: if initial action cost = 20 * 5 steps in path = total path cost estimate is 100
            //i found this to be best way to estimate w/o recalculating costs of real + theoretical objects
            int pathSteps = 1;
            Node p = leaves[i].parent;
            while (p.parent != null){
                pathSteps++;
                p = p.parent;
            }
            // if (debug) {Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) total paths: {5}",
            //     agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit,pathSteps));}
            leaves[i].costBenefit*=pathSteps;
            // if (debug) {Debug.Log(string.Format("[{0}] {1}{2}-{3} end cost: {4}",
            //     agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));}
        }
        // for (int i = 0;i<leaves.Count;i++){
        //     Debug.Log(string.Format("[{0}] {1}{2}-{3} starting cost: {4}",
        //         agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));
        //     Node p = leaves[i].parent;
        //     while (p.parent != null){
        //         Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) adding cost of parent {5}{6}-{7} ({8})",
        //         agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit,
        //         p.action,p.action.ActionLayer,p.action.ActionLayer2,p.costBenefit));
        //         leaves[i].costBenefit+=p.costBenefit;
        //         p = p.parent;
        //     }
        //     Debug.Log(string.Format("[{0}] {1}{2}-{3} end cost: {4}",
        //         agent.name,leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));
        // }
    }

    Node FindCheapestNode(List<Node> leaves, CreatureLogic agent){
        Node cheapest = null;
        if (manager.debug) {Debug.Log(string.Format("------------------ checking cheapest of {0} leaves ({1}) ------------------",leaves.Count,agent.name));}
        for (int i = 0;i<leaves.Count;i++){
            if (leaves[i].costBenefit == 26){
                leaves[i].costBenefit+=100;//adding a big premium to theoretical events
            }
            if (cheapest == null){
                cheapest = leaves[i];
                if(manager.debug){Debug.Log(string.Format("[{0}] cheapest set to: {1}{2}-{3} ({4})", 
                    agent.name, cheapest.action, cheapest.action.ActionLayer, cheapest.action.ActionLayer2,cheapest.costBenefit));}
            }
            if(manager.debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) vs {5}{6}-{7} ({8})",
                agent.name, cheapest.action, cheapest.action.ActionLayer, cheapest.action.ActionLayer2,cheapest.costBenefit,
                leaves[i].action,leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));}
            if (leaves[i].costBenefit < cheapest.costBenefit){
                cheapest = leaves[i];
            }
        }
        return cheapest;
    }

    //action which directly lead to state
    List<GOAPAct> getGoalActions(List<GOAPAct> actions, List<GameState.State> state){
        List<GOAPAct> usableActions = new List<GOAPAct>();
        foreach (GOAPAct a in actions){
            if (a.CheckEffects(state)){
                usableActions.Add(a);
            }
        }
        return usableActions;
    }

    //check which actions are made possible in state
    List<GOAPAct> getPossibleActions(List<GOAPAct> actions, List<GameState.State> state){
        List<GOAPAct> possibleActions = new List<GOAPAct>();
        foreach (GOAPAct a in actions){
            if (a.CheckPreconditions(state)){
                possibleActions.Add(a);
            }
        }
        return possibleActions;
    }

    //checks if agent's motives match what action aims to achieve
    float checkGoalAlignment(Buddy agent, GOAPAct action){
        float alignmentScore = 0;
        if (agent.motiveReproduction > 0 && action.motiveReproduction > 0){
            alignmentScore+=agent.motiveReproduction;
        }
        if (agent.motiveHarvest > 0 && action.motiveHarvest > 0){
            alignmentScore+=agent.motiveHarvest;
        }
        if (agent.motiveAttack > 0 && action.motiveAttack > 0){
            alignmentScore+=agent.motiveAttack;
        }
        if (agent.motiveHelper > 0 && action.motiveHelper > 0){
            alignmentScore+=agent.motiveHelper;
        }
        return alignmentScore;
    }

    //starts at current world state and checks if any actions can lead to goal
    // bool forwardGraph(Node node, List<Node> leaves, List<GOAPAct> availActions, List<GameState.State> worldState, CreatureLogic agent, float cheapest){
    //     bool foundIndirectGoalPath = false;
    //     List<GOAPAct> possibleActions = getPossibleActions(availActions,worldState);//actions possible in current state
    //     if (debug){
    //         Debug.Log(string.Format("[{0}] FORWARD GRAPH",agent.name));
    //         Tools.PrintList(agent.name, "AVAIL ACTIONS", availActions);
    //         Tools.PrintList(agent.name, "GOAL", node.goalState);
    //         Tools.PrintList(agent.name, "WORLD STATE", worldState);
    //         Tools.PrintList(agent.name, "CURRENTLY POSSIBLE ACTIONS", possibleActions);
    //     }
    //     foreach (var action in possibleActions){//review the leftovers not used by backwardGraph
    //         node.costBenefit = action.EstimateActionCost(agent);
    //         if (node.parent != null){
    //             node.costBenefit+= node.parent.costBenefit;
    //         }
    //         if (node.costBenefit < cheapest){//if cheaper than cheapest from backwardGraph
    //             if (GameState.CompareStates(action.Preconditions,worldState)){//and if it can be performed in world state
    //                 if (GameState.CompareStates(node.goalState,action.Effects)){//and if it will satisfy the creature's ultimate goal
    //                     if (debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) can be performed now, so adding it to leaves (parent {5}{6}-{7})",
    //                         agent.name, action, action.ActionLayer, action.ActionLayer2, node.costBenefit,
    //                         node.parent.action,node.parent.action.ActionLayer,node.parent.action.ActionLayer2));}
    //                     node.action = action;
    //                     leaves.Add(node);
    //                     foundIndirectGoalPath = true;
    //                 } else {//if it does not satisfy ultimate goal but can be performed, check if it will lead to something that can satisfy ultimate goal
    //                     if (debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} doesn't satify goal but looking if it leads to other actions (parent {4}{5}-{6})",
    //                         agent.name,action,action.ActionLayer,action.ActionLayer2,
    //                         node.parent.action,node.parent.action.ActionLayer,node.parent.action.ActionLayer2));}
    //                     // Node childNode = new Node(//then add it to leaves
    //                     //     node,//parent
    //                     //     0,//this action cost + parent cost
    //                     //     node.goalState,//ultimate goal
    //                     //     null);
    //                     // if (forwardGraph(childNode,leaves,Tools.ListSubset(availActions,action),GameState.CombineStates(worldState,action.Effects),agent,cheapest)){
    //                     //     foundIndirectGoalPath = true;
    //                     //     node.action = action;
    //                     // }

    //                     //new node to check if another action can lead to this action at a cheaper cost
    //                     Node childNode = new Node(node, 0, node.goalState, new Fake());

    //                     //if cheaper actions do exist, then create a new path with a new cost
    //                     if (forwardGraph(childNode,leaves,Tools.ListSubset(availActions,action),GameState.CombineStates(worldState,action.Effects),agent,cheapest)){
    //                         // Node newPathNode = new Node(node.parent,node.costBenefit,node.goalState,action);
    //                         // if (debug){Debug.Log(string.Format("[{0}] new path found {1} ({2}) via child {3} ({4})",
    //                         //     agent.name,action,newPathNode.costBenefit,childNode.action,childNode.costBenefit));}
    //                         // leaves.Add(newPathNode);
    //                         node.action = action;
    //                         foundIndirectGoalPath = true;
    //                     }
    //                 }
    //             } else {
    //                 if (debug){Debug.Log(string.Format("[{0}]'s {1}{2}-{3} couldn't be performed now (parent {4}{5}-{6}",
    //                 agent.name,action,action.ActionLayer,action.ActionLayer2,node.parent.action,node.parent.action.ActionLayer,node.parent.action.ActionLayer2));}
    //             }
    //         } else {
    //             if (debug){Debug.Log(string.Format("[{0}]'s {1}{2}-{3} cost ({4}) wasn't cheapest (parent {5}{6}-{7})",
    //                 agent.name,action,action.ActionLayer,action.ActionLayer2,node.costBenefit,
    //                 node.parent.action,node.parent.action.ActionLayer,node.parent.action.ActionLayer2));}
    //         }
    //     }
    //     if (debug){
    //         Debug.Log(string.Format("------------------ {0} leaves after {1} forwardGraph------------------",leaves.Count,agent.name));
    //         for (int i = 0;i<leaves.Count;i++){
    //             Debug.Log(string.Format("[{0}] {1}{2}-{3} : {4}",agent.name,leaves[i].action,
    //                 leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));
    //         }
    //     }
    //     return foundIndirectGoalPath;

    // }
    //starts at goal and checks if any actions can directly lead to that goal. more performant than forwardGraph
    // bool backwardGraph(Node node, List<Node> leaves, List<GOAPAct> availActions, List<GameState.State> worldState, Creature agent){
    //     bool foundDirectGoalPath = false;
    //     List<GOAPAct> goalActions = getGoalActions(availActions,node.goalState);//actions with Effects that will create goalState
    //     if (debug){
    //         Debug.Log(string.Format("[{0}] BACKWARD GRAPH",agent.name));
    //         Tools.PrintList(agent.name, "AVAIL ACTIONS", availActions);
    //         Tools.PrintList(agent.name, "GOAL", node.goalState);
    //         Tools.PrintList(agent.name, "ACTIONS SATISFYING GOAL STATE", goalActions);
    //     }
    //     foreach (GOAPAct action in goalActions){
    //         node.costBenefit = action.EstimateActionCost(agent);
    //         if (node.parent != null){
    //             node.costBenefit+=node.parent.costBenefit;
    //         }
    //         if (GameState.CompareStates(action.Preconditions,worldState)){
    //             node.action = action;
    //             if (debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) can be performed now, so addding it to leaves",
    //                 agent.name, action, action.ActionLayer,action.ActionLayer2,node.costBenefit));}
    //             leaves.Add(node);
    //             //Node goodNode = new Node(currentNode, action.EstimateActionCost(agent), null, action);//create new node cuz currentNode will be recycled
    //             // if (debug){Debug.Log(string.Format("[{0}] {1} ({2}) can be performed now, so addding it to leaves",
    //             //     agent.name, action, goodNode.costBenefit));}
    //             // leaves.Add(goodNode);
    //             foundDirectGoalPath = true;
    //         } else {
    //             if (debug){Debug.Log(string.Format("[{0}] {1}{2}-{3} ({4}) can't be performed now but looking if other actions can meet its preconditions",
    //                 agent.name, action, action.ActionLayer,action.ActionLayer2,node.costBenefit));}
                
    //             //make new node representing the world state required to run this action
    //             Node childNode = new Node(node, 0, action.Preconditions, null);
    //             //if new node is possible in current world state, then that is the action we need to do
    //             if (backwardGraph(childNode,leaves,Tools.ListSubset(availActions,action),worldState,agent)){
    //                 node.action = action;
    //                 foundDirectGoalPath = true;
    //             };
    //         }
    //     }
    //     if (debug){
    //         Debug.Log(string.Format("------------------ checking cheapest of {0} ({1}) ------------------",leaves.Count,agent.name));
    //         for (int i = 0;i<leaves.Count;i++){
    //             Debug.Log(string.Format("[{0}] {1}{2}-{3} : {4}",agent.name,leaves[i].action,
    //                 leaves[i].action.ActionLayer,leaves[i].action.ActionLayer2,leaves[i].costBenefit));
    //         }
    //     }
    //     return foundDirectGoalPath;
    // }

    

    

    // float calculateCost(Buddy agent,GOAPAct action){
    //     //float benefit = Mathf.Max(agent.motiveReproduction,Mathf.Max(agent.motiveAttack,agent.motiveHarvest));
    //     return action.EstimateActionCost(agent);
    //     //return benefit/cost;
    //     //need to include movement cost in here somewhere
    //     // float repro = action.motiveReproduction - agent.motiveReproduction;
    //     // float harvest = action.motiveHarvest - agent.motiveHarvest;
    //     // float attack = action.motiveReproduction - agent.motiveReproduction;
    //     // return Mathf.Min(repro,Mathf.Min(harvest,attack));
    // }
    
}
