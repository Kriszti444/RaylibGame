namespace bl.cs;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Csaba is very buta!");
        Console.WriteLine("i will guess your number...");
        Console.Write("enter a number: ");

        int a = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine($"your number is {a}!");
    }
}
