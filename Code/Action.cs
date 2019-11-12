using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : IComparable
{
    public State postConditions;
    public float cost { get; protected set; }
    public bool isMethod { get; protected set; } //is it a primitive action or a method?

    public Dictionary<Spice, int> actionValue = new Dictionary<Spice, int>
    {
        {Spice.Turmeric, 1},
        {Spice.Saffron,  2},
        {Spice.Cardamom, 3},
        {Spice.Cinnamon, 4},
        {Spice.Cloves,   5},
        {Spice.Pepper,   6},
        {Spice.Sumac,    7},
    };


    //all actions remove or add to players inventory
    public Action(State currentState)
    {
        isMethod = false;

        postConditions = new State();
        //copy current state
        postConditions[Inventory.Caravan] = new Dictionary<Spice, int>(currentState[Inventory.Caravan]);
        postConditions[Inventory.Player] = new Dictionary<Spice, int>(currentState[Inventory.Player]);
    }

    protected bool Add(Spice spice, int amount, State postConditions)
    {
        if (SpiceCount(postConditions) <= 4)
        {
            postConditions[Inventory.Player][spice] += amount;
            return SpiceCount(postConditions) <= 4; //verify again after adding, if the backpack would be overfilled
        }
        else
        {
            return false;
        }
    }

    protected bool Remove(Spice spice, int amount, State postConditions)
    {
        int currentAmount = postConditions[Inventory.Player][spice] + postConditions[Inventory.Caravan][spice];
        if (currentAmount >= amount)
        {

            //prioritize taking from players inventory
            if (postConditions[Inventory.Player][spice] >= amount)
            {
                postConditions[Inventory.Player][spice] -= amount;
            }
            else
            {
                //if backpack is full, need to stash and take stuff needed for it
                //need taking from backpack action
                //this is a method, if bp is full need to stash take out and take out again to reach the end state
                isMethod = true; //it's a method, since it requires withdrawing from the caravans

                postConditions[Inventory.Caravan][spice] -= amount - postConditions[Inventory.Player][spice];
                postConditions[Inventory.Player][spice] = 0;
                //Debug.Log("Did transfer from caravan, current back pack : " + SpiceCount() );
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private int SpiceCount(State state)
    {
        int total = 0;
        foreach (var spice in state[Inventory.Player])
        {
            total += spice.Value;
        }

        return total;
    }

    //just calculate the value/per occupied slot inside backpack, if more than 2 of anyspice have 0 value
    protected float ActionCost()
    {
        float total = 0;
        int occupiedSlots = 0;

        foreach(var spice in postConditions[Inventory.Player])
        {
            bool isMoreThanTwo = (spice.Value + postConditions[Inventory.Caravan][spice.Key]) > 2;//no difference between backpack and caravan stash
            occupiedSlots += spice.Value;
            if (isMoreThanTwo) //once a set is complete no urge to get those spices
            {
                return 0;//return 0 value if we already have a spice
            }
            else
            {
                total += actionValue[spice.Key];
            }
        }

        return total / occupiedSlots; //the urge to get better spices >>> have completed previous == maximize value per occupied slot
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Action otherAction = obj as Action;
        if (otherAction != null)
            return this.cost.CompareTo(otherAction.cost);
        else
            throw new ArgumentException("Object is not an Action");
    }

    public abstract bool IsValid();
    public abstract void Precondition(State state);
    public abstract string toString();
    public abstract void Execute(PlayerController controller);
    //public abstract void ApplyActionOnState(State state);

    //can always add to caravan
    protected void AddToCaravan(Spice spice, int amount, State postConditions)
    {
        postConditions[Inventory.Caravan][spice] += amount;
    }

    protected bool computeValueIfValid(bool func)
    {
        if (func)
            cost = ActionCost();

        return cost != 0;
    }
}

public class TradeWith_1 : Action
{
    public TradeWith_1(State worldState) : base(worldState)
    {
    }

    //Trader 1: Gives you 2 turmeric units.
    public override bool IsValid()
    {
        bool add = Add(Spice.Turmeric, 2, postConditions);
        return add;
    }

    public override void Precondition(State state)
    {
        //don't need anything from caravan for trade1
    }

    public override void Execute(PlayerController controller)
    {
        //trade1 is always a primitive action since it does not require withdrawing
        controller.SetDestination(InitTraders.traders[1].position);
    }

    public override string toString()
    {
        return "Trade 1";
    }
}

public class TradeWith_2 : Action
{
    public TradeWith_2(State worldState) : base(worldState)
    {
    }

    //Trader 2: Takes 2 Turmeric units and gives you 1 Saffron unit.
    public override bool IsValid()
    {
        bool remove = Remove(Spice.Turmeric, 2, postConditions);
        bool add = Add(Spice.Saffron, 1, postConditions);

        return computeValueIfValid(remove && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Turmeric, 2, postConditions);
        AddToCaravan(Spice.Turmeric, -2, state);
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[2].position);
    }

    public override string toString()
    {
        return "Trade 2";
    }
}

public class TradeWith_3 : Action
{
    public TradeWith_3(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove = Remove(Spice.Saffron, 2, postConditions);
        bool add = Add(Spice.Cardamom, 1, postConditions);
        return computeValueIfValid(remove && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Saffron, 2, state);
        AddToCaravan(Spice.Saffron, -2, state);
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[3].position);
    }

    public override string toString()
    {
        return "Trade 3";
    }

}

public class TradeWith_4 : Action
{
    public TradeWith_4(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove = Remove(Spice.Turmeric, 4, postConditions);
        bool add = Add(Spice.Cinnamon, 1, postConditions);
        return computeValueIfValid(remove && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Turmeric, 4, state);
        AddToCaravan(Spice.Turmeric, -4, state);
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[4].position);
    }

    public override string toString()
    {
        return "Trade 4";
    }

}

