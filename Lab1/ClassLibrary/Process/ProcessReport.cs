using Lab1.MyIO;
using Newtonsoft.Json;

namespace ClassLibrary.Process
{
    public class ProcessReport
    {
        public double Result;
        public int AmountOfSoftErrors;
        public int AmountOfHardErrors;
        public bool IsOperationInterrupted = false;
        public ProcessReport()
        {
        }
        public ProcessReport(double result, int amountOfSoftErrors, int amountOfHardErrors, bool isOperationInterrupted)
        {
            this.Result = result;
            this.AmountOfSoftErrors = amountOfSoftErrors;
            this.AmountOfHardErrors = amountOfHardErrors;
            this.IsOperationInterrupted = isOperationInterrupted;
        }
        public void Save(string pathName, bool append = false, bool loggerPrint = false)
        {
            string json = JsonConvert.SerializeObject(this);
            IORedirector.Print(json, pathName, append, loggerPrint);
        }
        public static ProcessReport Load(string pathName)
        {
            string json = IORedirector.Read(pathName: pathName, loggerPrint: false, printErrorIfFileNotFound: true, isReadLine: false);
            if (!string.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<ProcessReport>(json);
            return null;
        }
    }
}
