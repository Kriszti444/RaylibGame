using System;
using System.Numerics;
using Raylib_cs;

enum GameState
{
    Menu = 0,
    Play,
    GameOver,
    Win
}

struct Player
{
    public Vector2 Pos;
    public Vector2 Size;
    public float Speed;
    public int Lives;
    public int Score;
    public bool Active;
    public Color Color;
}

struct Enemy
{
    public Vector2 Pos;
    public Vector2 Vel;
    public float Radius;
    public bool Active;
}

struct Coin
{
    public Vector2 Pos;
    public float Radius;
    public bool Active;
}

class Program
{

    static bool UiButton(Rectangle r, string text, int fontSize, Color baseCol, Color hoverCol, Color textCol)
    {
        Vector2 m = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(m, r);

        Color col = hover ? hoverCol : baseCol;

        Raylib.DrawRectangleRec(r, col);
        Raylib.DrawRectangleLinesEx(r, 2, Color.Black);

        int tw = Raylib.MeasureText(text, fontSize);
        int tx = (int)(r.X + r.Width / 2 - tw / 2);
        int ty = (int)(r.Y + r.Height / 2 - fontSize / 2);
        Raylib.DrawText(text, tx, ty, fontSize, textCol);

        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    static void DrawPanel(Rectangle r)
    {
        Raylib.DrawRectangleRec(r, new Color(0, 0, 0, 160));
        Raylib.DrawRectangleLinesEx(r, 2, new Color(255, 255, 255, 140));
    }
    static void GenerateRandomMap(int[,] map, int mapW, int mapH, Random rng)
    {
        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                if (rng.Next(0, 100) < 80)
                    map[y, x] = 0;   // green
                else
                    map[y, x] = 1;   // other
            }
        }
    }

    static float Clamp(float v, float a, float b)
    {
        if (v < a) return a;
        if (v > b) return b;
        return v;
    }

    static float Dist(Vector2 a, Vector2 b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    static Rectangle PlayerRect(in Player p)
    {
        return new Rectangle(p.Pos.X, p.Pos.Y, p.Size.X, p.Size.Y);
    }

    static Vector2 RandPos(Random rng, int w, int h, int margin)
    {
        return new Vector2(rng.Next(margin, w - margin), rng.Next(margin, h - margin));
    }

    static Vector2 RandVel(Random rng, float minSpeed, float maxSpeed)
    {
        float sx = (float)(rng.NextDouble() * 2.0 - 1.0);
        float sy = (float)(rng.NextDouble() * 2.0 - 1.0);
        Vector2 d = new Vector2(sx, sy);
        if (d.LengthSquared() < 0.0001f) d = new Vector2(1, 0);
        d = Vector2.Normalize(d);
        float sp = minSpeed + (float)rng.NextDouble() * (maxSpeed - minSpeed);
        return d * sp;
    }

    static Rectangle SrcRectFromId(int id, int tileSize, int tilesPerRow)
    {
        int sx = (id % tilesPerRow) * tileSize;
        int sy = (id / tilesPerRow) * tileSize;
        return new Rectangle(sx, sy, tileSize, tileSize);
    }

    static void DrawTileBackground(Texture2D green, Texture2D other, int[,] map, int tileSize)
    {
        int mapH = map.GetLength(0);
        int mapW = map.GetLength(1);

        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                Texture2D tex = (map[y, x] == 0) ? green : other;

                Raylib.DrawTextureEx(
                    tex,
                    new Vector2(x * tileSize, y * tileSize),
                    0f,
                    (float)tileSize / tex.Width,
                    Color.White
                );
            }
        }
    }

    static void Main()
    {
        const int screenW = 1000;
        const int screenH = 650;

        Raylib.InitWindow(screenW, screenH, "batyu játék");
        Raylib.SetTargetFPS(60);

        Random rng = new Random();

        GameState state = GameState.Menu;

        bool twoPlayers = false;

        int level = 1;
        int coinsToWin = 10;
        int maxLevel = 5;

        float playTime = 0f;

        Player p1 = new Player
        {
            Pos = new Vector2(120, 120),
            Size = new Vector2(34, 34),
            Speed = 240f,
            Lives = 3,
            Score = 0,
            Active = true,
            Color = Color.SkyBlue
        };

        Player p2 = new Player
        {
            Pos = new Vector2(820, 480),
            Size = new Vector2(34, 34),
            Speed = 240f,
            Lives = 3,
            Score = 0,
            Active = false,
            Color = Color.Orange
        };

        const int ENEMY_MAX = 32;
        const int COIN_MAX = 40;

        Enemy[] enemies = new Enemy[ENEMY_MAX];
        Coin[] coins = new Coin[COIN_MAX];

        int enemyCount = 0;
        int coinCount = 0;

        float hitCooldown1 = 0f;
        float hitCooldown2 = 0f;

        void ResetPlayers()
        {
            p1.Pos = new Vector2(screenW / 2 - 50, screenH / 2);
            p1.Lives = 3;
            p1.Score = 0;
            p1.Active = true;

            p2.Pos = new Vector2(screenW / 2 + 50, screenH / 2);
            p2.Lives = 3;
            p2.Score = 0;
            p2.Active = twoPlayers;

            hitCooldown1 = 0f;
            hitCooldown2 = 0f;
        }

        void ClearWorld()                   
        {
            for (int i = 0; i < ENEMY_MAX; i++) enemies[i].Active = false;
            for (int i = 0; i < COIN_MAX; i++) coins[i].Active = false;
            enemyCount = 0;
            coinCount = 0;
        }

        Texture2D TILE_GREEN = Raylib.LoadTexture("kinezetek_and_stuff/FieldsTile_38.png");
        Texture2D TILE_OTHER = Raylib.LoadTexture("kinezetek_and_stuff/FieldsTile_48.png");
        int tileSize = 32;

        int mapW = (screenW + tileSize - 1) / tileSize;
        int mapH = (screenH + tileSize - 1) / tileSize;
        int[,] map = new int[mapH, mapW];

        void SpawnLevel(int lvl)
        {
            GenerateRandomMap(map, mapW, mapH, rng);
            ClearWorld();

            int baseEnemies = 2 + (lvl - 1);
            int extra = twoPlayers ? 1 : 0;
            enemyCount = Math.Clamp(baseEnemies + extra, 2, ENEMY_MAX);

            coinCount = Math.Clamp(8 + lvl * 2, 8, COIN_MAX);

            float enemyMinSp = 110f + (lvl - 1) * 25f;
            float enemyMaxSp = 170f + (lvl - 1) * 35f;

            for (int i = 0; i < enemyCount; i++)
            {
                enemies[i].Active = true;
                enemies[i].Radius = 16f;
                enemies[i].Pos = RandPos(rng, screenW, screenH, 70);
                enemies[i].Vel = RandVel(rng, enemyMinSp, enemyMaxSp);
            }

            for (int i = 0; i < coinCount; i++)
            {
                coins[i].Active = true;
                coins[i].Radius = 10f;
                coins[i].Pos = RandPos(rng, screenW, screenH, 60);
            }
        }

        void StartNewGame()
        {
            level = 1;
            coinsToWin = 10;
            playTime = 0f;
            ResetPlayers();
            SpawnLevel(level);
            state = GameState.Play;
        }

        void NextLevel()
        {
            level++;
            coinsToWin += 6;
            SpawnLevel(level);
        }

        Texture2D chickSheet = Raylib.LoadTexture("kinezetek_and_stuff/Chick_animation_without_shadow.png");
        Texture2D foxSheet = Raylib.LoadTexture("kinezetek_and_stuff/Fox_walk.png");

        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                map[y, x] = 0;
            }
        }

        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                if ((x + y) % 9 == 0) map[y, x] = 1;
                if ((x + y) % 13 == 0) map[y, x] = 2;
            }
        }

        int foxFrameW = 32;
        int foxFrameH = 32;
        int foxFramesPerRow = 6;

        int foxFrame = 0;
        float foxTimer = 0f;
        float foxFrameSpeed = 0.10f;

        float foxScale = 2.2f;
        int foxWalkRow = 2;

        int chickFrameW = 16;
        int chickFrameH = 16;
        int chickFramesPerRow = 6;
        int chickWalkRow = 2;

        int chickFrame1 = 0;
        float chickTimer1 = 0f;

        int chickFrame2 = 0;
        float chickTimer2 = 0f;

        float chickFrameSpeed = 0.12f;

        bool chickFlip1 = false;
        bool chickFlip2 = false;
        float chickScale = 3.0f;

        while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();

    int cx = screenW / 2;
    int cy = screenH / 2;

    if (state == GameState.Menu)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One))
        {
            twoPlayers = false;
            p2.Active = false;
            StartNewGame();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Two))
        {
            twoPlayers = true;
            p2.Active = true;
            StartNewGame();
        }
    }
    else if (state == GameState.Play)
    {
        playTime += dt;

        Vector2 move1 = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W)) move1.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) move1.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) move1.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) move1.X += 1;
        if (move1.LengthSquared() > 0) move1 = Vector2.Normalize(move1);
        p1.Pos += move1 * p1.Speed * dt;

        p1.Size = new Vector2(chickFrameW * chickScale, chickFrameH * chickScale);
        p2.Size = new Vector2(chickFrameW * chickScale, chickFrameH * chickScale);

        p1.Pos.X = Clamp(p1.Pos.X, 0, screenW - p1.Size.X);
        p1.Pos.Y = Clamp(p1.Pos.Y, 0, screenH - p1.Size.Y);

        bool p1Walking = move1.LengthSquared() > 0.0001f;

        if (move1.X < 0) chickFlip1 = false;
        else if (move1.X > 0) chickFlip1 = true;

        if (p1Walking)
        {
            chickTimer1 += dt;
            if (chickTimer1 >= chickFrameSpeed)
            {
                chickTimer1 = 0f;
                chickFrame1++;
                if (chickFrame1 >= chickFramesPerRow) chickFrame1 = 0;
            }
        }
        else
        {
            chickFrame1 = 0;
            chickTimer1 = 0f;
        }


        

        if (p2.Active)
        {
            Vector2 move2 = Vector2.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.Up)) move2.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.Down)) move2.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.Left)) move2.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) move2.X += 1;

            if (move2.LengthSquared() > 0) move2 = Vector2.Normalize(move2);
            p2.Pos += move2 * p2.Speed * dt;

            if (move2.X < 0) chickFlip2 = false;
            else if (move2.X > 0) chickFlip2 = true;

            p2.Pos.X = Clamp(p2.Pos.X, 0, screenW - p2.Size.X);
            p2.Pos.Y = Clamp(p2.Pos.Y, 0, screenH - p2.Size.Y);

        bool p2Walking = move2.LengthSquared() > 0.0001f;

            if (p2Walking)
        {
            chickTimer2 += dt;
            if (chickTimer2 >= chickFrameSpeed)
            {
                chickTimer2 = 0f;
                chickFrame2++;
                if (chickFrame2 >= chickFramesPerRow) chickFrame2 = 0;
            }
        }
        else
        {
            chickFrame2 = 0;
            chickTimer2 = 0f;
        }


        }

        foxTimer += dt;
        if (foxTimer >= foxFrameSpeed)
        {
            foxTimer = 0f;
            foxFrame++;
            if (foxFrame >= foxFramesPerRow) foxFrame = 0;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (!enemies[i].Active) continue;

            enemies[i].Pos += enemies[i].Vel * dt;

            if (enemies[i].Pos.X < enemies[i].Radius)
            {
                enemies[i].Pos.X = enemies[i].Radius;
                enemies[i].Vel.X *= -1;
            }
            if (enemies[i].Pos.X > screenW - enemies[i].Radius)
            {
                enemies[i].Pos.X = screenW - enemies[i].Radius;
                enemies[i].Vel.X *= -1;
            }
            if (enemies[i].Pos.Y < enemies[i].Radius)
            {
                enemies[i].Pos.Y = enemies[i].Radius;
                enemies[i].Vel.Y *= -1;
            }
            if (enemies[i].Pos.Y > screenH - enemies[i].Radius)
            {
                enemies[i].Pos.Y = screenH - enemies[i].Radius;
                enemies[i].Vel.Y *= -1;
            }
        }

        if (hitCooldown1 > 0) hitCooldown1 -= dt;
        if (hitCooldown2 > 0) hitCooldown2 -= dt;

        Rectangle r1 = new Rectangle(0, 0, 0, 0);
        Rectangle r2 = new Rectangle(0, 0, 0, 0);

        if (p1.Active) r1 = PlayerRect(p1);
        if (p2.Active) r2 = PlayerRect(p2);

        for (int i = 0; i < coinCount; i++)
        {
            if (!coins[i].Active) continue;

            Vector2 c = coins[i].Pos;
            float rad = coins[i].Radius;

            bool p1Hit = p1.Active && Raylib.CheckCollisionCircleRec(c, rad, r1);
            bool p2Hit = p2.Active && Raylib.CheckCollisionCircleRec(c, rad, r2);

            if (p1Hit || p2Hit)
            {
                coins[i].Active = false;
                if (p1Hit) p1.Score++;
                if (p2Hit) p2.Score++;
            }
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (!enemies[i].Active) continue;

            Vector2 e = enemies[i].Pos;
            float rad = enemies[i].Radius;

            bool p1Hit = p1.Active && Raylib.CheckCollisionCircleRec(e, rad, r1);
            bool p2Hit = p2.Active && Raylib.CheckCollisionCircleRec(e, rad, r2);

            if (p1Hit && hitCooldown1 <= 0f && p1.Active)
            {
                p1.Lives--;
                hitCooldown1 = 0.6f;
                if (p1.Lives <= 0) p1.Active = false;
            }

            if (p2Hit && hitCooldown2 <= 0f && p2.Active)
            {
                p2.Lives--;
                hitCooldown2 = 0.6f;
                if (p2.Lives <= 0) p2.Active = false;
            }
        }

        if (!twoPlayers)
        {
            if (!p1.Active) state = GameState.GameOver;
        }
        else
        {
            if (!p1.Active && !p2.Active) state = GameState.GameOver;
        }

        if (state == GameState.Play)
        {
            bool allCoinsCollected = true;
            for (int i = 0; i < coinCount; i++)
            {
                if (coins[i].Active)
                {
                    allCoinsCollected = false;
                    break;
                }
            }

            if (allCoinsCollected)
            {
                if (level >= maxLevel) state = GameState.Win;
                else NextLevel();
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            state = GameState.Menu;
    }
    else
    {
        if (Raylib.IsKeyPressed(KeyboardKey.R))
            StartNewGame();

        if (Raylib.IsKeyPressed(KeyboardKey.M))
            state = GameState.Menu;
    }

    Raylib.BeginDrawing();

    if (state == GameState.Play)
        DrawTileBackground(TILE_GREEN, TILE_OTHER, map, tileSize);
    else if (state == GameState.Menu)
        Raylib.ClearBackground(new Color(10, 20, 35, 255));
    else if (state == GameState.GameOver)
        Raylib.ClearBackground(new Color(35, 10, 15, 255));
    else
        Raylib.ClearBackground(new Color(10, 35, 15, 255));

    if (state == GameState.Menu)
    {
        Raylib.DrawText("Coop Chaos", cx - Raylib.MeasureText("Coop Chaos", 52) / 2, 120, 52, Color.RayWhite);
        Raylib.DrawText("Gather all the golden corn. Escape the foxes.", cx - Raylib.MeasureText("Gather all the golden corn. Escape the foxes.", 22) / 2, 185, 22, Color.LightGray);

        Rectangle panel = new Rectangle(cx - 260, cy - 80, 520, 260);
        DrawPanel(panel);

        Raylib.DrawText("Choose mode:", (int)panel.X + 30, (int)panel.Y + 25, 22, Color.RayWhite);

        Rectangle b1 = new Rectangle(panel.X + 30, panel.Y + 70, panel.Width - 60, 55);
        Rectangle b2 = new Rectangle(panel.X + 30, panel.Y + 140, panel.Width - 60, 55);

        bool click1 = UiButton(b1, "ONE PLAYER (press 1)", 22, new Color(40, 70, 120, 255), new Color(70, 110, 180, 255), Color.RayWhite);
        bool click2 = UiButton(b2, "TWO PLAYERS (press 2)", 22, new Color(120, 70, 40, 255), new Color(180, 110, 70, 255), Color.RayWhite);

        Raylib.DrawText("Controls: P1 WASD   P2 Arrows",
            cx - Raylib.MeasureText("Controls: P1 WASD   P2 Arrows", 18) / 2,
            (int)panel.Y + 210, 18, new Color(200, 200, 200, 255));

        if (click1)
        {
            twoPlayers = false;
            p2.Active = false;
            StartNewGame();
        }
        if (click2)
        {
            twoPlayers = true;
            p2.Active = true;
            StartNewGame();
        }
    }
    else if (state == GameState.Play)
    {
        Raylib.DrawRectangleLines(20, 20, screenW - 40, screenH - 40, Color.SkyBlue);

        for (int i = 0; i < coinCount; i++)
        {
            if (!coins[i].Active) continue;
            Raylib.DrawCircleV(coins[i].Pos, coins[i].Radius, Color.Gold);
            Raylib.DrawCircleLines((int)coins[i].Pos.X, (int)coins[i].Pos.Y, coins[i].Radius, Color.RayWhite);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (!enemies[i].Active) continue;

            bool flip = enemies[i].Vel.X > 0;

            Rectangle srcFox = new Rectangle(foxFrame * foxFrameW, foxWalkRow * foxFrameH, foxFrameW, foxFrameH);
            if (flip)
            {
                srcFox.X += foxFrameW;
                srcFox.Width = -foxFrameW;
            }

            float w = foxFrameW * foxScale;
            float h = foxFrameH * foxScale;

            Rectangle dstFox = new Rectangle(
                enemies[i].Pos.X - w / 2f,
                enemies[i].Pos.Y - h / 2f,
                w,
                h
            );

            Raylib.DrawTexturePro(foxSheet, srcFox, dstFox, new Vector2(0, 0), 0f, Color.White);
        }

        Rectangle srcChick1 = new Rectangle(chickFrame1 * chickFrameW, chickWalkRow * chickFrameH, chickFrameW, chickFrameH);
        if (chickFlip1)
        {
            srcChick1.X += chickFrameW;
            srcChick1.Width = -chickFrameW;
        }

        Rectangle srcChick2 = new Rectangle(chickFrame2 * chickFrameW, chickWalkRow * chickFrameH, chickFrameW, chickFrameH);
        if (chickFlip2)
        {
            srcChick2.X += chickFrameW;
            srcChick2.Width = -chickFrameW;
        }

        if (p1.Active)
        {
            Rectangle dstChick = new Rectangle(p1.Pos.X, p1.Pos.Y, p1.Size.X, p1.Size.Y);
            Raylib.DrawTexturePro(chickSheet, srcChick1, dstChick, new Vector2(0, 0), 0f, Color.White);
        }

        if (p2.Active)
        {
            Rectangle dstChick = new Rectangle(p2.Pos.X, p2.Pos.Y, p2.Size.X, p2.Size.Y);
            Raylib.DrawTexturePro(chickSheet, srcChick2, dstChick, new Vector2(0, 0), 0f, Color.White);
        }

        int totalScoree = p1.Score + (p2.Active ? p2.Score : 0);
        string t = $"Level: {level}/{maxLevel}   Total: {totalScoree}   Time: {playTime:0.0}s";
        Raylib.DrawText(t, 30, 30, 20, Color.RayWhite);

        string hud1 = $"P1 Lives: {p1.Lives}  Score: {p1.Score}";
        Raylib.DrawText(hud1, 30, 60, 20, Color.SkyBlue);

        if (p2.Active)
        {
            string hud2 = $"P2 Lives: {p2.Lives}  Score: {p2.Score}";
            Raylib.DrawText(hud2, 30, 85, 20, Color.Orange);
        }

    }
    else if (state == GameState.GameOver)
    {
        Raylib.DrawText("GAME OVER", cx - Raylib.MeasureText("GAME OVER", 64) / 2, 110, 64, new Color(255, 220, 220, 255));

        Rectangle panell = new Rectangle(cx - 260, 220, 520, 270);
        DrawPanel(panell);

        int totalScore = p1.Score + (p2.Active ? p2.Score : 0);

        Raylib.DrawText($"Level reached: {level}", (int)panell.X + 30, (int)panell.Y + 30, 24, Color.RayWhite);
        Raylib.DrawText($"Total golden corns: {totalScore}", (int)panell.X + 30, (int)panell.Y + 65, 24, Color.RayWhite);
        Raylib.DrawText($"Time: {playTime:0.0}s", (int)panell.X + 30, (int)panell.Y + 100, 24, Color.RayWhite);

        Rectangle rBtn = new Rectangle(panell.X + 30, panell.Y + 150, panell.Width - 60, 55);
        Rectangle mBtn = new Rectangle(panell.X + 30, panell.Y + 215, panell.Width - 60, 55);

        bool restart = UiButton(rBtn, "RESTART (R)", 22, new Color(90, 30, 40, 255), new Color(140, 50, 70, 255), Color.RayWhite);
        bool menu = UiButton(mBtn, "MENU (M)", 22, new Color(40, 40, 40, 255), new Color(70, 70, 70, 255), Color.RayWhite);

        if (restart) StartNewGame();
        if (menu) state = GameState.Menu;
    }
    else if (state == GameState.Win)
    {
        Raylib.DrawText("YOU WIN!", cx - Raylib.MeasureText("YOU WIN!", 64) / 2, 110, 64, Color.RayWhite);

        Rectangle panell = new Rectangle(cx - 260, 220, 520, 240);
        DrawPanel(panell);

        int totalScore = p1.Score + (p2.Active ? p2.Score : 0);

        Raylib.DrawText($"Finished: {level}/{maxLevel}", (int)panell.X + 30, (int)panell.Y + 30, 24, Color.RayWhite);
        Raylib.DrawText($"Total coins: {totalScore}", (int)panell.X + 30, (int)panell.Y + 65, 24, Color.RayWhite);
        Raylib.DrawText($"Time: {playTime:0.0}s", (int)panell.X + 30, (int)panell.Y + 100, 24, Color.RayWhite);

        Rectangle mBtn = new Rectangle(panell.X + 30, panell.Y + 160, panell.Width - 60, 55);
        bool menu = UiButton(mBtn, "MENU (M)", 22, new Color(40, 40, 40, 255), new Color(70, 70, 70, 255), Color.RayWhite);
        if (menu) state = GameState.Menu;
    }

    Raylib.EndDrawing();
    }

        Raylib.UnloadTexture(foxSheet);
        Raylib.UnloadTexture(chickSheet);
        Raylib.CloseWindow();
    }
}