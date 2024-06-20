/*******************************************************************************************
*
*   raylib maze generator
*
*   Procedural maze generator using Maze Grid Algorithm
*
*   This game has been created using raylib (www.raylib.com)
*   raylib is licensed under an unmodified zlib/libpng license (View raylib.h for details)
*
*   Copyright (c) 2024 Ramon Santamaria (@raysan5)
*
********************************************************************************************/

#include "raylib.h"
#define RAYGUI_IMPLEMENTATION
#include "raygui.h"                     // Required for immediate-mode UI elements
#include <stdlib.h>                     // Required for: malloc(), free()

#define MAZE_WIDTH          64
#define MAZE_HEIGHT         64
#define MAZE_DRAW_SCALE     10.0f

#define MAX_MAZE_ITEMS      16
#define TIME_LIMIT_SECONDS 90000

// Declare new data type: Point
typedef struct Point
{
    int x;
    int y;
} Point;

typedef struct CollectibleItem
{
    Point position;
    bool collected;
    int value;
    Color color;
} CollectibleItem;

// Generate procedural maze image, using grid-based algorithm
// NOTE: Functions defined as static are internal to the module
static Image GenImageMaze(int width, int height, int spacingRows, int spacingCols, float skipChance);

// Set collectible items in the maze
static void SetCollectibleItems(Image imMaze, Point startCells[], Point endCells[], CollectibleItem collectibleItems[]);

// Get shorter path between two points, implements pathfinding algorithm: A*
static Point* LoadPathAStar(Image map, Point start, Point end, int* pointCount);

