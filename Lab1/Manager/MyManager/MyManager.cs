using ClassLibrary.Process;
using Lab1.Function;
using Lab1.MyIO;
using System.Diagnostics;
using System.Globalization;

namespace Lab1.Manager
{
    class MyManager
    {
        private const float WAIT_TIME = 5000;
        private const string FIRST_FUN_NAME = "Fun1";
        private const string SECOND_FUN_NAME = "Fun2";
        public Action UserCancelEvent;
        public Action ShowFunInfoEvent;
        private Action CancelEvent;
        private string typeFunctionA;
        private string typeFunctionB;
        private string processName;
        private string functionsAssemblyName;
        private bool isCanCalculateResult = true;
        private int x;
        private ProcessReport resultIfError = null;
        private Dictionary<int, string> memoizedResults = new Dictionary<int, string>();
        public MyManager(IFunctionBase functionA, IFunctionBase functionB, int x, string processName, string functionsAssemblyName = "")
        {
            this.typeFunctionA = functionA.GetType().ToString();
            this.typeFunctionB = functionB.GetType().ToString();
            this.x = x;
            this.processName = processName;
            this.functionsAssemblyName = functionsAssemblyName;
        }
        public void SetFunctions(IFunctionBase functionA, IFunctionBase functionB, string functionsAssemblyName = "")
        {
            this.typeFunctionA = functionA.GetType().ToString();
            this.typeFunctionB = functionB.GetType().ToString();
            this.functionsAssemblyName = functionsAssemblyName;
        }
        public void SetX(int x) => this.x = x;
        public async Task<string> GetComputedResult()
        {
            UserCancelEvent = null;
            ShowFunInfoEvent = null;
            isCanCalculateResult = true;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            ProcessReport resultFirst = null, resultSecond = null;
            double finalResult = double.NaN;
            string nameFirst = FIRST_FUN_NAME, nameSecond = SECOND_FUN_NAME, txt = CheckIfMemoized();
            bool isCanceledByUser = false;
            UserCancelEvent += () => { isCanceledByUser = true; CancelEvent?.Invoke(); CancelEvent = null; };
            if (string.IsNullOrEmpty(txt))
            {
                memoizedResults.Add(x, string.Empty);
                var taskFirst = FunCompute(nameFirst, typeFunctionA, x, functionsAssemblyName: functionsAssemblyName);
                var taskSecond = FunCompute(nameSecond, typeFunctionB, x, functionsAssemblyName: functionsAssemblyName);
                // Await both tasks to complete
                //taskFirst.Wait();
                //taskSecond.Wait();
                //resultFirst = taskFirst.Result;
                //resultSecond = taskSecond.Result;
                resultFirst = await taskFirst;
                resultSecond = await taskSecond;
                if (isCanCalculateResult)
                {
                    finalResult = resultFirst.Result + resultSecond.Result;
                    txt = $"Result of {nameFirst} + {nameSecond}: " + finalResult.ToString();
                }
                else
                    txt = $"Result of {nameFirst} + {nameSecond} can't be caulculated!";
                if (!isCanceledByUser)
                    memoizedResults[x] += txt + "\n";
                else
                    memoizedResults.Remove(x);
            }
            return txt;
        }
        private string CheckIfMemoized()
        {
            string txt = string.Empty;
            if (memoizedResults.ContainsKey(x))
                txt = memoizedResults[x];
            return txt;
        }
        private async Task<ProcessReport> FunCompute(string name, string funType, int x, string functionsAssemblyName)
        {
            resultIfError = null;
            ProcessReport result = null;
            string txt = string.Empty;
            try
            {
                var task = CalculateFunProcessAsync(funType, x, functionsAssemblyName: functionsAssemblyName);
                result = await task;
                txt = $"\n{name} result: {result}";
                IORedirector.PrintLineStandartOut(txt);
            }
            catch (TimeoutException ex)
            {
                txt += $"Timeout Error occured in the {name}: " + ex.Message;
                if (resultIfError != null)
                {
                    txt += $"{name} result at the moment: {resultIfError}";
                    result = resultIfError;
                }
                else
                    isCanCalculateResult = false;
                IORedirector.PrintError(txt);
            }
            catch (TaskCanceledException ex)
            {
                txt += $"{name} was cancelled: " + ex.Message;
                IORedirector.PrintError(txt);
                isCanCalculateResult = false;
            }
            catch (Exception ex)
            {
                CancelEvent?.Invoke();
                txt += $"Error occured in the {name}: " + ex.Message;
                if (resultIfError != null)
                    txt += $"{name} result at the moment: {resultIfError}";
                IORedirector.PrintError(txt);
                isCanCalculateResult = false;
            }
            memoizedResults[x] += txt + "\n";
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
                Action killProcess = () =>
                {
                    proc?.Kill();
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
                Action cancelProcess = () =>
                {
                    killProcess?.Invoke();
                    tcs.TrySetCanceled(new CancellationToken());
                };
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
                    {
                        CancelEvent -= cancelProcess;
                        ShowFunInfoEvent -= showInfo;
                        tcs.TrySetResult(ReadLastLine(consoleReadText));
                    }
                    else
                    {
                        CancelEvent -= cancelProcess;
                        ShowFunInfoEvent -= showInfo;
                        var errorMessage = proc.StandardError.ReadToEnd();
                        resultIfError = ReadLastLine(consoleReadText);
                        tcs.TrySetException(new InvalidOperationException("The process did not exit correctly. " +
                            "The corresponding error message was: " + errorMessage));
                    }
                };
                proc.Start();
                CancelEvent += cancelProcess;
                ShowFunInfoEvent += showInfo;
                // This is the timeout mechanism
                if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)) != tcs.Task)
                {
                    CancelEvent -= cancelProcess;
                    ShowFunInfoEvent -= showInfo;
                    killProcess?.Invoke();
                    // Timeout happened. Kill the process and throw an exception.
                    tcs.TrySetException(new TimeoutException($"Process exceeded the timeout of {timeout.TotalMilliseconds} milliseconds."));
                }
                return await tcs.Task;
            }
        }
    }
}
