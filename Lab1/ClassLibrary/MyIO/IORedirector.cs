﻿using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Lab1.MyIO
{
    public class IORedirector
    {
        private const string INPUT_FILE = "input.txt";
        private const string OUTPUT_FILE = "output.txt";
        private const string ERROR_FILE = "errors.txt";
        private static TextReader standartTextReader;
        private static TextWriter standartTextWriter;
        private static Microsoft.Extensions.Logging.ILogger<Lab1.MyIO.IORedirector> logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<IORedirector>();
        public static void Print(string s, string pathName = OUTPUT_FILE, bool append = false, bool loggerPrint = true)
        {
            try
            {
                if (standartTextWriter == null)
                    standartTextWriter = Console.Out;
                if (loggerPrint)
                    logger.LogInformation($"Wrote {s} to the {pathName}!");
                using (var sw = new StreamWriter(pathName, append))
                {
                    Console.SetOut(sw);
                    Console.WriteLine(s);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Read error: {ex.Message}");
            }
        }
        public static void PrintError(string s, string pathName = ERROR_FILE, bool append = false)
        {
            logger.LogError($"Error message: {s}");
            Print(s, pathName, append, false);
        }
        public static void PrintLineStandartOut(string s)
        {
            try
            {
                if (standartTextWriter != null)
                    Console.SetOut(standartTextWriter);
                Console.WriteLine(s);
            }
            catch (Exception ex)
            {
                logger.LogError($"Read error: {ex.Message}"); ;
            }
        }
        public static string ReadLine(string pathName = INPUT_FILE, bool loggerPrint = true, bool isReadLastLine = false, bool printErrorIfFileNotFound = true) =>
            Read(pathName: pathName, loggerPrint: loggerPrint, isReadLine: true, isReadLastLine: isReadLastLine, printErrorIfFileNotFound: printErrorIfFileNotFound);
        public static List<string> ReadLines(string pathName = INPUT_FILE, bool printErrorIfFileNotFound = true)
        {
            List<string> linesList;
            linesList = Read(pathName: pathName, loggerPrint: false, isReadLine: false, printErrorIfFileNotFound: printErrorIfFileNotFound)?.Split("\n").ToList();
            return linesList;
        }
        public static string Read(string pathName, bool loggerPrint = true, bool printErrorIfFileNotFound = true, bool isReadLine = false, bool isReadLastLine = false)
        {
            try
            {
                if (standartTextReader == null)
                    standartTextReader = Console.In;
                using (var sr = new StreamReader(pathName))
                {
                    if (loggerPrint)
                        logger.LogInformation($"Read string line from the {pathName}!");
                    Console.SetIn(sr);
                    string currentLine;
                    string lastLine = string.Empty;
                    if (!isReadLine)
                        return sr.ReadToEnd();
                    while ((currentLine = Console.ReadLine()) != null)
                    {
                        if (!isReadLastLine)
                            return currentLine;
                        lastLine = currentLine;
                    }
                    return lastLine;
                }
            }
            catch (FileNotFoundException ex)
            {
                if (printErrorIfFileNotFound)
                    logger.LogError($"Read error: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Read error: {ex.Message}");
            }
            return string.Empty;
        }
        public static string ReadLineStandartIn()
        {
            try
            {
                string line;
                if (standartTextReader != null)
                    Console.SetIn(standartTextReader);
                line = Console.ReadLine();
                return line;
            }
            catch (Exception ex)
            {
                logger.LogError($"Read error: {ex.Message}");
                throw;
            }
        }
    }
}