//----------------------------------------------------------------------------------
// Main entry point
//----------------------------------------------------------------------------------
int main(void)
{
    // Initialization
    //---------------------------------------------------------
    const int screenWidth = 1280;
    const int screenHeight = 720;

    const int spacingRows = 4;
    const int spacingCols = 4;

    InitWindow(screenWidth, screenHeight, "raylib maze generator");

    // Current application mode
    int currentMode = 0;    // 0-Game2D, 1-Game3D, 2-Editor

    // Random seed defines the random numbers generation,
    // always the same if using the same seed
    SetRandomSeed(67218);

    // Generate maze image using the grid-based generator
    // DONE: [1p] Improve function to support extra configuration parameters 
    Image imMaze = GenImageMaze(MAZE_WIDTH, MAZE_HEIGHT, spacingRows, spacingCols, 0.3f);

    // Load a texture to be drawn on screen from our image data
    // WARNING: If imMaze pixel data is modified, texMaze needs to be re-loaded
    Texture texMaze = LoadTextureFromImage(imMaze);

    // Generate 3D mesh from image and load a 3D model from mesh 
    Mesh meshMaze = GenMeshCubicmap(imMaze, (Vector3) { 1.0f, 1.0f, 1.0f });
    Model mdlMaze = LoadModelFromMesh(meshMaze);
    Vector3 mdlPosition = { GetScreenWidth() / 2 - texMaze.width * MAZE_DRAW_SCALE / 2, GetScreenHeight() / 2 - texMaze.height * MAZE_DRAW_SCALE / 2 };  // Set model position

    // Start and end cell positions (user defined)
    Point startCells[4] = {
        { 1, 1 },
        { 1, 2 },
        { 2, 1 },
        { 2, 2 }
    };

    Point endCells[4] = {
        { imMaze.width - 3, imMaze.height - 3 },
        { imMaze.width - 3, imMaze.height - 2 },
        { imMaze.width - 2, imMaze.height - 3 },
        { imMaze.width - 2, imMaze.height - 2 }
    };

    int endCellsCount = sizeof(endCells) / sizeof(Point);

    // Player current position on image-coordinates
    // WARNING: It could require conversion to world coordinates!
    Point playerCell = startCells[0];

    // Camera 2D for 2d gameplay mode
    // DONE: Initialize camera parameters as required
    Camera2D camera2d = { 0 };
    camera2d.target = (Vector2){ mdlPosition.x + playerCell.x * MAZE_DRAW_SCALE, mdlPosition.y + playerCell.y * MAZE_DRAW_SCALE };
    camera2d.offset = (Vector2){ GetScreenWidth() / 2, GetScreenHeight() / 2 };
    camera2d.rotation = 0.0f;
    camera2d.zoom = 1.0f;

    // Camera 3D for first-person gameplay mode
    // DONE: Initialize camera parameters as required
    // NOTE: In a first-person mode, camera.position is actually the player position
    // REMEMBER: We are using a different coordinates space than 2d mode
    Camera cameraFP = { 0 };
    cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y };
    cameraFP.target = (Vector3){ cameraFP.position.x + 1.0f, cameraFP.position.y, cameraFP.position.z + 1.0f };
    cameraFP.up = (Vector3){ 0.0f, 1.0f, 0.0f };
    cameraFP.fovy = 45.0f;
    cameraFP.projection = CAMERA_PERSPECTIVE;
    Color* mapPixels = LoadImageColors(imMaze);

    // Mouse selected cell for maze editing
    Point selectedCell = { 0 };

    // Maze items position and state
    CollectibleItem collectibleItems[MAX_MAZE_ITEMS];
    int playerScore = 0;

    // Generate random collectible items in the maze
    SetCollectibleItems(imMaze, startCells, endCells, collectibleItems);

    // Define textures to be used as our "biomes"
    // DONE: Load additional textures for different biomes
    Texture2D texBiomes[4] = { 0 };
    int texBiomesCount = sizeof(texBiomes) / sizeof(Texture2D);
    for (int i = 0; i < texBiomesCount; i++)
    {
        texBiomes[i] = LoadTexture(TextFormat("resources/maze_atlas%02d.png", i + 1));
    }

    int currentBiome = 0;
    mdlMaze.materials[0].maps[MATERIAL_MAP_DIFFUSE].texture = texBiomes[currentBiome];

    // Background music
    InitAudioDevice();
    float masterVolume = 0.5f;
    bool pause = false;

    Music music = LoadMusicStream("resources/music_background.mp3"); // WAV not working
    PlayMusicStream(music);
    SetMusicVolume(music, masterVolume);

    // Audio fx
    Sound fxWin = LoadSound("resources/fx_win.mp3");
    Sound fxCoin = LoadSound("resources/fx_coin.mp3");
    Sound fxDie = LoadSound("resources/fx_die.mp3");
    SetSoundVolume(fxWin, masterVolume);
    SetSoundVolume(fxCoin, masterVolume);
    SetSoundVolume(fxDie, masterVolume);
    Sound sounds[] = { fxWin, fxCoin, fxDie };
    int soundsCount = sizeof(sounds) / sizeof(Sound);

    // DONE: Define all variables required for game UI elements (sprites, fonts...)
    bool drawPathAStar = false;
    int remainingTime = TIME_LIMIT_SECONDS;
    Model collectable3D = LoadModel("resources/mdl_pickup.glb");

    // DONE: Define all variables required for UI editor (raygui)
    int mapRowSpacing = 4;
    int mapColumnSpacing = 4;
    int mapRandomSeed = 67218;
    float mapPointsSkipChance = 0.3f;

    SetTargetFPS(60);       // Set our game to run at 60 frames-per-second
    //--------------------------------------------------------------------------------------

    // Main game loop
    while (!WindowShouldClose())    // Detect window close button or ESC key
    {
        // Update
        //----------------------------------------------------------------------------------

        // Update music stream buffer
        UpdateMusicStream(music);

        // Select current mode as desired
        if (IsKeyPressed(KEY_Z)) currentMode = 0;       // Game 2D mode
        else if (IsKeyPressed(KEY_X)) currentMode = 1;  // Game 3D mode
        else if (IsKeyPressed(KEY_C)) currentMode = 2;  // Editor mode

        if (IsKeyPressed(KEY_SPACE)) drawPathAStar = !drawPathAStar;

        switch (currentMode)
        {
        case 0:     // Game 2D mode
        {
            ShowCursor();

            // DONE: [2p] Player 2D movement from predefined start point (A) to end point (B)
            Point prevplayerCell = playerCell;

            // Implement maze 2D player movement logic (cursors || WASD)
            if (IsKeyDown(KEY_UP) || IsKeyDown(KEY_W)) playerCell.y -= 1;
            if (IsKeyDown(KEY_DOWN) || IsKeyDown(KEY_S)) playerCell.y += 1;
            if (IsKeyDown(KEY_LEFT) || IsKeyDown(KEY_A)) playerCell.x -= 1;
            if (IsKeyDown(KEY_RIGHT) || IsKeyDown(KEY_D)) playerCell.x += 1;

            // Use imMaze pixel information to check collisions
            if (GetImageColor(imMaze, playerCell.x, playerCell.y).r == 255) playerCell = prevplayerCell;

            // Detect if current playerCell == endCell to finish game
            for (int i = 0; i < endCellsCount; i++)
            {
                if (playerCell.x == endCells[i].x && playerCell.y == endCells[i].y)
                {
                    remainingTime = TIME_LIMIT_SECONDS; // Reset remaining time
                    playerCell = startCells[0];      // Reset player 2D position
                    cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y }; // Reset camera 3D position
                    playerScore = 0; // Reset player score
                    SetCollectibleItems(imMaze, startCells, endCells, collectibleItems); // Reset collectible items
                    PlaySound(fxWin);
                }
            }

            // DONE: [2p] Camera 2D system following player movement around the map
            // Update Camera2D parameters as required to follow player and zoom control
            camera2d.target = (Vector2){ mdlPosition.x + playerCell.x * MAZE_DRAW_SCALE, mdlPosition.y + playerCell.y * MAZE_DRAW_SCALE };
            camera2d.zoom += ((float)GetMouseWheelMove() * 0.5f);

            // Update 3d camera position (to stay in sync with 2d camera when changing mode)
            cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y };

            // Camera zoom
            if (camera2d.zoom > 6.0f) camera2d.zoom = 6.0f;
            else if (camera2d.zoom < 0.2f) camera2d.zoom = 0.2f;

            // DONE: Maze items pickup logic
            for (int i = 0; i < MAX_MAZE_ITEMS; i++)
            {
                if (!collectibleItems[i].collected && (playerCell.x == collectibleItems[i].position.x) && (playerCell.y == collectibleItems[i].position.y))
                {
                    collectibleItems[i].collected = true;
                    playerScore += collectibleItems[i].value;
                    PlaySound(fxCoin);
                }
            }

            // Time limit logic
            remainingTime -= GetFrameTime() * 1000;

            if (remainingTime <= 0)
            {
                remainingTime = TIME_LIMIT_SECONDS; // Reset remaining time
                playerCell = startCells[0];      // Reset player 2D position
                cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y }; // Reset camera 3D position
                playerScore = 0; // Reset player score
                SetCollectibleItems(imMaze, startCells, endCells, collectibleItems); // Reset collectible items
                PlaySound(fxDie);
            }

            } break;
        case 1:     // Game 3D mode
        {
            // DONE: [1p] Camera 3D system and “3D maze mode”
            // Implement maze 3d first-person mode -> TIP: UpdateCamera()
            // Use the imMaze map to implement collision detection, similar to 2D
            Vector3 oldCamPos = cameraFP.position;
            UpdateCamera(&cameraFP, CAMERA_FIRST_PERSON);
            DisableCursor();

            Vector2 playerPos = { cameraFP.position.x, cameraFP.position.z };
            float playerRadius = 0.1f;

            int playerCellX = (int)(playerPos.x - mdlPosition.x + 0.5f);
            int playerCellY = (int)(playerPos.y - mdlPosition.z + 0.5f);

            // Out-of-limits check
            if (playerCellX < 0) playerCellX = 0;
            else if (playerCellX >= texMaze.width) playerCellX = texMaze.width - 1;

            if (playerCellY < 0) playerCellY = 0;
            else if (playerCellY >= texMaze.height) playerCellY = texMaze.height - 1;

            // Check map collisions using image data and player position
            // DONE: Improvement: Just check player surrounding cells for collision
            for (int y = 0; y < texMaze.height; y++)
            {
                for (int x = 0; x < texMaze.width; x++)
                {
                    if ((mapPixels[y * texMaze.width + x].r == 255) &&
                        (CheckCollisionCircleRec(playerPos, playerRadius, (Rectangle) { mdlPosition.x - 0.5f + x * 1.0f, mdlPosition.z - 0.5f + y * 1.0f, 1.0f, 1.0f })))
                    {
                        cameraFP.position = oldCamPos;
                    }
                }
            }

            playerCell = (Point){ playerCellX, playerCellY };

            // DONE: Maze items pickup logic
            for (int i = 0; i < MAX_MAZE_ITEMS; i++)
            {
                if (!collectibleItems[i].collected && (playerCell.x == collectibleItems[i].position.x) && (playerCell.y == collectibleItems[i].position.y))
                {
                    collectibleItems[i].collected = true;
                    playerScore += collectibleItems[i].value;
                    PlaySound(fxCoin);
                }
            }

            // Check if player reaches the end cell
            for (int i = 0; i < endCellsCount; i++)
            {
                if (playerCell.x == endCells[i].x && playerCell.y == endCells[i].y)
                {
                    // Player reached the end cell, restart the game
                    remainingTime = TIME_LIMIT_SECONDS;
                    playerCell = startCells[0];
                    cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y };
                    playerScore = 0;
                    SetCollectibleItems(imMaze, startCells, endCells, collectibleItems);
                    PlaySound(fxWin);
                }
            }

            // Time limit logic
            remainingTime -= GetFrameTime() * 1000;

            if (remainingTime <= 0)
            {
                remainingTime = TIME_LIMIT_SECONDS;
                playerCell = startCells[0];
                cameraFP.position = (Vector3){ mdlPosition.x + playerCell.x, mdlPosition.y + 0.3f, mdlPosition.z + playerCell.y };
                playerScore = 0;
                SetCollectibleItems(imMaze, startCells, endCells, collectibleItems);
                PlaySound(fxDie);
            };

        } break;
        case 2:     // Editor mode
        {
            ShowCursor();

            // DONE: [2p] Visual “map editor mode”. Edit image pixels with mouse.
            // Check mouse position and update selected cell
            if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON) || IsMouseButtonPressed(MOUSE_RIGHT_BUTTON))
            {
                Vector2 mousePos = GetMousePosition();

                // Mouse position to image coordinates
                selectedCell.x = (int)((mousePos.x - mdlPosition.x) / MAZE_DRAW_SCALE);
                selectedCell.y = (int)((mousePos.y - mdlPosition.y) / MAZE_DRAW_SCALE);

                // Check if the selected cell is int the maze
                // +-1 to avoid drawing on the border
                if (selectedCell.x >= 1 && selectedCell.x < imMaze.width - 1 &&
                    selectedCell.y >= 1 && selectedCell.y < imMaze.height - 1)
                {
                    // Set pixel color based on mouse button
                    Color pixelColor = IsMouseButtonPressed(MOUSE_LEFT_BUTTON) ? BLACK : WHITE;
                    ImageDrawPixel(&imMaze, selectedCell.x, selectedCell.y, pixelColor);

                    UpdateTexture(texMaze, imMaze.data);
                    UnloadMesh(meshMaze);
                    meshMaze = GenMeshCubicmap(imMaze, (Vector3) { 1.0f, 1.0f, 1.0f });
                    mapPixels = LoadImageColors(imMaze);
                }
            }

            // DONE: [2p] Collectible map items: player score
            // Using same mechanism than map editor, implement an items editor, registering
            // points in the map where items should be added for player pickup -> TIP: mazeItems[]

            if (IsMouseButtonPressed(MOUSE_MIDDLE_BUTTON))
            {
                Vector2 mousePos = GetMousePosition();

                selectedCell.x = (int)((mousePos.x - mdlPosition.x) / MAZE_DRAW_SCALE);
                selectedCell.y = (int)((mousePos.y - mdlPosition.y) / MAZE_DRAW_SCALE);

                // If the selected cell is not a wall, add the item
                if (GetImageColor(imMaze, selectedCell.x, selectedCell.y).r == 0)
                {
                    // Choose a random item of the collectible items array
                    int item = GetRandomValue(0, MAX_MAZE_ITEMS - 1);

                    // Add item to the collectible items array
                    collectibleItems[item].position = selectedCell;
                    UnloadMesh(meshMaze);
                    meshMaze = GenMeshCubicmap(imMaze, (Vector3) { 1.0f, 1.0f, 1.0f });
                }
            }
        } break;
        default: break;
        }

        // DONE: [1p] Multiple maze biomes supported
        // Implement changing between the different textures to be used as biomes
        // NOTE: For the 3d model, the current selected texture must be applied to the model material 
        if (IsKeyPressed(KEY_ONE))
        {
            currentBiome = 0;
            mdlMaze.materials[0].maps[MATERIAL_MAP_DIFFUSE].texture = texBiomes[currentBiome];
        }
        else if (IsKeyPressed(KEY_TWO))
        {
            currentBiome = 1;
            mdlMaze.materials[0].maps[MATERIAL_MAP_DIFFUSE].texture = texBiomes[currentBiome];
        }
        else if (IsKeyPressed(KEY_THREE))
        {
            currentBiome = 2;
            mdlMaze.materials[0].maps[MATERIAL_MAP_DIFFUSE].texture = texBiomes[currentBiome];
        }
        else if (IsKeyPressed(KEY_FOUR))
        {
            currentBiome = 3;
            mdlMaze.materials[0].maps[MATERIAL_MAP_DIFFUSE].texture = texBiomes[currentBiome];
        }
        //----------------------------------------------------------------------------------

        // Draw
        //----------------------------------------------------------------------------------
        BeginDrawing();

        ClearBackground(RAYWHITE);

        switch (currentMode)
        {
        case 0:     // Game 2D mode
        {
            BeginMode2D(camera2d);

            // Draw lines rectangle over texture, scaled and centered on screen 
            DrawRectangleLines(GetScreenWidth() / 2 - texMaze.width * MAZE_DRAW_SCALE / 2, GetScreenHeight() / 2 - texMaze.height * MAZE_DRAW_SCALE / 2, MAZE_WIDTH * MAZE_DRAW_SCALE, MAZE_HEIGHT * MAZE_DRAW_SCALE, RED);

            // DONE: Draw maze walls and floor using current texture biome 
            for (int y = 0; y < imMaze.height; y++)
            {
                for (int x = 0; x < imMaze.width; x++)
                {
                    if (GetImageColor(imMaze, x, y).r == 255)
                    {
                        DrawTexturePro(texBiomes[currentBiome], (Rectangle) { 0, texBiomes[currentBiome].height / 2, texBiomes[currentBiome].width / 2, texBiomes[currentBiome].height / 2 },
                            (Rectangle) {
                            mdlPosition.x + x * MAZE_DRAW_SCALE, mdlPosition.y + y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE
                        }, (Vector2) { 0, 0 }, 0.0f, WHITE);
                    }
                    else if (GetImageColor(imMaze, x, y).r == 0)
                    {
                        DrawTexturePro(texBiomes[currentBiome], (Rectangle) { texBiomes[currentBiome].width / 2, texBiomes[currentBiome].height / 2, texBiomes[currentBiome].width / 2, texBiomes[currentBiome].height / 2 },
                            (Rectangle) {
                            mdlPosition.x + x * MAZE_DRAW_SCALE, mdlPosition.y + y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE
                        }, (Vector2) { 0, 0 }, 0.0f, WHITE);
                    }
                }
            }

            // DONE: Draw point A and B 
            for (int i = 0; i < endCellsCount; i++)
            {
                DrawRectangle(mdlPosition.x + startCells[i].x * MAZE_DRAW_SCALE, mdlPosition.y + startCells[i].y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, BLUE);
                DrawRectangle(mdlPosition.x + endCells[i].x * MAZE_DRAW_SCALE, mdlPosition.y + endCells[i].y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, RED);
            }

            // DONE: Draw player rectangle or sprite at player position
            DrawRectangle(mdlPosition.x + playerCell.x * MAZE_DRAW_SCALE, mdlPosition.y + playerCell.y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, GREEN);
            DrawRectangleLines(mdlPosition.x + playerCell.x * MAZE_DRAW_SCALE, mdlPosition.y + playerCell.y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, DARKGREEN);

            // DONE: Draw maze items 2d (using sprite texture?)
            for (int i = 0; i < MAX_MAZE_ITEMS; i++)
            {
                if (!collectibleItems[i].collected)
                {
                    DrawRectangle(mdlPosition.x + collectibleItems[i].position.x * MAZE_DRAW_SCALE, mdlPosition.y + collectibleItems[i].position.y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, collectibleItems[i].color);
                }
            }

            // DONE: EXTRA: Draw pathfinding result, shorter path from start to end
            if (drawPathAStar)
            {
                int pointCount = 0;
                Point* path = LoadPathAStar(imMaze, playerCell, endCells[0], &pointCount);

                for (int i = 1; i < pointCount - 1; i++) // i =1 to avoid drawing the player position and pointCount-1 to avoid drawing the end cell
                {
                    DrawRectangle(mdlPosition.x + path[i].x * MAZE_DRAW_SCALE, mdlPosition.y + path[i].y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, YELLOW);
                }
                free(path);
            }

            EndMode2D();

            // DONE: Draw editor UI required elements -> TIP: raygui immediate mode UI
            // NOTE: In immediate-mode UI, logic and drawing is defined together
            // REFERENCE: https://github.com/raysan5/raygui

            // DONE: Draw game UI (score, time...) using custom sprites/fonts

            // White rectangle for all the GUI
            DrawRectangle(0, 0, 270, GetScreenHeight(), Fade(RAYWHITE, 0.8f));

            // Game info
            GuiGroupBox((Rectangle) { 10, 10, 200, 60 }, "Game Info");
            GuiLabel((Rectangle) { 20, 20, 200, 20 }, TextFormat("Score: %d", playerScore));
            GuiLabel((Rectangle) { 20, 40, 200, 20 }, TextFormat("Time Remaining: %02d:%02d", remainingTime / 60000, (remainingTime % 60000) / 1000));

            // Editor info
            GuiGroupBox((Rectangle) { 10, 80, 250, 340 }, "Map Configuration");
            GuiLabel((Rectangle) { 20, 90, 200, 20 }, "Row Spacing");
            GuiLabel((Rectangle) { 100, 90, 200, 20 }, TextFormat("Current Value: %d", mapRowSpacing));
            mapRowSpacing = GuiButton((Rectangle) { 20, 110, 200, 20 }, "Increase") ? mapRowSpacing + 1 : mapRowSpacing;
            if (mapRowSpacing > 1)
            {
                mapRowSpacing = GuiButton((Rectangle) { 20, 140, 200, 20 }, "Decrease") ? mapRowSpacing - 1 : mapRowSpacing;
            }

            GuiLabel((Rectangle) { 20, 170, 200, 20 }, "Column Spacing");
            mapColumnSpacing = GuiButton((Rectangle) { 20, 200, 200, 20 }, "Increase") ? mapColumnSpacing + 1 : mapColumnSpacing;
            if (mapColumnSpacing > 1)
            {
                mapColumnSpacing = GuiButton((Rectangle) { 20, 230, 200, 20 }, "Decrease") ? mapColumnSpacing - 1 : mapColumnSpacing;
            }
            GuiLabel((Rectangle) { 100, 170, 200, 20 }, TextFormat("Current Value: %d", mapColumnSpacing));

            GuiLabel((Rectangle) { 20, 260, 200, 20 }, "Skip Chance");
            GuiSliderBar((Rectangle) { 20, 280, 200, 20 }, NULL, TextFormat("%.02f", mapPointsSkipChance), & mapPointsSkipChance, 0.0f, 1.0f);

            GuiLabel((Rectangle) { 20, 310, 200, 20 }, TextFormat("Current Seed: %d", mapRandomSeed));
            if (GuiButton((Rectangle) { 20, 330, 200, 20 }, "Random Seed"))
            {
                mapRandomSeed = GetRandomValue(0, 100000);
            }

            // Regenerate maze button (moved down)
            if (GuiButton((Rectangle) { 20, 360, 200, 50 }, "Regenerate Maze"))
            {
                imMaze = GenImageMaze(MAZE_WIDTH, MAZE_HEIGHT, mapRowSpacing, mapColumnSpacing, mapPointsSkipChance);
                UpdateTexture(texMaze, imMaze.data);
                UnloadImageColors(mapPixels);
                mapPixels = LoadImageColors(imMaze);

                //Update 3d model
                UnloadMesh(meshMaze);
                meshMaze = GenMeshCubicmap(imMaze, (Vector3) { 1.0f, 1.0f, 1.0f });

                //restart game
                remainingTime = TIME_LIMIT_SECONDS;
                playerCell = startCells[0];

                // Set collectible items
                SetCollectibleItems(imMaze, startCells, endCells, collectibleItems);
            }

            // Audio control
            GuiGroupBox((Rectangle) { 10, 430, 250, 100 }, "Audio Control");
            GuiLabel((Rectangle) { 20, 440, 200, 20 }, "Music Volume");
            GuiSliderBar((Rectangle) { 20, 460, 200, 20 }, NULL, TextFormat("%.02f", masterVolume), & masterVolume, 0.0f, 1.0f);
            SetMasterVolume(masterVolume);

            GuiCheckBox((Rectangle) { 20, 490, 20, 20 }, "Pause Music", & pause);
            if (pause)
            {
                PauseMusicStream(music);
                for (int i = 0; i < soundsCount; i++)
                {
                    StopSound(sounds[i]);
                }
            }
            else
            {
                ResumeMusicStream(music);
            }

            // Controls info
            GuiGroupBox((Rectangle) { 10, 540, 250, 140 }, "Controls");
            GuiLabel((Rectangle) { 20, 550, 200, 20 }, "Z - Game 2D mode");
            GuiLabel((Rectangle) { 20, 570, 200, 20 }, "X - Game 3D mode");
            GuiLabel((Rectangle) { 20, 590, 200, 20 }, "C - Editor mode");
            GuiLabel((Rectangle) { 20, 610, 200, 20 }, "1 - 4 - Change Biome");
            GuiLabel((Rectangle) { 20, 630, 200, 20 }, "SPACE - Draw path A*");
            GuiLabel((Rectangle) { 20, 650, 200, 20 }, "AWSD - Move player");

        } break;
        case 1:     // Game 3D mode
        {
            // Draw maze using cameraFP
            BeginMode3D(cameraFP);

            // DONE: Draw maze generated 3d model
            DrawModel(mdlMaze, mdlPosition, 1.0f, WHITE);                     // Draw maze map

            // EXTRA: Draw end cell as a cube
            for (int i = 0; i < endCellsCount; i++)
            {
                DrawCube((Vector3) { mdlPosition.x + endCells[i].x, 40.3f, mdlPosition.z + endCells[i].y }, 1.0f, 1.0f, 1.0f, RED);
            }

            // DONE: Maze items 3d draw (using 3d shape/model?) on required positions
            for (int i = 0; i < MAX_MAZE_ITEMS; i++)
            {
                if (!collectibleItems[i].collected)
                {
                    Color tintColor = collectibleItems[i].color;
                    float rotationSpeed = 100.0f;
                    float rotationAngle = GetTime() * rotationSpeed;

                    DrawModelEx(collectable3D, (Vector3) { mdlPosition.x + collectibleItems[i].position.x, mdlPosition.y + 0.3f, mdlPosition.z + collectibleItems[i].position.y }, (Vector3) { 0, 1, 0 }, rotationAngle, (Vector3) { 0.1f, 0.1f, 0.1f }, tintColor);
                }
            }

            EndMode3D();

            DrawTextureEx(texMaze, (Vector2) { GetScreenWidth() - texMaze.width * 4.0f - 20, 20.0f }, 0.0f, 4.0f, WHITE);
            DrawRectangleLines(GetScreenWidth() - texMaze.width * 4 - 20, 20, texMaze.width * 4, texMaze.height * 4, GREEN);

            // Draw player position radar
            DrawRectangle(GetScreenWidth() - texMaze.width * 4 - 20 + playerCell.x * 4, 20 + playerCell.y * 4, 4, 4, RED);

            // Draw pathfinding in radar
            if (drawPathAStar)
            {
                int pointCount = 0;
                Point* path = LoadPathAStar(imMaze, playerCell, endCells[0], &pointCount);
                for (int i = 1; i < pointCount - 1; i++) // i =1 to avoid drawing the player position and pointCount-1 to avoid drawing the end cell
                {
                    DrawRectangle(GetScreenWidth() - texMaze.width * 4 - 20 + path[i].x * 4, 20 + path[i].y * 4, 4, 4, YELLOW);
                }
                free(path);
            }

            // Draw Maze items radar
            for (int i = 0; i < MAX_MAZE_ITEMS; i++) {
                if (!collectibleItems[i].collected) {
                    DrawRectangle(GetScreenWidth() - texMaze.width * 4 - 20 + collectibleItems[i].position.x * 4, 20 + collectibleItems[i].position.y * 4, 4, 4, collectibleItems[i].color);
                }
            }

            // GUI

            // White rectangle for all the GUI ( game info and controls)
            DrawRectangle(0, 0, 270, 225, Fade(RAYWHITE, 0.9f));

            // Game info
            GuiGroupBox((Rectangle) { 10, 10, 200, 60 }, "Game Info");
            GuiLabel((Rectangle) { 20, 20, 200, 20 }, TextFormat("Score: %d", playerScore));
            GuiLabel((Rectangle) { 20, 40, 200, 20 }, TextFormat("Time Remaining: %02d:%02d", remainingTime / 60000, (remainingTime % 60000) / 1000));

            // Controls info
            GuiGroupBox((Rectangle) { 10, 80, 250, 140 }, "Controls");
            GuiLabel((Rectangle) { 20, 90, 200, 20 }, "Z - Game 2D mode");
            GuiLabel((Rectangle) { 20, 110, 200, 20 }, "X - Game 3D mode");
            GuiLabel((Rectangle) { 20, 130, 200, 20 }, "C - Editor mode");
            GuiLabel((Rectangle) { 20, 150, 200, 20 }, "1 - 4 - Change Biome");
            GuiLabel((Rectangle) { 20, 170, 200, 20 }, "SPACE - Draw path A*");
            GuiLabel((Rectangle) { 20, 190, 200, 20 }, "AWSD - Move player");

        } break;
        case 2:     // Editor mode
        {
            // Draw maze
            DrawTextureEx(texMaze, (Vector2) { GetScreenWidth() / 2 - texMaze.width * MAZE_DRAW_SCALE / 2, GetScreenHeight() / 2 - texMaze.height * MAZE_DRAW_SCALE / 2 }, 0.0f, MAZE_DRAW_SCALE, WHITE);
            DrawRectangleLines(GetScreenWidth() / 2 - texMaze.width * MAZE_DRAW_SCALE / 2, GetScreenHeight() / 2 - texMaze.height * MAZE_DRAW_SCALE / 2, MAZE_WIDTH * MAZE_DRAW_SCALE, MAZE_HEIGHT * MAZE_DRAW_SCALE, RED);

            // DONE: Draw maze items 2d (using sprite texture?)
            for (int i = 0; i < MAX_MAZE_ITEMS; i++) {
                if (!collectibleItems[i].collected) {
                    DrawRectangle(mdlPosition.x + collectibleItems[i].position.x * MAZE_DRAW_SCALE, mdlPosition.y + collectibleItems[i].position.y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, collectibleItems[i].color);
                }
            }

            // Draw pathfinding result, shorter path from start to end
            if (drawPathAStar)
            {
                int pointCount = 0;
                Point* path = LoadPathAStar(imMaze, startCells[0], endCells[3], &pointCount);
                for (int i = 0; i < pointCount; i++)
                {
                    DrawRectangle(mdlPosition.x + path[i].x * MAZE_DRAW_SCALE, mdlPosition.y + path[i].y * MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, MAZE_DRAW_SCALE, YELLOW);
                }
                free(path);
            }

            //GUI
            // Editor info
            GuiGroupBox((Rectangle) { 10, 80, 250, 340 }, "Map Configuration");
            GuiLabel((Rectangle) { 20, 90, 200, 20 }, "Row Spacing");
            GuiLabel((Rectangle) { 100, 90, 200, 20 }, TextFormat("Current Value: %d", mapRowSpacing));
            mapRowSpacing = GuiButton((Rectangle) { 20, 110, 200, 20 }, "Increase") ? mapRowSpacing + 1 : mapRowSpacing;
            //decrease button with limit to 1
            if (mapRowSpacing > 1)
            {
                mapRowSpacing = GuiButton((Rectangle) { 20, 140, 200, 20 }, "Decrease") ? mapRowSpacing - 1 : mapRowSpacing;
            }

            GuiLabel((Rectangle) { 20, 170, 200, 20 }, "Column Spacing");
            mapColumnSpacing = GuiButton((Rectangle) { 20, 200, 200, 20 }, "Increase") ? mapColumnSpacing + 1 : mapColumnSpacing;
            if (mapColumnSpacing > 1)
            {
                mapColumnSpacing = GuiButton((Rectangle) { 20, 230, 200, 20 }, "Decrease") ? mapColumnSpacing - 1 : mapColumnSpacing;
            }
            GuiLabel((Rectangle) { 100, 170, 200, 20 }, TextFormat("Current Value: %d", mapColumnSpacing));

            GuiLabel((Rectangle) { 20, 260, 200, 20 }, "Skip Chance");
            GuiSliderBar((Rectangle) { 20, 280, 200, 20 }, NULL, TextFormat("%.02f", mapPointsSkipChance), & mapPointsSkipChance, 0.0f, 1.0f);

            //seed button (moved down)
            GuiLabel((Rectangle) { 20, 310, 200, 20 }, TextFormat("Current Seed: %d", mapRandomSeed));
            if (GuiButton((Rectangle) { 20, 330, 200, 20 }, "Random Seed"))
            {
                mapRandomSeed = GetRandomValue(0, 100000);
            }

            // Regenerate maze button (moved down)
            if (GuiButton((Rectangle) { 20, 360, 200, 50 }, "Regenerate Maze"))
            {
                imMaze = GenImageMaze(MAZE_WIDTH, MAZE_HEIGHT, mapRowSpacing, mapColumnSpacing, mapPointsSkipChance);
                UpdateTexture(texMaze, imMaze.data);
                UnloadImageColors(mapPixels);
                mapPixels = LoadImageColors(imMaze);

                // Update 3d model
                UnloadMesh(meshMaze);
                meshMaze = GenMeshCubicmap(imMaze, (Vector3) { 1.0f, 1.0f, 1.0f });

                // Restart game
                remainingTime = TIME_LIMIT_SECONDS;
                playerCell = startCells[0];

                // Set collectible items
                SetCollectibleItems(imMaze, startCells, endCells, collectibleItems);
            }

            // Audio control
            GuiGroupBox((Rectangle) { 10, 430, 250, 100 }, "Audio Control");
            GuiLabel((Rectangle) { 20, 440, 200, 20 }, "Music Volume");
            GuiSliderBar((Rectangle) { 20, 460, 200, 20 }, NULL, TextFormat("%.02f", masterVolume), & masterVolume, 0.0f, 1.0f);
            SetMasterVolume(masterVolume);

            GuiCheckBox((Rectangle) { 20, 490, 20, 20 }, "Pause Music", & pause);
            if (pause)
            {
                PauseMusicStream(music);
                for (int i = 0; i < soundsCount; i++)
                {
                    StopSound(sounds[i]);
                }
            }
            else
            {
                ResumeMusicStream(music);
            }

            // Controls info
            GuiGroupBox((Rectangle) { 10, 540, 250, 175 }, "Controls");
            GuiLabel((Rectangle) { 20, 550, 200, 20 }, "Z - Game 2D mode");
            GuiLabel((Rectangle) { 20, 570, 200, 20 }, "X - Game 3D mode");
            GuiLabel((Rectangle) { 20, 590, 200, 20 }, "C - Editor mode");
            GuiLabel((Rectangle) { 20, 610, 200, 20 }, "1 - 4 - Change Biome");
            GuiLabel((Rectangle) { 20, 630, 200, 20 }, "SPACE - Draw path A*");
            GuiLabel((Rectangle) { 20, 650, 200, 20 }, "Mouse Left - Draw Wall");
            GuiLabel((Rectangle) { 20, 670, 200, 20 }, "Mouse Right - Erase Wall");
            GuiLabel((Rectangle) { 20, 690, 200, 20 }, "Mouse Middle - Add Item");

        } break;
        default: break;
        }

        EndDrawing();
        //----------------------------------------------------------------------------------
        }

        // De-Initialization
        //--------------------------------------------------------------------------------------
        // 
        // DONE: Unload all other resources (textures, sprites, music, sound...)
        UnloadTexture(texMaze);     // Unload maze texture from VRAM (GPU)
        UnloadImage(imMaze);        // Unload maze image from RAM (CPU)
        UnloadModel(collectable3D);
        UnloadImageColors(mapPixels);

        for (int i = 0; i < texBiomesCount; i++)     
        {
            UnloadTexture(texBiomes[i]);
        }

        UnloadMusicStream(music);
        UnloadSound(fxWin);
        UnloadSound(fxCoin);
        UnloadSound(fxDie);
        CloseAudioDevice();

        CloseWindow();              // Close window and OpenGL context
        //--------------------------------------------------------------------------------------

        return 0;
}

