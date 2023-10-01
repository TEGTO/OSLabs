using ClassLibrary.Function;
using Lab1.Function;
using Lab1.MyIO;
using System.Diagnostics;

namespace Lab1.Manager
{
    class MyManager
    {
        private string typeFunctionA;
        private string typeFunctionB;
        private string processName;
        private string functionsAssemblyName;
        private int x;
        public MyManager(FunctionBase functionA, FunctionBase functionB, int x, string processName, string functionsAssemblyName = "")
        {
            this.typeFunctionA = functionA.GetType().ToString();
            this.typeFunctionB = functionB.GetType().ToString();
            this.x = x;
            this.processName = processName;
            this.functionsAssemblyName = functionsAssemblyName;
        }
        public void SetFunctions(FunctionBase functionA, FunctionBase functionB, string functionsAssemblyName = "")
        {
            this.typeFunctionA = functionA.GetType().ToString();
            this.typeFunctionB = functionB.GetType().ToString();
            this.functionsAssemblyName = functionsAssemblyName;
        }
        public void SetX(int x) => this.x = x;
        public double GetComputedResult()
        {
            CalculateFunProcess(typeFunctionA, x, "resultFirst.txt", functionsAssemblyName);
            CalculateFunProcess(typeFunctionB, x, "resultSecond.txt", functionsAssemblyName);
            return 0;
        }
        private void CalculateFunProcess(string funType, int x, string resultPath, string functionsAssemblyName)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = processName,
                        UseShellExecute = false,
                        Arguments = $"{funType} {x} {resultPath} {functionsAssemblyName}",
                        //RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                using (proc)
                {
                    proc.Start();
                    //while (!proc.StandardOutput.EndOfStream)
                    //{
                    //    string line = proc.StandardOutput.ReadLine();
                    //}
                }
            }
            catch (Exception e)
            {
                IORedirector.PrintError(e.Message);
            }
        }
    }
}
