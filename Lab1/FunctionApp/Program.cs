using ClassLibrary.Function;
using Lab1.MyIO;

class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 3)
            IORedirector.PrintError("Not enough arguments!");
        else
        {
            string funType = args[0];
            string x = args[1];
            string resultPath = args[2];
            Type type = Type.GetType(args.Length >= 4 ? $"{funType}, {args[3]}" : funType);
            object convertedFun = Activator.CreateInstance(type);
            FunctionBase fun = (FunctionBase)convertedFun;
            IORedirector.Print(fun.MakeCalculations(int.Parse(x)).ToString(), resultPath);
        }
    }
}