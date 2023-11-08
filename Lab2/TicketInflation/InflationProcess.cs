using ClassLibrary;

namespace TicketInflation
{
    public class InflationProcess : MyProcess
    {
        public Ticket[] Tickets;
        public InflationTicketMachine TicketMachine;
        public int RequestChance { get; set; }
        public override void StartProcess()
        {
            if (new Random().Next(100) < RequestChance)
            {
                int amountOfTicketsToRequest = new Random().Next(TicketMachine.MaxAmountOfTickets - Tickets.Length);
                Ticket[] requstedTickets = TicketMachine.RequestMoreTickets(amountOfTicketsToRequest, this);
                Tickets = requstedTickets.Concat(Tickets).ToArray();
                Console.WriteLine($"$$$$Request of {amountOfTicketsToRequest} tickets from {Name}. Got {requstedTickets.Length} tickets.$$$$");
            }
            base.StartProcess();
        }
    }
}
