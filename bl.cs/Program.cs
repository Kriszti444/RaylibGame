using System.Numerics;
using System.Collections.Generic;
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

    static void walk(ref float x, ref float y, float speed, bool walking, Texture2D back, Texture2D left, Texture2D right, Texture2D facing)
    {
        if (IsKeyDown(KeyboardKey.LeftShift))
        {
            speed += 0.06f;
        }
        if (IsKeyDown(KeyboardKey.W))
        {
            y -= speed;
            walking = true;
        }
        if (IsKeyDown(KeyboardKey.A))
        {
            x -= speed;
            walking = true;
        }
        if (IsKeyDown(KeyboardKey.S))
        {
            y += speed;
            walking = true;
        }
        if (IsKeyDown(KeyboardKey.D))
        {
            x += speed;
            walking = true;
        }

        if (walking)
        {
            if (IsKeyDown(KeyboardKey.W))
            {
                DrawTexture(back, (int)x, (int)y, Color.White);
            }
            else if (IsKeyDown(KeyboardKey.A))
            {
                DrawTexture(left, (int)x, (int)y, Color.White);
            }
            else if (IsKeyDown(KeyboardKey.S))
            {
                DrawTexture(facing, (int)x, (int)y, Color.White);
            }
            else if (IsKeyDown(KeyboardKey.D))
            {
                DrawTexture(right, (int)x, (int)y, Color.White);
            }
        }
        else
        {
            DrawTexture(facing, (int)x, (int)y, Color.White);
        }
    }

    static void itemLetrehozas(string itemname, ref bool hasitem, ref bool itempicked, Rectangle interactZone, Rectangle itemrect, ref float messageTime,
        float messageSeconds)
    {
        if (!itempicked) {
            DrawRectangleRec(itemrect, Color.Red);
        }

        bool nearItem2 = !itempicked && CheckCollisionRecs(interactZone, itemrect);

        if (nearItem2)
        {
            DrawText("Press E", (int)itemrect.X, (int)itemrect.Y - 20, 30, Color.White);
            if (IsKeyPressed(KeyboardKey.E))
            {
                itempicked = true;
                hasitem = true;
                messageTime = messageSeconds;
            }
        }

        float dt = GetFrameTime();

        if (messageTime > 0f)
                {
                    messageTime -= dt;
                }

                if (messageTime > 0f)
                {
                    if (hasitem) DrawText($"You picked up item {itemname}!", (int)itemrect.X, (int)itemrect.Y - 30, 20, Color.White);
                }
    }

    static void Main(string[] args)
    {
        Raylib.InitWindow(1920, 1080, "Raylib Inside Existing Project");

        // kinezetek
        Texture2D medium_cat = LoadTexture("kinezetek_and_stuff/medium_cat.png");
        Texture2D medium_cat_left = LoadTexture("kinezetek_and_stuff/medium_cat_left.png");
        Texture2D medium_cat_right = LoadTexture("kinezetek_and_stuff/medium_cat_right.png");
        Texture2D medium_cat_back = LoadTexture("kinezetek_and_stuff/medium_cat_back.png");

        Texture2D lvl1Background = LoadTexture("kinezetek_and_stuff/padlas.png");

        Texture2D drawerImg = LoadTexture("kinezetek_and_stuff/small_cat.png");

        // karakter pozicioja es sebessege
        Vector2 playerpos = new Vector2(1200, 170);

        // IMPORTANT: speed should be pixels/second (not 0.1f)
        float speed = 250f;

        // items
        List<int> picked = new List<int>();

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

        bool drawerShow = false;
        bool drawerImgShow = false;

        // shoot
        List<Vector2> bulletPositions = new List<Vector2>();
        List<Vector2> bulletVelocities = new List<Vector2>();
        List<float> bulletRemaining = new List<float>();

        float bulletSpeed = 900f;

        // enemies
        List<Vector2> enemyPositions = new List<Vector2>();
        List<float> enemyRadii = new List<float>();
        List<int> enemyHP = new List<int>();

        enemyPositions.Add(new Vector2(1200, 300));
        enemyRadii.Add(18f);
        enemyHP.Add(2);

        enemyPositions.Add(new Vector2(1600, 800));
        enemyRadii.Add(22f);
        enemyHP.Add(3);

        enemyPositions.Add(new Vector2(800, 900));
        enemyRadii.Add(16f);
        enemyHP.Add(1);

        // message time
        float messageTime = 0f;

        // menu
        bool pauseMenuOpen = false;

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
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    pauseMenuOpen = !pauseMenuOpen;
                }

                Rectangle pauseButton = btnLetrehozas(14, 12, 70, 61, "Menu");

                //Rectangle pauseButton = new Rectangle(20, 20, 120, 50);
                //DrawRectangleRec(pauseButton, Color.DarkGray);
                //DrawText("Menu", 35, 32, 25, Color.White);

                if (CheckCollisionPointRec(mouse, pauseButton) && IsMouseButtonPressed(MouseButton.Left))
                    pauseMenuOpen = true;

                if (!pauseMenuOpen)
                {
                DrawTexture(lvl1Background, 0, 0, Color.White); 

                float dt = Raylib.GetFrameTime();

                // --- map szelei ---
                Rectangle roomBounds = new Rectangle(100, 103, 1714, 868);

                // --- dolgok ---
                List<Rectangle> dolgok = new List<Rectangle>()
                {
                    new Rectangle(1342, 125, 182, 96),  // kis dobozs
                    new Rectangle(1536, 123, 264, 158),  // nagy doboz
                    new Rectangle(100, 103, 360, 400),  // lepcso
                    new Rectangle(460, 125, 336, 138),  // szekreny a lepcso mellett
                    new Rectangle(1433, 842, 370, 105),  // szonyeg
                    new Rectangle(872, 814, 416, 132),  // szonyeg
                };                

                // --- hitbox  ---
                float feetOffsetX = 18f;
                float feetOffsetY = 42f;
                float feetW = medium_cat.Width - 36f;
                float feetH = medium_cat.Height - 44f;

                Rectangle playerRec = new Rectangle(
                    playerpos.X + feetOffsetX,
                    playerpos.Y + feetOffsetY,
                    feetW,
                    feetH
                );

                float moveX = 0, moveY = 0;
                if (IsKeyDown(KeyboardKey.A)) moveX -= 1;
                if (IsKeyDown(KeyboardKey.D)) moveX += 1;
                if (IsKeyDown(KeyboardKey.W)) moveY -= 1;
                if (IsKeyDown(KeyboardKey.S)) moveY += 1;

                float actualSpeed = speed;
                if (IsKeyDown(KeyboardKey.LeftShift)) actualSpeed *= 1.6f;

                if (moveX != 0 && moveY != 0)
                {
                    float inv = 1.0f / MathF.Sqrt(2);
                    moveX *= inv;
                    moveY *= inv;
                }

                float dx = moveX * actualSpeed * dt;
                float dy = moveY * actualSpeed * dt;

                Rectangle p = playerRec;

                p.X += dx;
                foreach (var s in dolgok)
                {
                    if (CheckCollisionRecs(p, s))
                    {
                        if (dx > 0) p.X = s.X - p.Width;
                        else if (dx < 0) p.X = s.X + s.Width;
                    }
                }

                p.Y += dy;
                foreach (var s in dolgok)
                {
                    if (CheckCollisionRecs(p, s))
                    {
                        if (dy > 0) p.Y = s.Y - p.Height;
                        else if (dy < 0) p.Y = s.Y + s.Height;
                    }
                }

                p.X = MathF.Max(roomBounds.X, MathF.Min(p.X, roomBounds.X + roomBounds.Width - p.Width));
                p.Y = MathF.Max(roomBounds.Y, MathF.Min(p.Y, roomBounds.Y + roomBounds.Height - p.Height));

                playerpos.X = p.X - feetOffsetX;
                playerpos.Y = p.Y - feetOffsetY;

                walk(ref playerpos.X, ref playerpos.Y, 0f, walking, medium_cat_back, medium_cat_left, medium_cat_right, medium_cat);

                Rectangle interactZone = new Rectangle(p.X - 12, p.Y - 12, p.Width + 24, p.Height + 24);

                //*-------------------------------------------------//
                // static void itemLetrehozas(string itemname, bool hasitem, bool itempicked, Rectangle interactZone, Rectangle itemrect)

                itemLetrehozas("Picture", ref hasItem1, ref itemPicked1, interactZone, itemRect1, ref messageTime, 2f);
                itemLetrehozas("Drawing", ref hasItem2, ref itemPicked2, interactZone, itemRect2, ref messageTime, 2f);
                itemLetrehozas("Key", ref hasItem3, ref itemPicked3, interactZone, itemRect3, ref messageTime, 2f);

                //*-------------------------------------------------//
                
                // shoot 
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Vector2 start = new Vector2(playerpos.X + medium_cat.Width / 2f, playerpos.Y + medium_cat.Height / 2f);
                    Vector2 target = mouse;

                    Vector2 dir = target - start;
                    float dist = dir.Length();

                    if (dist > 1f)
                    {
                        dir /= dist;

                        bulletPositions.Add(start);
                        bulletVelocities.Add(dir * bulletSpeed);
                        bulletRemaining.Add(dist);
                    }
                }

                for (int i = bulletPositions.Count - 1; i >= 0; i--)
                {
                    Vector2 step = bulletVelocities[i] * dt;
                    float stepLen = step.Length();

                    bulletPositions[i] += step;
                    bulletRemaining[i] -= stepLen;

                    if (bulletRemaining[i] <= 0f)
                    {
                        bulletPositions.RemoveAt(i);
                        bulletVelocities.RemoveAt(i);
                        bulletRemaining.RemoveAt(i);
                    }
                }

                for (int i = 0; i < bulletPositions.Count; i++)
                {
                    Raylib.DrawCircleV(bulletPositions[i], 5f, Color.LightGray);
                }

                // szekreny interaktalas

                Rectangle drawerRect = new Rectangle(460, 125, 336, 138);

                bool nearDrawer = CheckCollisionRecs(interactZone, drawerRect);

                if (nearDrawer && !drawerShow)
                {
                    DrawText("Press E", (int)drawerRect.X, (int)drawerRect.Y - 30, 20, Color.White);

                    if (IsKeyPressed(KeyboardKey.E))
                    {
                        drawerShow = true;
                    }
                }

                if (drawerShow)
                {
                    DrawRectangle(0, 0, 1920, 1080, new Color(0, 0, 0, 200)); // dark overlay

                    Rectangle itemRec = new Rectangle(1920 / 2 - drawerImg.Width / 2, 1080 / 2 - drawerImg.Height / 2, drawerImg.Width, drawerImg.Height);

                    // bool mouseOnItem = CheckCollisionPointRec(mouse, itemRec);

                    if (!itemPicked1)
                    {
                        DrawTexture(drawerImg, 1920 / 2 - drawerImg.Width / 2, 1080 / 2 - drawerImg.Height / 2, Color.White);
                        
                        if (CheckCollisionPointRec(mouse, itemRec) && IsMouseButtonPressed(MouseButton.Left)) {
                            hasItem1 = true;
                            itemPicked1 = true;

                            drawerImgShow = false; 
                        }
                    } 

                    DrawText("Press X to close", 800, 900, 30, Color.White);

                    if (IsKeyPressed(KeyboardKey.X))
                    {
                        drawerShow = false;
                    }
                }

                // szinatmenet
                DrawRectangleGradientH(0, 0, 1920, 1080, new Color(0, 0, 0, 210), new Color(0, 0, 0, 100));
                //

                // tovabb lepes
                if (hasItem1 && hasItem2 && hasItem3)
                {
                    haseverything = true;
                    CurrentState = GameState.Gamelvl2;
                }
                }
                else
                {
                DrawTexture(lvl1Background, 0, 0, Color.White); 
                DrawTexture(medium_cat, (int)playerpos.X, (int)playerpos.Y, Color.White); 
                DrawRectangleGradientH(0, 0, 1920, 1080, new Color(0, 0, 0, 210), new Color(0, 0, 0, 100));

                DrawRectangle(0, 0, 1920, 1080, new Color(0, 0, 0, 180));

                DrawText("PAUSED", 860, 300, 60, Color.White);

                Rectangle resumeBtn = new Rectangle(760, 450, 400, 80);
                Rectangle menuBtn = new Rectangle(760, 570, 400, 80);

                DrawRectangleRec(resumeBtn, Color.Gray);
                DrawText("Resume", 900, 475, 30, Color.White);

                DrawRectangleRec(menuBtn, Color.Gray);
                DrawText("Main Menu", 860, 595, 30, Color.White);

                if (CheckCollisionPointRec(mouse, resumeBtn) && IsMouseButtonPressed(MouseButton.Left))
                {
                    pauseMenuOpen = false;
                }

                if (CheckCollisionPointRec(mouse, menuBtn) && IsMouseButtonPressed(MouseButton.Left))
                {
                    pauseMenuOpen = false;
                    CurrentState = GameState.Menu;
                }
                }
            }
            else if (CurrentState == GameState.Gamelvl2)
            {
                DrawText("lvl 2", 960, 200, 200, Color.Black);

                // karakter mozgasa (your old method still works here, but no collision)
                walk(ref playerpos.X, ref playerpos.Y, speed * GetFrameTime(), walking, medium_cat_back, medium_cat_left, medium_cat_right, medium_cat);

                // shoot
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Vector2 start = new Vector2(playerpos.X + medium_cat.Width / 2f, playerpos.Y + medium_cat.Height / 2f);
                    Vector2 target = mouse;

                    Vector2 dir = target - start;
                    float dist = dir.Length();

                    if (dist > 1f)
                    {
                        dir /= dist;

                        bulletPositions.Add(start);
                        bulletVelocities.Add(dir * bulletSpeed);
                        bulletRemaining.Add(dist);
                    }
                }

                float dt = Raylib.GetFrameTime();

                for (int i = bulletPositions.Count - 1; i >= 0; i--)
                {
                    Vector2 step = bulletVelocities[i] * dt;
                    float stepLen = step.Length();

                    bulletPositions[i] += step;
                    bulletRemaining[i] -= stepLen;

                    if (bulletRemaining[i] <= 0f)
                    {
                        bulletPositions.RemoveAt(i);
                        bulletVelocities.RemoveAt(i);
                        bulletRemaining.RemoveAt(i);
                    }
                }

                for (int i = 0; i < bulletPositions.Count; i++)
                {
                    Raylib.DrawCircleV(bulletPositions[i], 5f, Color.Red);
                }

                // enemies
                float enemySpeed = 130f;

                Vector2 playerCenter = new Vector2(playerpos.X + medium_cat.Width / 2f, playerpos.Y + medium_cat.Height / 2f);

                for (int i = 0; i < enemyPositions.Count; i++)
                {
                    Vector2 dir = playerCenter - enemyPositions[i];
                    float dist = dir.Length();

                    if (dist > 0.001f)
                    {
                        dir /= dist;
                        enemyPositions[i] += dir * enemySpeed * dt;
                    }
                }

                float bulletRadius = 5f;

                for (int b = bulletPositions.Count - 1; b >= 0; b--)
                {
                    for (int e = enemyPositions.Count - 1; e >= 0; e--)
                    {
                        float hitDist = Vector2.Distance(bulletPositions[b], enemyPositions[e]);

                        if (hitDist <= bulletRadius + enemyRadii[e])
                        {
                            enemyHP[e] -= 1;

                            bulletPositions.RemoveAt(b);
                            bulletVelocities.RemoveAt(b);
                            bulletRemaining.RemoveAt(b);

                            if (enemyHP[e] <= 0)
                            {
                                enemyPositions.RemoveAt(e);
                                enemyRadii.RemoveAt(e);
                                enemyHP.RemoveAt(e);
                            }

                            break;
                        }
                    }
                }

                for (int i = 0; i < enemyPositions.Count; i++)
                {
                    Raylib.DrawCircleV(enemyPositions[i], enemyRadii[i], Color.Green);
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}