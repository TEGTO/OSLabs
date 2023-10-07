using ClassLibrary.Function.Types;
using Lab1.Manager;
using Lab1.MyIO;

namespace Lab1
{
    class Program
    {
        static bool isManagerFinished = false;
        private static async Task Main()
        {
            FunctionA functionA = new FunctionA();
            FunctionB functionB = new FunctionB();
            MyManager myManager = new MyManager(functionA, functionB, 0, "D:\\Studies\\Course_3\\OS\\Labs\\Lab1\\FunctionApp\\bin\\Debug\\net7.0\\FunctionApp.exe", "ClassLibrary");
            bool isLeaving = false;
            string resultTxtPeport = string.Empty;
            CancellationTokenSource cts = new CancellationTokenSource();
            while (!isLeaving)
            {
                IORedirector.PrintLineStandartOut("\nChoose option: ");
                IORedirector.PrintLineStandartOut($"1.Enter x\n2.Exit");
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        IORedirector.PrintLineStandartOut("Enter x: ");
                        if (int.TryParse(IORedirector.ReadLineStandartIn(), out int x))
                        {
                            IORedirector.PrintLineStandartOut($"F1 to call PromtMenu");
                            myManager.SetX(x);
                            isManagerFinished = false;
                            // Start monitoring for key press in a separate task
                            Task monitorKeyPress = Task.Run(() =>
                            {
                                PromtMenu(myManager);
                            });
                            resultTxtPeport = await myManager.GetComputedResult();
                            isManagerFinished = true;
                        }
                        else
                            IORedirector.PrintError("x is not int!!", append: true);
                        IORedirector.PrintLineStandartOut(resultTxtPeport);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        isLeaving = true;
                        break;
                    default:
                        IORedirector.PrintLineStandartOut("Incorrect choice, please try again.");
                        break;
                }
            }
        }
        public static void PromtMenu(MyManager myManager)
        {
            while (!isManagerFinished)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.F1 && !isManagerFinished)
                {
                    IORedirector.PrintLineStandartOut($"C.Cancel operations x\nI.Show operations status");
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.C:
                            myManager.CancelEvent?.Invoke();
                            break;
                        case ConsoleKey.I:
                            myManager.ShowFunInfoEvent?.Invoke();
                            break;
                        default:
                            IORedirector.PrintLineStandartOut("EXIT FROM PROMT MENU");
                            break;
                    }
                }
            }
        }
    }
}