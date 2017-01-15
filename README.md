# SurroundGame

##Introduction
This repository contains the Visual Studio solution of a WPF application, which implements a simple game playable by up to 6 players. The project was developed in MVVM architecture.

##About the Game
The game may be playable alone or by up to 6 players. The game is played on an NxM size field, where players place 2x1 size bricks in turns. If a player surrounds an area on the game field completely by his own bricks, the enclosed area will be acquired by that player, including any territory previously claimed by other players within. Each square yields 1 point to the player occupying it, including squares occupied by bricks. The game ends when no more bricks can be placed, and the player with the most points win. The size of the game field can be set in the game's main menu, and both it's row-, and column number may range between 6 and 30. 

##To-Do
- Rework of model-side gamefield to use newly introduced Tile nodes instead of enums.
- Integration of newly introduced Player nodes into model-side Player classes.
- Complete rework of step generation:
  - StepGenerator class will be raised to static level to support planned changes.
  - Introducing changelists, enabling an easy undo-redo mechanism
    - Stepgenerator will create a list of changes made in the actual step.
    - Elements inside the list will contain information about the nature of the change (type of action, further informatione xclusive to the specific action type), and the values associated with the action before and after the step.  
    - Changelist will be stored inside the reworked GameTracker class, associated with the initial information of the step (Position, Brick rotation, Player).
  - Current StepGenerator class is to be replaced by a Graph-based step generator.
    - A graph-like representation of the game field will be implemented inside the step generator, which contains the relevant squares on the game field, and the connection between neighbouring graph nodes.
      - A relevant square is one which is a wall (brick) field, is either an intersection, or a corner.
      - Fields, that have a same-color wall on all four non-diagonal adjacent tiles, and walls or occupied fields belonging tot he same player on all diagonal adjacent tiles are excluded from the graph structure, as they bear no relevancy. 
    - A width-first traverse of the graph will be constructed originating from the brick, which will help us decide whether the palcement of the brick creates a new enclosed area, or not.
    - Each step will go as follows:
      - First, initial conditions will be tested whether the brick can be placed at the given position.
      - Then the tiles surrounding the placed brick are grouped.
        - The tiles are put into a list, and checked, whether or not they contain walls belonging to the current player.
        - Wall tiles, that are connected to each other through the tiles inside the list (tiles adjacent to the brick) are put into the same group.
        - This way, tiles within the same group will be connected to each other, while tiles in different groups are not directly connected,
      - These groups will serve as the roots of the tree that traverses the graph.
        - The true root of the tree will be the brick itself, but we exclude the brick from the tree to make he algorithm easier.
      - Each tile inside the initial groups will be checked whether they are eligible to be graph nodes, and graph nodes are created accordingly.
      - After this, a basic width-first traverse will be executed, using the graph nodes in the starting groups as roots.
        - The goal is to find the shortest road between the different roots, if any exists.
        - During traversing, each branch of the tree will also store the tile closest to the top left corner ont hat branch. This will be needed to determine where to start filling up the enclosed area, if an enclosure is determined.
      - After the traverse ends, newly enclosed areas are filled with the current player's field color, and point differences are determined accordingly.
      - Finally, the step generator generates the output game field, and changelist from the current step.
- Refactoring of game tracking related classes, and reworking to use changelists.
- Implementation of playback mechanics, enabling replay of previous game steps (or even undo of them).
- Expanding the view and viewmodel of the game window to support playback.
