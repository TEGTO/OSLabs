using ClassLibrary.Function;
using ClassLibrary.Process;
using Lab1.MyIO;
using System.Diagnostics;
using System.Globalization;

namespace Lab1.Manager
{
    class MyManager
    {
        private const float WAIT_TIME = 5000;
        private const string SPLIT_SYMBOL = "◙";
        private const string RESULT_FILE_NAME = "result.txt";
        private const string FIRST_FUN_NAME = "Fun1";
        private const string SECOND_FUN_NAME = "Fun2";
        public Action CancelEvent;
        public Action ShowFunInfoEvent;
        private string typeFunctionA;
        private string typeFunctionB;
        private string processName;
        private string functionsAssemblyName;
        private bool isCanCalculateResult = true;
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
        public async Task<string> GetComputedResult()
        {
            CancelEvent = null;
            ShowFunInfoEvent = null;
            isCanCalculateResult = true;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            ProcessReport resultFirst = null, resultSecond = null;
            double finalResult = double.NaN;
            string nameFirst = FIRST_FUN_NAME, nameSecond = SECOND_FUN_NAME, txt = CheckIfMemoized(nameFirst, nameSecond, x, RESULT_FILE_NAME);
            bool isCanceled = false;
            CancelEvent += () => { isCanceled = true; };
            if (string.IsNullOrEmpty(txt))
            {
                var taskFirst = FunCompute(nameFirst, typeFunctionA, x, functionsAssemblyName: functionsAssemblyName);
                var taskSecond = FunCompute(nameSecond, typeFunctionB, x, functionsAssemblyName: functionsAssemblyName);
                // Await both tasks to complete
                resultFirst = await taskFirst;
                resultSecond = await taskSecond;
                if (isCanCalculateResult)
                {
                    finalResult = resultFirst.Result + resultSecond.Result;
                    txt = $"Result of {nameFirst} + {nameSecond}: " + finalResult.ToString();
                }
                else
                    txt = $"Result of {nameFirst} + {nameSecond} can't be caulculated!";
                if (!isCanceled)
                    IORedirector.Print($"{x}{SPLIT_SYMBOL}" +
                        $"{isCanCalculateResult}{SPLIT_SYMBOL}" +
                        $"{finalResult}{SPLIT_SYMBOL}" +
                        $"{ProcessReport.ProcessReportSerialize(resultFirst)}{SPLIT_SYMBOL}" +
                        $"{ProcessReport.ProcessReportSerialize(resultSecond)}", RESULT_FILE_NAME, append: true);
            }
            return txt;
        }
        private string CheckIfMemoized(string nameFirst, string nameSecond, int x, string resultPathName)
        {
            ProcessReport resultFirst = null, resultSecond = null;
            double memoizedResult = double.NaN;
            bool isSuccess = true;
            string txt = string.Empty;
            string[] memoizedStringRes;
            List<string> results = IORedirector.ReadLines(pathName: resultPathName, printErrorIfFileNotFound: false);
            if (results?.Count <= 0)
                return txt;
            memoizedStringRes = results.Find(line => line.Split(SPLIT_SYMBOL)[0].Equals(x.ToString()))?.Split(SPLIT_SYMBOL);
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
                    txt = string.Empty;
                    txt += $"\n{nameFirst} result: ";
                    txt += resultFirst != null ? resultFirst + "\n" : "undefined\n";
                    txt += $"{nameSecond} result: ";
                    txt += resultSecond != null ? resultSecond + "\n" : "undefined\n";
                    txt += isSuccess ? $"Result of {nameFirst} + {nameSecond}: " + memoizedResult.ToString()
                        : $"Result of {nameFirst} + {nameSecond} can't be calculated";
                }
                return txt;
            }
            return txt;
        }
        private async Task<ProcessReport> FunCompute(string name, string funType, int x, string functionsAssemblyName)
        {
            resultIfError = null;
            ProcessReport result = null;
            string txt;
            try
            {
                var task = CalculateFunProcessAsync(funType, x, functionsAssemblyName: functionsAssemblyName);
                result = await task;
                txt = $"\n{name} result: {result}";
                IORedirector.PrintLineStandartOut(txt);
            }
            catch (TimeoutException ex)
            {
                IORedirector.PrintError($"Timeout Error occured in the {name}: " + ex.Message);
                if (resultIfError != null)
                {
                    txt = $"{name} result at the moment: {resultIfError}";
                    IORedirector.PrintLineStandartOut(txt);
                    result = resultIfError;
                }
                else
                    isCanCalculateResult = false;
            }
            catch (Exception ex)
            {
                // IORedirector.PrintLineStandartOut("Error occured in the first process: " + ex.Message);
                IORedirector.PrintError($"Error occured in the {name}: " + ex.Message);
                if (resultIfError != null)
                {
                    txt = $"{name} result at the moment: {resultIfError}";
                    IORedirector.PrintLineStandartOut(txt);
                }
                isCanCalculateResult = false;
            }
            return result;
        }
        private ProcessReport ReadLastLine(string read)
        {
            ProcessReport processReport = null;
            if (!string.IsNullOrEmpty(read))
            {
                string lastLine = null;
                string[] arr = read.Split("\n");
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
        private async Task<ProcessReport> CalculateFunProcessAsync(string funType, int x, string functionsAssemblyName)
        {
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
                    if (proc.StandardOutput.Peek() != -1)
                        consoleReadText = proc.StandardOutput.ReadToEnd();
                    if (proc.ExitCode == (int)ProcessStatus.COMPUTING_SUCCESS)
                        tcs.TrySetResult(ReadLastLine(consoleReadText));
                    else
                    {
                        var errorMessage = proc.StandardError.ReadToEnd();
                        resultIfError = ReadLastLine(consoleReadText);
                        tcs.TrySetException(new InvalidOperationException("The process did not exit correctly. " +
                            "The corresponding error message was: " + errorMessage));
                    }
                };
                proc.Start();
                Action killProcess = () =>
                {
                    proc.Kill();
                    if (proc.StandardOutput.Peek() != -1)
                        consoleReadText = proc.StandardOutput.ReadToEnd();
                    resultIfError = ReadLastLine(consoleReadText);
                };
                Action showInfo = () =>
                {
                    if (proc.StandardOutput.Peek() != -1)
                        consoleReadText = proc.StandardOutput.ReadLine();
                    ProcessReport processReport = ReadLastLine(consoleReadText);
                    if (processReport != null)
                        IORedirector.PrintLineStandartOut(processReport.ToString());
                };
                CancelEvent += () =>
                {
                    killProcess?.Invoke();
                    tcs.TrySetCanceled(new CancellationToken());
                };
                ShowFunInfoEvent += showInfo;
                // This is the timeout mechanism
                if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)) != tcs.Task)
                {
                    killProcess?.Invoke();
                    // Timeout happened. Kill the process and throw an exception.
                    tcs.TrySetException(new TimeoutException($"Process exceeded the timeout of {timeout.TotalMilliseconds} milliseconds."));
                }
                return await tcs.Task;
            }
        }
    }
}
