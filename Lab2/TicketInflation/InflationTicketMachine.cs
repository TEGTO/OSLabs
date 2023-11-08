using ClassLibrary;

namespace TicketInflation
{
    public class InflationTicketMachine
    {
        public int MaxAmountOfTickets;
        public int AmountOfRequests = 0;
        public InflationProcess[] Processes;
        public Ticket[] RequestMoreTickets(int amountOfTickets, InflationProcess newProcess)
        {
            AmountOfRequests++;
            InflationProcess processMostCommon = Processes.MaxBy(x => x.ScheduledTimes);
            Ticket[] takenTickets;
            if (processMostCommon.Tickets.Length > amountOfTickets)
            {
                takenTickets = processMostCommon.Tickets.Take(amountOfTickets).ToArray();
                processMostCommon.Tickets = processMostCommon.Tickets.Skip(amountOfTickets).Take(processMostCommon.Tickets.Length - amountOfTickets).ToArray();
            }
            else
            {
                takenTickets = processMostCommon.Tickets;
                processMostCommon.Tickets = new Ticket[0];
            }
            for (int i = 0; i < takenTickets.Length; i++)
                takenTickets[i].MyProcess = newProcess;
            return takenTickets;
        }
    }
}
