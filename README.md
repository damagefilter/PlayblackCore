# Playblack Core Library
Library with some common and hopefully useful functionality to use with the Unity game engine.
 
Currently under development. Described features may not be present yet as I am extracting (and mostly rewriting) them
from a lot of code from different prototypes I made over the years.


## Contents

### Save Games
Saves game state based on saveable components.
Each game object can have a save manager.
This save manager will read all components attached to the gameobject
and extract their states as defined by attributes on fields and properties.
This is serialized into byte arrays, zipped and written to a save file.


### Behaviour Trees
A very very basic behaviour tree implementation based on the JBT library.
It has an editor interface and works on two kinds of components.
AI components and sequencer components.


### Sequencer
An editor based on the BT that can be used to create scripted sequences, dialogues and many many more things.


### Component Signal Processing (CSP)
An event system revolving around the game world.
Think this: You want a counter in your game and once the counter hits some defined maximum,
you want a door to be opened. Once the player walked through that door you want a scripted sequence (Sequencer)
to fire up that shows how the lights in the corridor flicker, a camera drive through the corridor and back.
After that, give control back to the player.
And you want to create this in the editor without any special code to define all this logic.
With CSP you can do just that.


### Event System
Event system where events are raised based on method signatures.
If you ever delved into the world of minecraft modding, you'll feel right at home.
Example for a callback:

 void OnSave(GameSavingEvent event) { /*DO something!*/}

Will be raised like so:

 new GameSavingEvent().Call();

Okay!


## Compiling

To compile this dll you'll have to reference at least the UnityEngine.dll in the project.
(And possibly remove some existing references from the project)
Unfortunately I know no way of making this dynamic and redistributing the dll seems too shady.

