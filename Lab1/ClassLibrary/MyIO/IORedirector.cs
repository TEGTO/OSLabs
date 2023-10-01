using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Lab1.MyIO
{
    public class IORedirector
    {
        const string INPUT_FILE = "input.txt";
        const string OUTPUT_FILE = "output.txt";
        const string ERROR_FILE = "errors.txt";
        private static TextReader standartTextReader;
        private static TextWriter standartTextWriter;
        private static Microsoft.Extensions.Logging.ILogger<Lab1.MyIO.IORedirector> logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<IORedirector>();
        public static void Print(string s, string pathName = OUTPUT_FILE, bool append = false)
        {
            try
            {
                if (standartTextWriter == null)
                    standartTextWriter = Console.Out;
                logger.LogInformation($"Wrote string to the {pathName}!");
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
            Print(s, pathName, append);
        }
        public static void PrintLineConsole(string s)
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
        public static string ReadLine(string pathName = INPUT_FILE)
        {
            try
            {
                if (standartTextReader == null)
                    standartTextReader = Console.In;
                using (var sr = new StreamReader(pathName))
                {
                    logger.LogInformation($"Read string line from the {pathName}!");
                    Console.SetIn(sr);
                    string line;
                    while ((line = Console.ReadLine()) != null)
                        return line;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Read error: {ex.Message}");
            }
            return string.Empty;
        }
        public static List<string> ReadLines(string pathName = INPUT_FILE)
        {
            List<string> linesList = new List<string>();
            string line;
            do
            {
                line = ReadLine(pathName);
                if (!string.IsNullOrEmpty(line))
                    linesList.Add(line);
            } while (!string.IsNullOrEmpty(line));
            return linesList;
        }
        public static string ReadLineConsole()
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
