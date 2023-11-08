
using ClassLibrary;
using System.Text.Json;
using TicketInflation;

class Program
{
    const int AMOUNT_OF_TICKETS = 100;
    private static void Main()
    {
        //CreateProcesses();
        Ticket[] tickets = new Ticket[AMOUNT_OF_TICKETS];
        for (int i = 0; i < tickets.Length; i++)
            tickets[i] = new Ticket();
        string json = File.ReadAllText("inflation_processes.json");
        var processes = JsonSerializer.Deserialize<InflationProcess[]>(json);
        if (processes == null || processes.Length <= 0)
            return;
        InflationTicketMachine ticketMachine = new InflationTicketMachine();
        ticketMachine.MaxAmountOfTickets = AMOUNT_OF_TICKETS;
        ticketMachine.Processes = processes;
        int mutualPriority = 0;
        foreach (var process in processes)
            mutualPriority += process.Priority;
        int startIndex = 0;
        foreach (var process in processes)
        {
            int amountOfTicketsForProcess = Ticket.CountAmountTicketsByPriority(mutualPriority, process.Priority, tickets.Length, startIndex);
            Ticket[] currentTickets = new Ticket[amountOfTicketsForProcess];
            int localIndex = 0;
            for (int i = startIndex; i < startIndex + amountOfTicketsForProcess; i++)
            {
                tickets[i].MyProcess = process;
                currentTickets[localIndex] = tickets[i];
                localIndex++;
            }
            process.Tickets = currentTickets;
            process.TicketMachine = ticketMachine;
            startIndex = (int)amountOfTicketsForProcess;
        }
        for (int i = 0; i < tickets.Length; i++)
            tickets[new Random().Next(0, tickets.Length)].MyProcess.StartProcess();
        Console.WriteLine("\n\n####ANALYSIS####\n");
        Console.WriteLine($"Amount of requests: {ticketMachine.AmountOfRequests}");
        for (int i = 0; i < processes.Length; i++)
            Console.WriteLine(processes[i]);
    }
    public static void CreateProcesses()
    {
        var processes = new InflationProcess[]
        {
                new InflationProcess { Name = "Process1", BurstTime = 10.5, Priority = 1, RequestChance = 10 },
                new InflationProcess { Name = "Process2", BurstTime = 8.0, Priority = 2, RequestChance = 20 },
        };
        string json = JsonSerializer.Serialize(processes);
        File.WriteAllText("inflation_processes.json", json);
    }
}