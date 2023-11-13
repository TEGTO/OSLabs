namespace ClassLibrary
{
    public class Ticket
    {
        private int ticketId;
        private MyProcess myProcess;
        public int TicketId
        {
            get { return ticketId; }
        }
        public MyProcess MyProcess
        {
            get { return myProcess; }
            set { myProcess = value; }
        }
        private static int ticketsAmount = 0;
        public Ticket()
        {
            ticketsAmount++;
            ticketId = ticketsAmount;
        }
        public static int CountAmountTicketsByPriority(int mutualPriority, int priority, int ticketsLength, int startIndex)
        {
            float amountOfTicketsForProcess = (priority * 100f / mutualPriority) * ticketsLength / 100f;
            if (amountOfTicketsForProcess % 1 != 0 && ticketsLength - ((int)amountOfTicketsForProcess + startIndex + 1) > 0)
                amountOfTicketsForProcess = amountOfTicketsForProcess + 1;
            else if (ticketsLength - ((int)amountOfTicketsForProcess + startIndex) < 0)
                amountOfTicketsForProcess = ticketsLength - startIndex;
            return (int)amountOfTicketsForProcess;
        }
    }
}
