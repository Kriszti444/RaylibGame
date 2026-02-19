using Raylib_cs;

namespace bl.cs;

class Program
{

    
    static void Main(string[] args)
    {
        Raylib.InitWindow(1200, 600, "Raylib Inside Existing Project");

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            Raylib.DrawText("Raylib works!", 250, 200, 20, Color.Black);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