// Generate procedural maze image, using grid-based algorithm
// NOTE: Black=Walkable cell, White=Wall/Block cell
static Image GenImageMaze(int width, int height, int spacingRows, int spacingCols, float skipChance)
{
    // Generate image of plain color (BLACK)
    Image imMaze = GenImageColor(width, height, BLACK);

    // Allocate an array of point used for maze generation
    // NOTE: Dynamic array allocation, memory allocated in HEAP (MAX: Available RAM)
    Point* mazePoints = (Point*)malloc(MAZE_WIDTH * MAZE_HEIGHT * sizeof(Point));
    int mazePointsCounter = 0;

    // Start traversing image data, line by line, to paint our maze
    for (int y = 0; y < imMaze.height; y++)
    {
        for (int x = 0; x < imMaze.width; x++)
        {
            // Check image borders (1 px)
            if ((x == 0) || (x == (imMaze.width - 1)) || (y == 0) || (y == (imMaze.height - 1)))
            {
                ImageDrawPixel(&imMaze, x, y, WHITE);   // Image border pixels set to WHITE
            }
            else
            {
                // Check pixel module to set maze corridors width and height
                if ((x % spacingRows == 0) && (y % spacingCols == 0))
                {
                    // Get change to define a point for further processing
                    float chance = (float)GetRandomValue(0, 100) / 100.0f;

                    if (chance >= skipChance)
                    {
                        // Set point as wall...
                        ImageDrawPixel(&imMaze, x, y, WHITE);

                        // ...save point for further processing
                        mazePoints[mazePointsCounter] = (Point){ x, y };
                        mazePointsCounter++;
                    }
                }
            }
        }
    }

    // Define an array of 4 directions for convenience
    Point directions[4] = {
        { 0, -1 },      // Up
        { 0, 1 },       // Down
        { -1, 0 },      // Left
        { 1, 0 },       // Right
    };

    // Load a random sequence of points, to be used as indices, so,
    // we can access mazePoints[] randomly indexed, instead of following the order we gor them
    int* pointIndices = LoadRandomSequence(mazePointsCounter, 0, mazePointsCounter - 1);

    // Process every random maze point, moving in one random direction,
    // until we collision with another wall (WHITE pixel)
    for (int i = 0; i < mazePointsCounter; i++)
    {
        Point currentPoint = mazePoints[pointIndices[i]];
        Point currentDir = directions[GetRandomValue(0, 3)];
        currentPoint.x += currentDir.x;
        currentPoint.y += currentDir.y;

        // Keep incrementing wall in selected direction until a WHITE pixel is found
        // NOTE: We only check against the color.r component
        while (GetImageColor(imMaze, currentPoint.x, currentPoint.y).r != 255)
        {
            ImageDrawPixel(&imMaze, currentPoint.x, currentPoint.y, WHITE);

            currentPoint.x += currentDir.x;
            currentPoint.y += currentDir.y;
        }
    }

    UnloadRandomSequence(pointIndices);

    return imMaze;
}


