using ClassLibrary.Function.Types;
using Lab1.Manager;
using Lab1.MyIO;

namespace Lab1
{
    class Program
    {
        private static async Task Main()
        {
            FunctionA functionA = new FunctionA();
            FunctionB functionB = new FunctionB();
            bool isLeaving = false;
            MyManager myManager = new MyManager(functionA, functionB, 0, "D:\\Studies\\Course_3\\OS\\Labs\\Lab1\\FunctionApp\\bin\\Debug\\net7.0\\FunctionApp.exe", "ClassLibrary");
            while (!isLeaving)
            {
                IORedirector.PrintLineStandartOut("\nChoose option: ");
                IORedirector.PrintLineStandartOut($"1.Enter x\n2.Exit");
                string chooice = IORedirector.ReadLineStandartIn();
                switch (chooice)
                {
                    case "1":
                        IORedirector.PrintLineStandartOut("Enter x: ");
                        if (int.TryParse(IORedirector.ReadLineStandartIn(), out int x))
                        {
                            myManager.SetX(x);
                            await myManager.GetComputedResult();
                        }
                        else
                            IORedirector.PrintError("x is not int!!", append: true);
                        break;
                    case "2":
                        isLeaving = true;
                        break;
                    default:
                        IORedirector.PrintLineStandartOut("Incorrect choice, please try again.");
                        break;
                }
            }
        }
    }
}