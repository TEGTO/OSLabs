using ClassLibrary.Function;
using ClassLibrary.Process;
using Lab1.MyIO;
using System.Diagnostics;
using System.Globalization;

namespace Lab1.Manager
{

    class MyManager
    {
        private const float WAIT_TIME = 3500;
        private string typeFunctionA;
        private string typeFunctionB;
        private string processName;
        private string functionsAssemblyName;
        private bool isSuccess = true;
        private int x;
        private ProcessReport resultIfError = null;
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            ProcessReport resultFirst = null, resultSecond = null;
            double finalResult = double.NaN;
            isSuccess = true;
            string txt, nameFirst = "Fun1", nameSecond = "Fun2";
            if (!CheckIfMemoized(nameFirst, nameSecond, x, "result.txt"))
            {
                resultFirst = await FunCompute(nameFirst, typeFunctionA, x, functionsAssemblyName: functionsAssemblyName);
                // if (isSuccess)
                resultSecond = await FunCompute(nameSecond, typeFunctionB, x, functionsAssemblyName: functionsAssemblyName);
                if (isSuccess)
                {
                    finalResult = resultFirst.Result + resultSecond.Result;
                    IORedirector.PrintLineStandartOut($"Result of {nameFirst} + {nameSecond}: " + finalResult.ToString());
                }
                else
                    IORedirector.PrintLineStandartOut($"Critical error occured in one of the functions, result of {nameFirst} + {nameSecond} can't be caulculated");
                IORedirector.Print($"{x}◙{isSuccess}◙{finalResult}◙{ProcessReport.ProcessReportSerialize(resultFirst)}◙{ProcessReport.ProcessReportSerialize(resultSecond)}", "result.txt", append: true, false);
            }
        }
        private async Task<ProcessReport> FunCompute(string name, string funType, int x, string functionsAssemblyName)
        {
            ProcessReport result = null;
            resultIfError = null;
            string txt;
            try
            {
                result = await CalculateFunProcessAsync(funType, x, functionsAssemblyName: functionsAssemblyName);
                txt = $"\n{name} result: {result}";
                IORedirector.PrintLineStandartOut(txt);
            }
            catch (TimeoutException ex)
            {
                IORedirector.PrintError($"Timeout Error occured in the {name}: " + ex.Message, append: true);
                if (resultIfError != null)
                {
                    txt = $"{name} result at the moment: {resultIfError}";
                    IORedirector.PrintLineStandartOut(txt);
                    result = resultIfError;
                }
            }
            catch (Exception ex)
            {
                // IORedirector.PrintLineStandartOut("Error occured in the first process: " + ex.Message);
                IORedirector.PrintError($"Error occured in the {name}: " + ex.Message, append: true);
                isSuccess = false;
            }
            return result;
        }
        private bool CheckIfMemoized(string nameFirst, string nameSecond, int x, string resultPathName)
        {
            double memoizedResult = double.NaN;
            ProcessReport resultFirst = null, resultSecond = null;
            bool isSuccess = true;
            List<string> results = IORedirector.ReadLines(pathName: resultPathName, printErrorIfFileNotFound: false);
            if (results?.Count <= 0)
                return false;
            string[] memoizedStringRes;
            memoizedStringRes = results.Find(line => line.Split("◙")[0].Equals(x.ToString()))?.Split("◙");
            if (memoizedStringRes != null)
            {
                if (memoizedStringRes.Length <= 3 || !double.TryParse(memoizedStringRes[2], out memoizedResult))
                    memoizedResult = double.NaN;
                else double.TryParse(memoizedStringRes[2], out memoizedResult);
                try
                {
                    bool.TryParse(memoizedStringRes[1], out isSuccess);
                    resultFirst = ProcessReport.ProcessReportDeserialize(memoizedStringRes[3]);
                    resultSecond = ProcessReport.ProcessReportDeserialize(memoizedStringRes[4]);
                }
                catch
                {
                }
                finally
                {
                    string txt = string.Empty;
                    txt += $"\n{nameFirst} result: ";
                    txt += resultFirst != null ? resultFirst + "\n" : "undefined\n";
                    txt += $"{nameSecond} result: ";
                    txt += resultSecond != null ? resultSecond + "\n" : "undefined\n";
                    txt += isSuccess ? "Result of {nameFirst} + {nameSecond}: " + memoizedResult.ToString()
                        : $"Critical error occured in one of the functions, result of {nameFirst} + {nameSecond} can't be caulculated";
                    IORedirector.PrintLineStandartOut(txt);
                }
                return true;
            }
            return false;
        }
        private async Task<ProcessReport> CalculateFunProcessAsync(string funType, int x, string functionsAssemblyName)
        {
            string line, lastLine = null;
            string consoleReadText = string.Empty;
            var tcs = new TaskCompletionSource<ProcessReport>();
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
                    if (proc.ExitCode == (int)ProcessStatus.COMPUTING_SUCCESS || proc.ExitCode == (int)ProcessStatus.COMPUTING_SOFT_ERROR)
                    {
                        if (proc.StandardOutput.Peek() != -1)
                            consoleReadText = proc.StandardOutput.ReadToEnd();
                        tcs.TrySetResult(ReadLastLine(consoleReadText));
                    }
                    else
                    {
                        var errorMessage = proc.StandardError.ReadToEnd();
                        if (proc.StandardOutput.Peek() != -1)
                            consoleReadText = proc.StandardOutput.ReadToEnd();
                        resultIfError = ReadLastLine(consoleReadText);
                        tcs.TrySetException(new InvalidOperationException("The process did not exit correctly. " +
                            "The corresponding error message was: " + errorMessage));
                    }
                };
                proc.Start();
                // This is the timeout mechanism
                if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)) != tcs.Task)
                {
                    // Timeout happened. Kill the process and throw an exception.
                    proc.Kill();
                    tcs.TrySetException(new TimeoutException($"Process exceeded the timeout of {timeout.TotalMilliseconds} milliseconds."));
                    if (proc.StandardOutput.Peek() != -1)
                        consoleReadText = proc.StandardOutput.ReadToEnd();
                    resultIfError = ReadLastLine(consoleReadText);
                }
                return await tcs.Task;
            }
        }
        private ProcessReport ReadLastLine(string read)
        {
            ProcessReport processReport = null;
            if (!string.IsNullOrEmpty(read))
            {
                string[] arr = read.Split("\n");
                string lastLine = null;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arr[i]))
                        lastLine = arr[i];
                }
                if (!string.IsNullOrEmpty(lastLine))
                {
                    try
                    {
                        processReport = ProcessReport.ProcessReportDeserialize(lastLine);
                    }
                    catch (Exception)
                    {
                        processReport = null;
                    }
                }
            }
            return processReport;
        }
    }
}
