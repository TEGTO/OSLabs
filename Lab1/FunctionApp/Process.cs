using ClassLibrary.Function;
using ClassLibrary.Process;
using Lab1.MyIO;

class Process
{
    const int MAX_AMOUT_OF_SOFT_ERRORS = 3;
    private static void Main(string[] args)
    {
        //FunctionA f = new FunctionA();
        //args = new string[3] { f.GetType().ToString(), "5","ClassLibrary" };
        double calcResult = double.NaN;
        ProcessReport processReport = new ProcessReport();
        if (args.Length < 2)
        {
            Environment.ExitCode = (int)ProcessStatus.COMPUTING_HARD_ERROR;
            processReport.CalculationStatus = ProcessStatus.COMPUTING_HARD_ERROR;
            processReport.Result = double.NaN;
            IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
            throw new Exception("Not enough arguments!");
        }
        else
        {
            string funType = args.Length >= 3 ? $"{args[0]}, {args[2]}" : args[0];
            string x = args[1];
            processReport.FunType = funType;
            if (int.TryParse(x, out int intX))
            {
                bool isCorrectCalc = false;
                processReport.CalculationStatus = ProcessStatus.COMPUTING_SUCCESS;
                while (processReport.AmountOfSoftErrors <= MAX_AMOUT_OF_SOFT_ERRORS && !isCorrectCalc)
                {
                    try
                    {
                        isCorrectCalc = true;
                        calcResult = FunCalculate(funType, intX);
                        processReport.Result = calcResult;
                        IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                    }
                    catch (InvalidOperationException)
                    {
                        isCorrectCalc = false;
                        processReport.AmountOfSoftErrors++;
                        if (processReport.AmountOfSoftErrors > MAX_AMOUT_OF_SOFT_ERRORS)
                        {
                            processReport.CalculationStatus = ProcessStatus.COMPUTING_SOFT_ERROR;
                            IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                            Environment.ExitCode = (int)ProcessStatus.COMPUTING_SOFT_ERROR;
                            throw new Exception($"Too many soft errors: {processReport.AmountOfSoftErrors}! Calculations not possible!");
                        }
                        IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                    }
                    catch (Exception ex)
                    {
                        processReport.CalculationStatus = ProcessStatus.COMPUTING_HARD_ERROR;
                        IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                        Environment.ExitCode = (int)ProcessStatus.COMPUTING_HARD_ERROR;
                        throw ex;
                    }
                }
                IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                if (double.IsNaN(calcResult))
                {
                    processReport.Result = double.NaN;
                    processReport.CalculationStatus = ProcessStatus.COMPUTING_UNDEFINED;
                    IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                    Environment.ExitCode = (int)ProcessStatus.COMPUTING_UNDEFINED;
                    throw new Exception($"Undefined result! Amount of soft errors: {processReport.AmountOfSoftErrors}");
                }
                Environment.ExitCode = (int)ProcessStatus.COMPUTING_SUCCESS;
                IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
            }
            else
            {
                processReport.Result = double.NaN;
                processReport.CalculationStatus = ProcessStatus.COMPUTING_UNDEFINED;
                IORedirector.PrintLineStandartOut(ProcessReport.ProcessReportSerialize(processReport));
                Environment.ExitCode = (int)ProcessStatus.COMPUTING_UNDEFINED;
                throw new Exception("Can not parse x to int!");
            }
        }
    }
    private static double FunCalculate(string FunType, int x)
    {
        double result = double.NaN; ;
        Type type = Type.GetType(FunType);
        object convertedFun = Activator.CreateInstance(type);
        FunctionBase fun = (FunctionBase)convertedFun;
        result = fun.MakeCalculations(x);
        //if (double.IsNaN(result))
        //    throw new InvalidOperationException();
        return result;
    }
}