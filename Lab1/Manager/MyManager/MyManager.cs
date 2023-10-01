using ClassLibrary.Function;
using Lab1.MyIO;
using System.Diagnostics;

namespace Lab1.Manager
{
    class MyManager
    {
        private const float WAIT_TIME = 0;
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
        public async Task GetComputedResult()
        {
            double resultFirst = double.NaN, resultSecond = double.NaN;
            try
            {
                resultFirst = await CalculateFunProcessAsync(typeFunctionA, x, functionsAssemblyName);
                IORedirector.PrintLineStandartOut(resultFirst.ToString());

            }
            catch (Exception ex)
            {
                IORedirector.PrintError("Error occured in the first process: " + ex.Message, append: true);
            }
            try
            {
                resultSecond = await CalculateFunProcessAsync(typeFunctionB, x, functionsAssemblyName);
                IORedirector.PrintLineStandartOut(resultSecond.ToString());
            }
            catch (Exception ex)
            {
                IORedirector.PrintError("Error occured in the second process: " + ex.Message, append: true);
            }
            IORedirector.PrintLineStandartOut("Result: " + (resultFirst + resultSecond).ToString());
            IORedirector.Print((resultFirst + resultSecond).ToString(), "result.txt");
        }
        private async Task<double> CalculateFunProcessAsync(string funType, int x, string functionsAssemblyName)
        {
            double result = double.NaN;
            var tcs = new TaskCompletionSource<double>();
            var timeout = TimeSpan.FromMilliseconds(WAIT_TIME); // desired timeout value
            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = processName,
                    UseShellExecute = false,
                    Arguments = $"{funType} {x} {functionsAssemblyName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, args) =>
                {
                    if (proc.ExitCode != 0)
                    {
                        var errorMessage = proc.StandardError.ReadToEnd();
                        tcs.TrySetException(new InvalidOperationException("The process did not exit correctly. " +
                            "The corresponding error message was: " + errorMessage));
                    }
                    else
                    {
                        string line = proc.StandardOutput.ReadLine();
                        result = Convert.ToDouble(line);
                        tcs.TrySetResult(result);
                    }
                };
                proc.Start();
                // This is the timeout mechanism
                if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)) == tcs.Task)
                {
                    // Process completed. Return the result or throw any exceptions
                    return await tcs.Task;
                }
                else
                {
                    // Timeout happened. Kill the process and throw an exception.
                    string line = proc.StandardOutput.ReadLine();
                    result = Convert.ToDouble(line);
                    tcs.TrySetResult(result);
                    proc.Kill();
                    throw new TimeoutException($"Process exceeded the timeout of {timeout.TotalMilliseconds} milliseconds. Calculated result at the moment is {result}");
                }
            }
        }
    }
}
