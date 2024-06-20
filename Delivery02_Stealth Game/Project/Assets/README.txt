Summary
Vampire is an engaging 2D top-down that challenges players to complete the level avoiding enemies with the maximum score 

How to Play
Movement: Use AWSD or the arrow keys to move the character.
Reload Level: Hit Enter to reload the level.
Exit Game: Press Escape to exit the game at any time.

Resources
Sprites: Most of sprites are made by our team. These can be found in the Sprites folder, organized by respective licenses.
SFX: Sound effects are generated using rFXGen.

Design Patterns Used
- Singleton Pattern
- Observer Pattern

Conventions Used
- PascalCase for Folder Names
- snake_case for File Names

Small Details/Polish
Calculates the player distance based on a position list, so the distance is always accurat, no matter the collision with the walls.
Fade in and fade out when you enter and exit the level.
Patrol system escalable, you can add as many points as you want to the patrol system.