static void SetCollectibleItems(Image imMaze, Point startCells[], Point endCells[], CollectibleItem collectibleItems[])
{
    for (int i = 0; i < MAX_MAZE_ITEMS; i++)
    {
        collectibleItems[i].position = (Point){ GetRandomValue(1, imMaze.width - 2), GetRandomValue(1, imMaze.height - 2) };
        collectibleItems[i].collected = false;
        collectibleItems[i].value = GetRandomValue(1, 3);

        // Set color based on value
        switch (collectibleItems[i].value) {
        case 1: collectibleItems[i].color = ORANGE; break;
        case 2: collectibleItems[i].color = RAYWHITE; break;
        case 3: collectibleItems[i].color = PURPLE; break;
        default: break;
        }

        // Check if collectible item is placed on start or end cells and if so, regenerate
        for (int j = 0; j < 4; j++)
        {
            if ((collectibleItems[i].position.x == startCells[j].x && collectibleItems[i].position.y == startCells[j].y) ||
                (collectibleItems[i].position.x == endCells[j].x && collectibleItems[i].position.y == endCells[j].y))
            {
                collectibleItems[i].position = (Point){ GetRandomValue(1, imMaze.width - 2), GetRandomValue(1, imMaze.height - 2) };
                j = 0;
            }
        }

        // Check if collectible item is placed on a wall and if so, regenerate
        while (GetImageColor(imMaze, collectibleItems[i].position.x, collectibleItems[i].position.y).r == 255)
        {
            collectibleItems[i].position = (Point){ GetRandomValue(1, imMaze.width - 2), GetRandomValue(1, imMaze.height - 2) };
        }
    }
}

