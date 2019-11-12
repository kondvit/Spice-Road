using System.Collections.Generic;
using System;

public enum Spice { Turmeric, Saffron, Cardamom, Cinnamon, Cloves, Pepper, Sumac }

public enum Inventory { Player, Caravan }

//Just to make it easier to write the type everytime
public class State : Dictionary<Inventory, Dictionary<Spice, int>>
{
    public State DeepCopy()
    {
        State copy = new State();

        copy[Inventory.Caravan] = new Dictionary<Spice, int>(this[Inventory.Caravan]);
        copy[Inventory.Player] = new Dictionary<Spice, int>(this[Inventory.Player]);

        return copy;
    }

    public static string StateToString(State state)
    {
        string f = "Backpack: ";
        foreach (Inventory inventory in (Inventory[])Enum.GetValues(typeof(Inventory)))
        {
            string s = "";
            foreach (Spice spice in (Spice[])Enum.GetValues(typeof(Spice)))
            {
                s += state[inventory][spice];
                s += " ";


            }

            f += s;
            if (inventory == Inventory.Player)
                f += " Caravan: ";
        }

        return f;
    }
}
