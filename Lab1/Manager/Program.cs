using ClassLibrary.Function.Types;
using Lab1.Manager;
using Lab1.MyIO;

namespace Lab1
{
    class Program
    {
        private static async Task Main()
        {
            IORedirector.PrintLineStandartOut("Enter x: ");
            if (int.TryParse(IORedirector.ReadLineStandartIn(), out int x))
            {
                FunctionA functionA = new FunctionA();
                FunctionB functionB = new FunctionB();
                MyManager myManager = new MyManager(functionA, functionB, x, "D:\\Studies\\Course_3\\OS\\Labs\\Lab1\\FunctionApp\\bin\\Debug\\net7.0\\FunctionApp.exe", "ClassLibrary");
                await myManager.GetComputedResult();
            }
            else
                IORedirector.PrintError("x is not int!!", append: true);
        }
    }
}