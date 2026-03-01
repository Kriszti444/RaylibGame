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

    static void DrawTileBackground(Texture2D tileset, int[,] map, int tileSize)
    {
        int tilesPerRow = tileset.Width / tileSize;
        int mapH = map.GetLength(0);
        int mapW = map.GetLength(1);

        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                int id = map[y, x];
                if (id < 0) continue;

                Rectangle src = SrcRectFromId(id, tileSize, tilesPerRow);
                Rectangle dst = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                Raylib.DrawTexturePro(tileset, src, dst, Vector2.Zero, 0f, Color.White);
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
            p1.Pos = new Vector2(120, 120);
            p1.Lives = 3;
            p1.Score = 0;
            p1.Active = true;

            p2.Pos = new Vector2(820, 480);
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

        void SpawnLevel(int lvl)
        {
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

        Texture2D tileset = Raylib.LoadTexture("kinezetek_and_stuff/Tiles/FieldsTileset.png");
        int tileSize = 32;

        int mapW = (screenW + tileSize - 1) / tileSize;
        int mapH = (screenH + tileSize - 1) / tileSize;
        int[,] map = new int[mapH, mapW];

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

        int chickFrame = 0;
        float chickTimer = 0f;
        float chickFrameSpeed = 0.12f;

        bool chickFlip1 = false;
        bool chickFlip2 = false;
        float chickScale = 3.0f;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

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
                    chickTimer += dt;
                    if (chickTimer >= chickFrameSpeed)
                    {
                        chickTimer = 0f;
                        chickFrame++;
                        if (chickFrame >= chickFramesPerRow) chickFrame = 0;
                    }
                }
                else
                {
                    chickFrame = 0;
                    chickTimer = 0f;
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

                    // 👇 ADD THIS RIGHT HERE (no new declaration!)
                    if (move2.X < 0) chickFlip2 = false;
                    else if (move2.X > 0) chickFlip2 = true;

                    p2.Pos.X = Clamp(p2.Pos.X, 0, screenW - p2.Size.X);
                    p2.Pos.Y = Clamp(p2.Pos.Y, 0, screenH - p2.Size.Y);
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

                        if (p1.Lives <= 0)
                            p1.Active = false;
                    }

                    if (p2Hit && hitCooldown2 <= 0f && p2.Active)
                    {
                        p2.Lives--;
                        hitCooldown2 = 0.6f;

                        if (p2.Lives <= 0)
                            p2.Active = false;
                    }
                }

                int totalScore = p1.Score + (p2.Active ? p2.Score : 0);

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
                {
                    state = GameState.Menu;
                }
            }
            else
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    StartNewGame();
                }
                if (Raylib.IsKeyPressed(KeyboardKey.M))
                {
                    state = GameState.Menu;
                }
            }

            Raylib.BeginDrawing();

            if (state == GameState.Play)
            {
                DrawTileBackground(tileset, map, tileSize);
            }
            else
            {
                Raylib.ClearBackground(Color.DarkBlue);
            }

            if (state == GameState.Menu)
            {
                Raylib.DrawText("press 1 - One Player", 410, 240, 22, Color.RayWhite);
                Raylib.DrawText("press 2 - Two Players", 410, 280, 22, Color.RayWhite);
                Raylib.DrawText("Controls:", 430, 350, 20, Color.RayWhite);
                Raylib.DrawText("P1: W A S D", 410, 380, 20, Color.RayWhite);
                Raylib.DrawText("P2: Arrow Keys", 410, 410, 20, Color.RayWhite);
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

                Rectangle srcChick1 = new Rectangle(chickFrame * chickFrameW, chickWalkRow * chickFrameH, chickFrameW, chickFrameH);
                if (chickFlip1)
                {
                    srcChick1.X += chickFrameW;
                    srcChick1.Width = -chickFrameW;
                }

                Rectangle srcChick2 = new Rectangle(chickFrame * chickFrameW, chickWalkRow * chickFrameH, chickFrameW, chickFrameH);
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

                int totalScore = p1.Score + (p2.Active ? p2.Score : 0);
                string t = $"Level: {level}/{maxLevel}   Target: {coinsToWin}   Total: {totalScore}   Time: {playTime:0.0}s";
                Raylib.DrawText(t, 30, 30, 20, Color.RayWhite);

                string hud1 = $"P1 Lives: {p1.Lives}  Score: {p1.Score}";
                Raylib.DrawText(hud1, 30, 60, 20, Color.SkyBlue);

                if (p2.Active)
                {
                    string hud2 = $"P2 Lives: {p2.Lives}  Score: {p2.Score}";
                    Raylib.DrawText(hud2, 30, 85, 20, Color.Orange);
                }

                Raylib.DrawText("ESC: Menu", screenW - 150, 30, 18, Color.LightGray);
            }
            else if (state == GameState.GameOver)
            {
                Raylib.DrawText("GAME OVER", 380, 150, 52, Color.RayWhite);

                int totalScore = p1.Score + (p2.Active ? p2.Score : 0);
                Raylib.DrawText($"Reached level: {level}", 410, 240, 22, Color.LightGray);
                Raylib.DrawText($"Total score: {totalScore}", 410, 270, 22, Color.LightGray);
                Raylib.DrawText($"Time: {playTime:0.0}s", 410, 300, 22, Color.LightGray);

                if (p2.Active)
                {
                    string winner;
                    if (p1.Score > p2.Score) winner = "Winner: Player 1";
                    else if (p2.Score > p1.Score) winner = "Winner: Player 2";
                    else winner = "Draw!";
                    Raylib.DrawText(winner, 420, 350, 26, Color.Gold);
                }

                Raylib.DrawText("R - Restart   M - Menu", 365, 450, 24, Color.RayWhite);
            }
            else if (state == GameState.Win)
            {
                Raylib.DrawText("YOU WIN!", 410, 150, 52, Color.RayWhite);

                int totalScore = p1.Score + (p2.Active ? p2.Score : 0);
                Raylib.DrawText($"Finished level: {level}/{maxLevel}", 375, 240, 22, Color.LightGray);
                Raylib.DrawText($"Total score: {totalScore}", 410, 270, 22, Color.LightGray);
                Raylib.DrawText($"Time: {playTime:0.0}s", 410, 300, 22, Color.LightGray);

                if (p2.Active)
                {
                    string winner;
                    if (p1.Score > p2.Score) winner = "Winner: Player 1";
                    else if (p2.Score > p1.Score) winner = "Winner: Player 2";
                    else winner = "Draw!";
                    Raylib.DrawText(winner, 420, 350, 26, Color.Gold);
                }

                Raylib.DrawText("R - Restart   M - Menu", 365, 450, 24, Color.RayWhite);
            }

            Raylib.EndDrawing();
        }

        Raylib.UnloadTexture(tileset);
        Raylib.UnloadTexture(foxSheet);
        Raylib.UnloadTexture(chickSheet);
        Raylib.CloseWindow();
    }
}