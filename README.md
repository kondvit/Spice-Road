# Spice-Road

(Disclaimer) This is my most recent ongoing project. The code may look raw and many comments are not removed yet.

## Goal

The goal of this project is to implement a Goal Oriented Action Planner (GOAP) AI system. The player agent (green) has to collect two of each spice by making intelligent trades. The complexity of the planner comes from the thief (red). Every 5 seconds the thief has 33% chance of stealing from the player. The player agent has to adapt to the new state and solve the problem. 

### The following trades are possible:

(a) Trader 1: Gives you 2 turmeric units.

(b) Trader 2: Takes 2 Turmeric units and gives you 1 Saffron unit.

(c) Trader 3: Takes 2 Saffron units and gives you 1 Cardamom unit.

(d) Trader 4: Takes 4 Turmeric units and gives you 1 Cinnamon.

(e) Trader 5: Takes 1 Cardamom and 1 Turmeric and gives you 1 Cloves unit.

(f) Trader 6: Takes 2 Turmeric, 1 Saffron and 1 Cinnamon and gives 1 Pepper unit.

(g) Trader 7: Takes 4 Cardamom units and gives you 1 Sumac unit.

(h) Trader 8: Takes 1 Saffron, 1 Cinnamon and 1 Cloves unit and gives you 1 Sumac unit.

### Aditional rules:
The player has a permanent unlimited storage space, the caravan.
The player can only hold 4 spices at a time.

### Files:
  - PlayerController.cs contains all the planner code
  - Action.cs a class for instantiating an action for the agent

