﻿using Lab1.MyIO;
using Newtonsoft.Json;

namespace ClassLibrary.Process
{
    public class ProcessReport
    {
        public string FunType;
        public double Result;
        public int AmountOfSoftErrors;
        public int AmountOfHardErrors;
        public bool IsOperationInterrupted = false;
        public ProcessReport()
        {
        }
        public ProcessReport(string funType, double result, int amountOfSoftErrors, int amountOfHardErrors, bool isOperationInterrupted)
        {
            this.FunType = funType;
            this.Result = result;
            this.AmountOfSoftErrors = amountOfSoftErrors;
            this.AmountOfHardErrors = amountOfHardErrors;
            this.IsOperationInterrupted = isOperationInterrupted;
        }
        public static ProcessReport ProcessReportDeserialize(string s) =>
           JsonConvert.DeserializeObject<ProcessReport>(s);
        public static string ProcessReportSerialize(ProcessReport processReport) =>
            JsonConvert.SerializeObject(processReport);
        public void Save(string pathName, bool append = false)
        {
            string json = ProcessReportSerialize(this);
            IORedirector.Print(json, pathName, append);
        }
        public static ProcessReport Load(string pathName)
        {
            string json = IORedirector.Read(pathName: pathName, printErrorIfFileNotFound: true, isReadLine: false);
            if (!string.IsNullOrEmpty(json))
                return ProcessReportDeserialize(json);
            return null;
        }
        public override string ToString()
        {
            return $"Result: {Result}, AmountOfSoftErrors: {AmountOfSoftErrors}, AmountOfHardErrors: {AmountOfHardErrors}, IsOperationInterrupted: {IsOperationInterrupted}";
        }
    }
}
