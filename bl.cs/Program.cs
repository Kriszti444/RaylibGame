using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;


namespace bl.cs;

enum GameState {
    Menu,
    Info,
    Gamelvl1,
    Gamelvl2,
    Gamelvl3,
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

        Rectangle itemRect1 = new Rectangle(500, 500, 50, 50);
        bool itemPicked1 = false;
        bool hasItem1 = false;

        Rectangle itemRect2 = new Rectangle(600, 700, 50, 50);
        bool itemPicked2 = false;
        bool hasItem2 = false;

        Rectangle itemRect3 = new Rectangle(900, 200, 50, 50);
        bool itemPicked3 = false;
        bool hasItem3 = false;

        bool haseverything = false;

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
                    CurrentState = GameState.Gamelvl1;
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

            else if (CurrentState == GameState.Gamelvl1)
            {
                DrawText("lvl 1", 960, 200, 200, Color.Black);

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
                Rectangle interactZone = new Rectangle(playerRec.X - 12, playerRec.Y -12, medium_cat.Width +24, medium_cat.Height+24);
                    
                if (!itemPicked1)
                {
                    DrawRectangle(500, 500, 50, 50, Color.Red);
                }

                bool nearItem = !itemPicked1 && CheckCollisionRecs(interactZone, itemRect1);

                if (nearItem)
                {
                    DrawText("Press E", 50, 60, 30, Color.Black);

                    if (IsKeyPressed(KeyboardKey.E))
                    {
                        itemPicked1 = true;
                        hasItem1 = true;
                    }
                }

                if (hasItem2)
                {
                    DrawText("You picked up the item!", 70, 70, 20, Color.Black);
                }

                //                    
                if (!itemPicked2)
                {
                    DrawRectangle(600, 700, 50, 50, Color.Red);
                }

                bool nearItem2 = !itemPicked2 && CheckCollisionRecs(interactZone, itemRect2);

                if (nearItem2)
                {
                    DrawText("Press E", 50, 60, 30, Color.Black);

                    if (IsKeyPressed(KeyboardKey.E))
                    {
                        itemPicked2 = true;
                        hasItem2 = true;
                    }
                }

                if (hasItem2)
                {
                    DrawText("You picked up the item1!", 70, 90, 20, Color.Black);
                }

                //                   
                if (!itemPicked3)
                {
                    DrawRectangle(900, 200, 50, 50, Color.Red);
                }

                bool nearItem3 = !itemPicked3 && CheckCollisionRecs(interactZone, itemRect3);

                if (nearItem3)
                {
                    DrawText("Press E", 50, 60, 30, Color.Black);

                    if (IsKeyPressed(KeyboardKey.E))
                    {
                        itemPicked3 = true;
                        hasItem3 = true;
                    }
                }

                if (hasItem3)
                {
                    DrawText("You picked up the item!", 70, 110, 20, Color.Black);
                }

                if (hasItem1 && hasItem2 && hasItem3)
                {
                    haseverything = true;

                    // a "haseverything arra kell, hogy ha majd meg lesz a palya, csak akkor tudjon a következore menni a jatekor,
                    // ha meg van minen olyan targy amit fel kellett vennie."
                    CurrentState = GameState.Gamelvl2;
                }

            } else if (CurrentState == GameState.Gamelvl2)
            {
                DrawText("lvl 2", 960, 200, 200, Color.Black);

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
                    
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