// DONE: EXTRA: [10p] Get shorter path between two points, implements pathfinding algorithm: A*
// NOTE: The functions returns an array of points and the pointCount

typedef struct PathNode
{
    Point position;
    int gValue;         // Cost from the start node to this node
    int hValue;         // Heuristic estimate of the cost from this node to the end node
    struct PathNode* parent;
} PathNode;

typedef struct OpenList
{
    PathNode* node;
    struct OpenList* next; // Pointer to the next node in the list
} OpenList;

typedef struct ClosedList
{
    Point position;
    struct ClosedList* next; // Pointer to the next node in the list
} ClosedList;

int ManhattanDistance(Point a, Point b)
{
    return abs(a.x - b.x) + abs(a.y - b.y);
}

bool IsAccessible(Image map, Point p, ClosedList* closedList)
{
    // Check if the point is within bounds
    if (p.x < 0 || p.y < 0 || p.x >= map.width || p.y >= map.height) return false;

    // Check if the point is walkable
    if (GetImageColor(map, p.x, p.y).r != 0) return false;

    // Check if the point is in the closed list
    ClosedList* current = closedList;

    while (current != NULL)
    {
        if (current->position.x == p.x && current->position.y == p.y) return false;
        current = current->next;
    }

    return true;
}

// Find the node with the lowest f value in the open list
PathNode* FindLowestFInOpenList(OpenList* openList)
{
    PathNode* lowestFNode = NULL;
    int lowestF = INT_MAX;
    OpenList* current = openList;

    while (current != NULL)
    {
        if ((current->node->gValue + current->node->hValue) < lowestF)
        {
            lowestF = current->node->gValue + current->node->hValue;
            lowestFNode = current->node;
        }
        current = current->next;
    }
    return lowestFNode;
}

