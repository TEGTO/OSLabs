using ClassLibrary;
using System.Text.Json;

namespace TransferTickets
{
    class Program
    {
        const int AMOUNT_OF_TICKETS = 100;
        const int TRANSFER_PERCENT = 5;
        const int TRANSGER_ON_PERCENT = 80; // Переводить білети, якщо один із процесів з'являвся частіше n-го відсотка на даний момент
        private static void Main()
        {
            CreateProcesses();
            Ticket[] tickets = new Ticket[AMOUNT_OF_TICKETS];
            for (int i = 0; i < tickets.Length; i++)
                tickets[i] = new Ticket();
            string json = File.ReadAllText("processes.json");
            var processes = JsonSerializer.Deserialize<MyProcess[]>(json);
            int mutualPriority = 0;
            Dictionary<MyProcess, List<Ticket>> ticketList = new Dictionary<MyProcess, List<Ticket>>();
            if (processes == null || processes.Length <= 0)
                return;
            foreach (var process in processes)
                mutualPriority += process.Priority;
            int startIndex = 0;
            foreach (var process in processes)
            {
                ticketList[process] = new List<Ticket>();
                int amountOfTicketsForProcess = Ticket.CountAmountTicketsByPriority(mutualPriority, process.Priority, tickets.Length, startIndex);
                for (int i = startIndex; i < startIndex + amountOfTicketsForProcess; i++)
                {
                    ticketList[process].Add(tickets[i]);
                    tickets[i].MyProcess = process;
                }
                startIndex = (int)amountOfTicketsForProcess;
            }
            int amountOfTransfers = 0;
            for (int i = 0; i < tickets.Length; i++)
            {
                int random = new Random().Next(0, tickets.Length);
                tickets[random].MyProcess.StartProcess();
                MyProcess processMostCommon = processes.MaxBy(x => x.ScheduledTimes);
                if (processMostCommon.ScheduledTimes * 100 / (i + 1) > TRANSGER_ON_PERCENT)
                {
                    amountOfTransfers++;
                    MyProcess processMostRarest = processes.MinBy(x => x.ScheduledTimes);
                    int transferAmount = ticketList[processMostCommon].Count * TRANSFER_PERCENT / 100;
                    TransferTickets(ticketList, processMostCommon, processMostRarest, transferAmount);
                }
            }
            Console.WriteLine("\n\n####ANALYSIS####\n");
            Console.WriteLine($"Amount of transfers: {amountOfTransfers}");
            for (int i = 0; i < processes.Length; i++)
            {
                Console.WriteLine(processes[i]);
            }
        }
        public static void CreateProcesses()
        {
            var processes = new MyProcess[]
            {
                new MyProcess { Name = "Process1", BurstTime = 10.5, Priority = 1 },
                new MyProcess { Name = "Process2", BurstTime = 8.0, Priority = 2 },
            };
            string json = JsonSerializer.Serialize(processes);
            File.WriteAllText("processes.json", json);
        }
        public static void TransferTickets(Dictionary<MyProcess, List<Ticket>> dictionary, MyProcess key1, MyProcess key2, int count)
        {
            if (dictionary.ContainsKey(key1) && dictionary.ContainsKey(key2))
            {
                List<Ticket> list1 = dictionary[key1];
                List<Ticket> list2 = dictionary[key2];
                if (count <= list1.Count)
                {
                    List<Ticket> transferredItems = list1.GetRange(0, count);
                    for (int i = 0; i < transferredItems.Count; i++)
                        transferredItems[i].MyProcess = key2;
                    list2.AddRange(transferredItems);
                    list1.RemoveRange(0, count);
                }
                else
                {
                    // Handle the case where you want to transfer more elements than available
                    Console.WriteLine("Not enough elements to transfer.");
                }
            }
            else
            {
                // Handle the case where one or both keys do not exist in the dictionary
                Console.WriteLine("One or both keys do not exist in the dictionary.");
            }
        }
    }
}