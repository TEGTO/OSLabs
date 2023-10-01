using ClassLibrary.Function;
using Lab1.MyIO;

class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Environment.ExitCode = -1;
                throw new Exception("Not enough arguments!");
            }
            //IORedirector.PrintError("Not enough arguments!", append: true);
            else
            {
                string funType = args[0];
                string x = args[1];
                Type type = Type.GetType(args.Length >= 3 ? $"{funType}, {args[2]}" : funType);
                object convertedFun = Activator.CreateInstance(type);
                FunctionBase fun = (FunctionBase)convertedFun;
                IORedirector.PrintLineStandartOut(fun.MakeCalculations(int.Parse(x)).ToString());
            }
        }
        catch (Exception ex)
        {
            Environment.ExitCode = -1;
            throw ex;
        }
    }
}