// Remove a node from the open list
void RemoveFromOpenList(OpenList** openList, PathNode* nodeToRemove)
{
    OpenList* current = *openList;
    OpenList* prev = NULL;

    while (current != NULL)
    {
        if (current->node == nodeToRemove)
        {
            if (prev == NULL) {
                *openList = current->next;
            }
            else
            {
                prev->next = current->next;
            }
            free(current);
            break;
        }
        prev = current;
        current = current->next;
    }
}

static Point* LoadPathAStar(Image map, Point start, Point end, int* pointCount) {
    Point* path = NULL;
    int pathCounter = 0;

    // Initialize open and closed lists
    OpenList* openList = NULL;
    ClosedList* closedList = NULL;

    // Create start node
    PathNode* startNode = (PathNode*)malloc(sizeof(PathNode));
    startNode->position = start;
    startNode->gValue = 0;
    startNode->hValue = ManhattanDistance(start, end);
    startNode->parent = NULL;

    // Add start node to open list
    OpenList* startNodeItem = (OpenList*)malloc(sizeof(OpenList));
    startNodeItem->node = startNode;
    startNodeItem->next = NULL;
    openList = startNodeItem;

    // Loop until the open list is empty
    while (openList != NULL)
    {
        // Find the node with the lowest f value in the open list
        PathNode* currentNode = FindLowestFInOpenList(openList);

        //Remove current node from open list
        RemoveFromOpenList(&openList, currentNode);

        // Add current node to closed list
        ClosedList* newClosedListNode = (ClosedList*)malloc(sizeof(ClosedList));
        newClosedListNode->position = currentNode->position;
        newClosedListNode->next = closedList;
        closedList = newClosedListNode;

        // Check if the current node is the end node
        if (currentNode->position.x == end.x && currentNode->position.y == end.y)
        {
            PathNode* currenbNode = currentNode;
            while (currenbNode != NULL)
            {
                pathCounter++;
                path = (Point*)realloc(path, pathCounter * sizeof(Point));
                path[pathCounter - 1] = currenbNode->position;
                currenbNode = currenbNode->parent;
            }
            break;
        }

        //Generate Successors of current Node
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if ((dx == 0 && dy == 0) || (dx != 0 && dy != 0))
                {
                    continue;
                }

                // Create successor node and check if it is accessible
                Point successorPosition = { currentNode->position.x + dx, currentNode->position.y + dy };
                if (!IsAccessible(map, successorPosition, closedList))
                {
                    continue;
                }

                // Calculate tentative g value for the successor node
                int tentatinveG = currentNode->gValue + 1;
                bool found = false;
                OpenList* openListPtr = openList;

                // Check if the successor node is in the open list
                while (openListPtr != NULL)
                {
                    if (openListPtr->node->position.x == successorPosition.x && openListPtr->node->position.y == successorPosition.y)
                    {
                        found = true; // Successor node is in the open list
                        break;
                    }
                    openListPtr = openListPtr->next;
                }

                if (!found) // Successor node is not in the open list
                {
                    // Add successor node to open list
                    PathNode* successorNode = (PathNode*)malloc(sizeof(PathNode));
                    successorNode->position = successorPosition;
                    successorNode->gValue = tentatinveG;
                    successorNode->hValue = ManhattanDistance(successorPosition, end);
                    successorNode->parent = currentNode;
                    OpenList* newSuccessorNodeOpenList = (OpenList*)malloc(sizeof(OpenList));
                    newSuccessorNodeOpenList->node = successorNode;
                    newSuccessorNodeOpenList->next = openList;
                    openList = newSuccessorNodeOpenList;
                }
            }
        }
    }

    // Free memory
    while (openList != NULL)
    {
        OpenList* temp = openList;
        openList = openList->next;
        free(temp);
    }

    while (closedList != NULL)
    {
        ClosedList* temp = closedList;
        closedList = closedList->next;
        free(temp);
    }

    *pointCount = pathCounter;

    return path;
}