public class TradeWith_5 : Action
{
    public TradeWith_5(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove1 = Remove(Spice.Cardamom, 1, postConditions);
        bool remove2 = Remove(Spice.Turmeric, 1, postConditions);
        bool add = Add(Spice.Cloves, 1, postConditions);
        return computeValueIfValid(remove1 && remove2 && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Cardamom, 1, state);
        Add(Spice.Turmeric, 1, state);
        AddToCaravan(Spice.Cardamom, -1, state);
        AddToCaravan(Spice.Turmeric, -1, state);
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[5].position);
    }

    public override string toString()
    {
        return "Trade 5";
    }

}

public class TradeWith_6 : Action
{
    public TradeWith_6(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove1 = Remove(Spice.Turmeric, 2, postConditions);
        bool remove2 = Remove(Spice.Saffron, 1, postConditions);
        bool remove3 = Remove(Spice.Cinnamon, 1, postConditions);
        bool add = Add(Spice.Pepper, 1, postConditions);
        return computeValueIfValid(remove1 && remove2 && remove3 && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Turmeric, 2, state);
        Add(Spice.Saffron, 1, state);
        Add(Spice.Cinnamon, 1, state);
        AddToCaravan(Spice.Turmeric, -2, state);
        AddToCaravan(Spice.Saffron, -1, state);
        AddToCaravan(Spice.Cinnamon, -1, state);
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[6].position);
    }

    public override string toString()
    {
        return "Trade 6";
    }

}

public class TradeWith_7 : Action
{
    public TradeWith_7(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove1 = Remove(Spice.Cardamom, 4, postConditions);
        bool add = Add(Spice.Sumac, 1, postConditions);
        return computeValueIfValid(remove1 && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Cardamom, 4, state);
        AddToCaravan(Spice.Cardamom, -4, state);
    }


    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[7].position);
    }

    public override string toString()
    {
        return "Trade 7";
    }

}

public class TradeWith_8 : Action
{
    public TradeWith_8(State worldState) : base(worldState)
    {
    }

    // Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.
    public override bool IsValid()
    {
        bool remove1 = Remove(Spice.Saffron, 1, postConditions);
        bool remove2 = Remove(Spice.Cinnamon, 1, postConditions);
        bool remove3 = Remove(Spice.Cloves, 1, postConditions);
        bool add = Add(Spice.Sumac, 1, postConditions);
        return computeValueIfValid(remove1 && remove2 && remove3 && add);
    }

    public override void Precondition(State state)
    {
        Add(Spice.Saffron, 1, state);
        Add(Spice.Cinnamon, 1, state);
        Add(Spice.Cloves, 1, state);
        AddToCaravan(Spice.Saffron, -1, state);
        AddToCaravan(Spice.Cinnamon, -1, state);
        AddToCaravan(Spice.Cloves, -1, state);
    }


    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[8].position);
    }

    public override string toString()
    {
        return "Trade 8";
    }

}

public class StashAll : Action
{
    public StashAll(State worldState) : base(worldState)
    {
    }

    public override bool IsValid()
    {
        foreach (Spice spice in (Spice[])Enum.GetValues(typeof(Spice)))
        {
            int backPackValue = postConditions[Inventory.Player][spice];

            if (backPackValue > 0)
            {
                Remove(spice, backPackValue, postConditions);
                AddToCaravan(spice, backPackValue, postConditions);
            }
        }
        return true; //always possible to stash
    }

    public override void Precondition(State state)
    {
    }

    public override void Execute(PlayerController controller)
    {
        controller.SetDestination(InitTraders.traders[0].position);
    }

    public override string toString()
    {
        return "StashAll";
    }

}

public class TransferOUT : Action
{
    public TransferOUT(State worldState, Action trade) : base(worldState)
    {
        trade.Precondition(postConditions);
    }

    public override bool IsValid()
    {
        return true;
    }

    public override void Precondition(State state)
    {
    }

    public override void Execute(PlayerController controller)
    {
        //controller.SetDestination(InitTraders.traders[0].position);
        //TransferOUT can only happen after stashing
        //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().arrived = true;
        //Set arrived to true
    }

    public override string toString()
    {
        return "TransferOUT";
    }
    //execute set destination to caravan
}

