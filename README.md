# Cordyceps
*A TAS mod for Rain World*

*By Error: String Expected, Got Nil*

Cordyceps is a simple TASing tool for Rain World, allowing you to manually control the tickrate of the game, or pause it and advance tick-by-tick. A "tick" in this case means a single physics step for the game's simulation. While these are often called "frames", it's important to rememeber these are distinct from graphical frames, so I prefer the term "tick".

In vanilla Rain World, the tickrate is 40 ticks/second, occasionally modified in-game (most commonly by eating a mushroom, being near an Echo, or being in the Depths). Cordyceps allows you to manually cap the tickrate at any value from 1 to 40 Hz. The speedrun/file timer is automatically adjusted to ignore Cordyceps' own slowdown, but still responds normally to vanilla tickrate changes.

Keybinds for the various functions are configurable in the Remix menu, but are set to the following by default:
- Show/hide info panel: \[M\]
- Toggle tickrate cap: \[Comma\]
- Increase tickrate cap: \[Equals\]
- Decrease tickrate cap: \[Minus\]
- Toggle pause: \[Period\]
- Tick advance: \[Forward Slash\]
- Reset tick counter: \[Semicolon\]
- Pause/unpause tick counter: \[Single Quote\]

Pressing and holding the increase/decrease tickrate cap keys will make them rapidly change value, so you don't have to repeatedly press them over and over.

## Note on the Pause Function
The game is still running while paused with Cordyceps. Functionally, it is equivalent to pausing the game with the standard pause button, except the pause menu doesn't appear, and your inputs aren't eaten by it (since it doesn't exist).

## Note on the Tick Advance Function
Tick advance can only be used while the game is paused with Cordyceps, and does not work by immediately forcing the next tick to run. Instead, it simply unpauses the game until the next tick occurs, then automatically pauses it again for you. Any inputs you want to occur on the tick you advance to should be held down while you press the tick advance key. Tick advance is best used while the tickrate cap is set to a low number, I've found about 10 Hz works pretty well.
