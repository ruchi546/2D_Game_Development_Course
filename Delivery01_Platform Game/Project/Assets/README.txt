Summary
BubLuna is an engaging 2D platformer that challenges players to collect as many bubbles as possible.

How to Play
Movement: Use the A and D keys to move left and right.
Jump: Press the Space bar to make your character leap.
Reload Level: Hit Enter to reload the level.
Exit Game: Press Escape to exit the game at any time.

Resources
Sprites: With the exception of the gameplay tiles, all sprites are made by our team. These can be found in the Sprites folder, organized by respective licenses.
SFX: Sound effects are generated using rFXGen.

Design Patterns Used
Singleton Pattern: Employed in the managers of the game.
Observer Pattern: Implemented in Bubbles/Coin, Power Up, Die, and Win GameObjects.

Small Details/Polish
Fade in and fade out when you enter and exit the level.
Interpolation of the number of bubbles when you collect them.
Smooth camera movement.
Background Parallax Scrolling.