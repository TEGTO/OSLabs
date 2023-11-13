using ClassLibrary;
using System.Text.Json.Serialization;

namespace TicketCurrency
{
    public class User
    {
        public string Name { get; set; }
        public int UserPriority { get; set; }
        [JsonInclude]
        public List<MyProcess> Processes;
        public Ticket[] tickets;
        [JsonIgnore]
        public Ticket[] Tickets
        {
            get
            {
                return tickets;
            }
            set
            {
                tickets = value;
                int mutualPriority = 0;
                List<MyProcess> unfinishedProcesses = Processes.FindAll(x => !x.IsFinished);
                foreach (var process in unfinishedProcesses)
                    mutualPriority += process.Priority;
                int startIndex = 0;
                foreach (var process in unfinishedProcesses)
                {
                    int amountOfTicketsForProcess = Ticket.CountAmountTicketsByPriority(mutualPriority, process.Priority, tickets.Length, startIndex);
                    for (int i = startIndex; i < startIndex + amountOfTicketsForProcess; i++)
                        tickets[i].MyProcess = process;
                    startIndex += amountOfTicketsForProcess;
                }
            }
        }
        public override string ToString()
        {
            return $"==User: {Name} | UserPriority: {UserPriority} | Amount of Tickets at the end: {Tickets.Length}==";
        }
    }
}
