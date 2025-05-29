namespace mylib;

public class Class1
{
    public int MyPublicNumber {get;set;} = 111;

    private int _myNumber = 213;

    public void PrintMyNumber()
    {
        Console.WriteLine($"hello from mylib. My number is {_myNumber}");
    }

    public static void SayHello()
    {
        Console.WriteLine($"hello from mylib");
    }

    public string DoStuff(int num)
    {
        if (num == 0)
        {
            return "num is 0";
        }
        // all ifs should be bracketed
        if (num == 1) return "num is 1";

        return "num is not 0 or 1";
    }

    public void RandomExample()
    {
        // prefer var
        Random rng = new Random();
        DoStuff(rng.Next());
    }
}
