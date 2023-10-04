using ClassLibrary.Function;
using ClassLibrary.Process;
using Lab1.MyIO;
using System.Diagnostics;

namespace Lab1.Manager
{
    class MyManager
    {
        private const float WAIT_TIME = 2000;
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
            double resultFirst = double.NaN, resultSecond = double.NaN, computingResult = double.NaN;
            string amountOfSoftErrorsFirst = "0", amountOfSoftErrorsSecond = "0";
            bool isSuccess = true;
            string txt;
            if (!CheckIfMemoized(x, "result.txt"))
            {
                try
                {
                    resultFirst = await CalculateFunProcessAsync(typeFunctionA, x, "softErrors1.txt", functionsAssemblyName: functionsAssemblyName);
                    amountOfSoftErrorsFirst = IORedirector.ReadLine("softErrors1.txt");
                    txt = $"Fun1 result: {resultFirst}. Amount of soft errors occured: {amountOfSoftErrorsFirst}";
                    IORedirector.PrintLineStandartOut(txt);
                }
                catch (Exception ex)
                {
                    // IORedirector.PrintLineStandartOut("Error occured in the first process: " + ex.Message);
                    IORedirector.PrintError("Error occured in the first process: " + ex.Message, append: true);
                    isSuccess = false;
                }
                if (isSuccess)
                {
                    try
                    {
                        resultSecond = await CalculateFunProcessAsync(typeFunctionB, x, "softErrors2.txt", functionsAssemblyName: functionsAssemblyName);
                        amountOfSoftErrorsSecond = IORedirector.ReadLine("softErrors2.txt");
                        txt = $"Fun2 result: {resultSecond}. Amount of soft errors occured: {amountOfSoftErrorsSecond}";
                        IORedirector.PrintLineStandartOut(txt);
                    }
                    catch (Exception ex)
                    {
                        // IORedirector.PrintLineStandartOut("Error occured in the first process: " + ex.Message);
                        IORedirector.PrintError("Error occured in the second process: " + ex.Message, append: true);
                        isSuccess = false;
                    }
                }
                if (isSuccess)
                {
                    computingResult = resultFirst + resultSecond;
                    IORedirector.PrintLineStandartOut("Result of Fun1 + Fun2: " + computingResult.ToString());
                }
                else
                    IORedirector.PrintLineStandartOut("Error occured in one of the functions, result of Fun1 + Fun2: " + computingResult);
                IORedirector.Print($"{x} {computingResult} {amountOfSoftErrorsFirst} {amountOfSoftErrorsSecond}", "result.txt", append: true, false);
            }
        }
        private bool CheckIfMemoized(int x, string resultPathName)
        {
            double memoizedResult = double.NaN;
            List<string> results = IORedirector.ReadLines(pathName: resultPathName, printErrorIfFileNotFound: false);
            if (results?.Count <= 0)
                return false;
            string[] memoizedStringRes;
            memoizedStringRes = results.Find(line => line.Split(" ")[0].Equals(x.ToString()))?.Split(" ");
            if (memoizedStringRes != null)
            {
                double.TryParse(memoizedStringRes[1].ToString(), out memoizedResult);
                IORedirector.PrintLineStandartOut($"Fun1 amount of soft errors occured: {memoizedStringRes[2]}");
                IORedirector.PrintLineStandartOut($"Fun2 amount of soft errors occured: {memoizedStringRes[3]}");
                IORedirector.PrintLineStandartOut("Result of Fun1 + Fun2: " + memoizedResult.ToString());
                return true;
            }
            return false;
        }
        private async Task<double> CalculateFunProcessAsync(string funType, int x, string softErrorsFile, string functionsAssemblyName)
        {
            double result = double.NaN;
            string lastLine = string.Empty;
            var tcs = new TaskCompletionSource<double>();
            var timeout = TimeSpan.FromMilliseconds(WAIT_TIME); // desired timeout value
            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = processName,
                    UseShellExecute = false,
                    Arguments = $"{funType} {x} {softErrorsFile} {functionsAssemblyName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                proc.EnableRaisingEvents = true;
                proc.Exited += (sender, args) =>
                {
                    if (proc.ExitCode != (int)ProcessStatus.COMPUTING_SUCCESS)
                    {
                        var errorMessage = proc.StandardError.ReadToEnd();
                        tcs.TrySetException(new InvalidOperationException("The process did not exit correctly. " +
                            "The corresponding error message was: " + errorMessage));
                    }
                    else
                    {
                        while (!proc.StandardOutput.EndOfStream)
                            lastLine = proc.StandardOutput.ReadLine();
                        double.TryParse(lastLine, out result);
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
                    //string line = proc.StandardOutput.ReadLine();
                    proc.Kill();
                    while (!proc.StandardOutput.EndOfStream)
                        lastLine = proc.StandardOutput.ReadLine();
                    double.TryParse(lastLine, out result);
                    tcs.TrySetResult(result);
                    throw new TimeoutException($"Process exceeded the timeout of {timeout.TotalMilliseconds} milliseconds. Calculated result at the moment is {result}");
                }
            }
        }
    }
}
