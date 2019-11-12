using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

//might have to set to public



/*TODO:
 * Figure out how to make agend arrive if he does the same trade twice !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 * place destination points appropriatly
 * 
 */ 




public class PlayerController : MonoBehaviour
{
    public State worldState;
    private Action nextAction;

    private Queue<Action> currentPlan;
    private NavMeshAgent agent;
    private ThiefController thief;

    public Text UIPlayer;
    public Text UICaravan;
    public Text UIAction;
        

    //states for FSM
    bool executingAction = false;
    bool beingRobbed = false;
    float actionTimer = 0;

    private int counter = 0;
    // Start is called before the first frame update
    void Awake()
    {
        InitWorldState(); //initialize the world state, set all spice amounts to 0
    }

    void Update()
    {
        ExecuteNextAction();
    }


    private void ExecuteNextAction()
    {
        //Have a end condition
        if (!executingAction)
        {
            if (!DesiredWordState(worldState))
            {
                FindNextActionAndExecute();
            }
            else
            {
                Time.timeScale = 0;
                SimulationController.gameOver = true; //no further actions allowed
            }
        }
        //action is being executed
        else
        {
            //arrived to the point of interest
            //TODO: here will go thiefs interuption of world state
            if (!beingRobbed)
            {
                if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance < 0.5f)
                {
                    //action is not complete, stand there
                    if (actionTimer < 0.5)
                    {
                        actionTimer += Time.deltaTime;
                    }
                    //action is complete, next action
                    else
                    {
                        //reset the states for the next action
                        actionTimer = 0;
                        executingAction = false;
                        //arrived = false;

                        //update the state
                        worldState = nextAction.postConditions;
                        UpdateUI();
                    }
                }
            }
            else
            {
                agent.isStopped = true;
                agent.ResetPath();

                actionTimer = 0;
                executingAction = false;
                beingRobbed = false;

                currentPlan.Clear();
            }
        }
    }

    private void FindNextActionAndExecute()
    {

        if (currentPlan.Count == 0)
        {
            Action next = GetNextAction(worldState);
            


            if (next.isMethod)
            {


                //we dont have a state after taking out
                //we dont have a state after trading
                //we have a state before executing method
                //we have a state after executing method
                //need a state at all points because of the thief
                //IDDEA: stash take out dont retake out
                
                StashAll stash = new StashAll(worldState);
                stash.IsValid();
                TransferOUT transfer = new TransferOUT(stash.postConditions, next); //condition gets copied
                next.postConditions = transfer.postConditions.DeepCopy(); 
                next.IsValid();


                currentPlan.Enqueue(stash);
                currentPlan.Enqueue(transfer);
                currentPlan.Enqueue(next);
            }
            else
            {
                currentPlan.Enqueue(next);
            }
        }

        nextAction = currentPlan.Dequeue();
        nextAction.Execute(this);


        DisplayActionUI();
        ///////////////////////////////////////
        //counter++;
        //Debug.Log(counter);
        //Debug.Log("Executing : " + nextAction.toString());
        ///////////////////////////////////////

        executingAction = true;
    }

    private void DisplayActionUI()
    {
        UIAction.text = nextAction.toString();
    }

    private void InitWorldState()
    {
        worldState = new State();
        currentPlan = new Queue<Action>();
        agent = GetComponent<NavMeshAgent>();
        thief = GameObject.FindGameObjectWithTag("Thief").GetComponent<ThiefController>();

        foreach (Inventory inventory in (Inventory[])Enum.GetValues(typeof(Inventory)))
        {
            Dictionary<Spice, int> spices = new Dictionary<Spice, int>();
            worldState[inventory] = spices;

            foreach (Spice spice in (Spice[])Enum.GetValues(typeof(Spice)))
            {
                worldState[inventory][spice] = 0;
            }
        }

        UpdateUI();
    }



    private void MakePlan(State currentState)
    {


        //need to have list of possible actions using execute();
        //for this we need a list of actions
        //say we find action that sutisfies pre cond
        //we enqueue it, make current state postcondition 
        //
        //all possible actions that meet preconditions of current worldstate
        //TODO: pass copy of state
        //Get sorted actions by cost

        //cheapest action

        //TODO: way to record predecessors
        //set new cost

        //Problem: after we have stored everything, all the actions will be equivalent picking form backpack will give us less 
        //maybe if he gets it in his backpack, it's have the work of getting it in the caravan
        //depositing above 2 might be unnecessairy, but what if we need to free up backpack?
        //maybe not allow the first trade if we have more that 4 in the caravan ?


        //1. taking out should be a mean, not an action
        //2. backpack and caravan should have shared available inventory for trades
        //

        //Question: since at every step there is an infinite amount of actions possible, we actually don't comeback to explore other possible actions at a step

        //make him stash everything everytime
        //taking out is only a mean to the thing
        //getting a spice 

        //score of higher spices goes up
        // multiplier of back pack stash vs caravan stash



        /*heuristic:
         * never need to bank i.e. having more than 2 of any spice other than turmic since we are doing trades directly from caravan, can solve without banking and without using trade 7
         * 
         * 
         * 
        */


        int count = 0;

        while (!DesiredWordState(currentState))
        //for (int i = 0; i < 60; i++)
        {
            Debug.Log("------------------------------------------------------------------------");
 
            Action nextAction = GetNextAction(currentState);

            currentState = nextAction.postConditions;
            Debug.Log("Picked : " + nextAction.toString());

            count++;
        }

        Debug.Log(count);
    }

    private bool DesiredWordState(State currentState)
    {
        bool reached = true;

        foreach(var spice in currentState[Inventory.Caravan])
        {
            reached = reached && (spice.Value >= 2);
        }

        return reached;
    }

    private Action GetNextAction(State currentState)
    {

        List<Action> actions = new List<Action>();

        Action[] trades = new Action[8]{
            new TradeWith_1(currentState),
            new TradeWith_2(currentState),
            new TradeWith_3(currentState),
            new TradeWith_4(currentState),
            new TradeWith_5(currentState),
            new TradeWith_6(currentState),
            new TradeWith_7(currentState),
            new TradeWith_8(currentState)
        };

        for(int i = 0; i < trades.Length; i++)
        {
            //if preconditions are met, it's a possible action
            if (trades[i].IsValid())
            {
               
                actions.Add(trades[i]);
                //Debug.Log("Possible : " + trades[i].toString());
            }
        }

        //backpack is full && no more usefull trades to do
        if (actions.Count == 0)
        {
            //return stash function
            StashAll stash = new StashAll(currentState);
            stash.IsValid();
            return stash;
        }
        else
        {
            actions.Sort();
            //Debug.Log("Out of : " + actions.Count);
            return actions[actions.Count - 1]; //get last
        }
    }


    //never happens
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Thief")
        {
            if (thief.stealing)
            {
                thief.touchedPlayer = true;
                beingRobbed = true;
            }
        }
    }

    public void SetDestination(Vector3 position)
    {
        
        agent.SetDestination(position);
        
    }

    public void UpdateUI()
    {
        foreach (Inventory inventory in (Inventory[])Enum.GetValues(typeof(Inventory)))
        {
            string s = "";
            foreach (Spice spice in (Spice[])Enum.GetValues(typeof(Spice)))
            {
                s += worldState[inventory][spice];
                s += "   ";


            }

            if (inventory == Inventory.Player)
                UIPlayer.text = "Player: " + s;
            else
                UICaravan.text = "Caravan: " + s;
        }

        Debug.Log("NewState : " + State.StateToString(worldState));
    } 

}



