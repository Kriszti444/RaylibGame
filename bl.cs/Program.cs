using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;


namespace bl.cs;

enum GameState {
    Menu,
    Info,
    Game,
    End
}

class Program
{

    static Rectangle btnLetrehozas(int x, int y, int w, int h, string szoveg)
    {
        Rectangle btn = new Rectangle(x, y, w, h);

        int fontsize = h / 2;
        int textwidth = MeasureText(szoveg, fontsize);

        int textX = (int)(btn.X + btn.Width / 2 - textwidth / 2);
        int textY = (int)(btn.Y + btn.Height / 2 - fontsize / 2);

        DrawRectangleRec(btn, Color.Gray);
        DrawText(szoveg, textX, textY, fontsize, Color.White);

        return btn;
    }

    static void Main(string[] args)
    {
        Raylib.InitWindow(1920, 1080, "Raylib Inside Existing Project");

        // kinezetek   
        Texture2D medium_cat = LoadTexture(@"/home/kriszti/Desktop/Code/C#/First Test/kinezetek and stuff/medium_cat.png");
        Texture2D medium_cat_left = LoadTexture(@"/home/kriszti/Desktop/Code/C#/First Test/kinezetek and stuff/medium_cat_left.png");
        Texture2D medium_cat_right = LoadTexture(@"/home/kriszti/Desktop/Code/C#/First Test/kinezetek and stuff/medium_cat_right.png");
        Texture2D medium_cat_back = LoadTexture(@"/home/kriszti/Desktop/Code/C#/First Test/kinezetek and stuff/medium_cat_back.png");

        // karakter pozicioja es sebessege
        Vector2 playerpos = new Vector2(100, 200);
        float speed = 0.05f;

        Rectangle itemRect = new Rectangle(800, 450, 50, 50);
        bool itemPicked = false;
        bool hasItem = false;

        GameState CurrentState = GameState.Menu;
        while (!Raylib.WindowShouldClose())
        {
            Vector2 mouse = GetMousePosition();
            bool walking = false;
            

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
           
           if (CurrentState == GameState.Menu)
            {
                Rectangle btnPlay = btnLetrehozas(50, 740, 550, 150, "Play");
                Rectangle btnInfo = btnLetrehozas(50, 920, 550, 150, "Info");

                bool mouseOnPlay = CheckCollisionPointRec(mouse, btnPlay);
                bool mouseOnInfo = CheckCollisionPointRec(mouse, btnInfo);

                if (mouseOnPlay && IsMouseButtonPressed(MouseButton.Left))
                {
                    CurrentState = GameState.Game;
                }
                if (mouseOnInfo && IsMouseButtonPressed(MouseButton.Left))
                {
                    CurrentState = GameState.Info;
                }
            }

            else if (CurrentState == GameState.Info)
            {
                Rectangle btnBack = btnLetrehozas(1500, 980, 400, 80, "Back");

                bool mouseOnBack = CheckCollisionPointRec(mouse, btnBack);

                if (mouseOnBack && IsMouseButtonDown(MouseButton.Left))
                {
                    CurrentState = GameState.Menu;
                }
            }

            else if (CurrentState == GameState.Game)
            {

                // karakter mozgasa

                if (IsKeyDown(KeyboardKey.W))
                {
                    playerpos.Y -= speed;
                    walking = true;
                } 
                if (IsKeyDown(KeyboardKey.A)) {
                    playerpos.X -= speed;
                    walking = true;
                }
                if (IsKeyDown(KeyboardKey.S)) {
                    playerpos.Y += speed;
                    walking = true;
                }
                if (IsKeyDown(KeyboardKey.D)) {
                    playerpos.X += speed;
                    walking = true;
                }
                

                if (walking)
                {
                    if (IsKeyDown(KeyboardKey.W))
                    {
                        DrawTexture(medium_cat_back, (int)playerpos.X, (int)playerpos.Y, Color.White);
                    } 
                    else if (IsKeyDown(KeyboardKey.A)) {
                        DrawTexture(medium_cat_left, (int)playerpos.X, (int)playerpos.Y, Color.White);
                    }
                    else if (IsKeyDown(KeyboardKey.S)) {
                        DrawTexture(medium_cat, (int)playerpos.X, (int)playerpos.Y, Color.White);
                    }
                    else if (IsKeyDown(KeyboardKey.D)) {
                        DrawTexture(medium_cat_right, (int)playerpos.X, (int)playerpos.Y, Color.White);
                    } 
                } else {
                    DrawTexture(medium_cat, (int)playerpos.X, (int)playerpos.Y, Color.White);
                }
                    

                // dolgokkal valo interaktálás

                Rectangle playerRec = new Rectangle(playerpos.X, playerpos.Y, medium_cat.Width, medium_cat.Height);
                Rectangle interactZone = new Rectangle(playerRec.X - 12, playerRec.Y, medium_cat.Width, medium_cat.Height);
                    
                if (!itemPicked)
                {
                    DrawRectangle();
                }

            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
