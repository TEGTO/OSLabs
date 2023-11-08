using ClassLibrary;
using System.Text.Json;
namespace TicketCurrency
{
    class Program
    {
        const int AMOUNT_OF_TICKETS = 100;
        private static void Main()
        {
            //CreateUsers();
            Ticket[] tickets = new Ticket[AMOUNT_OF_TICKETS];
            for (int i = 0; i < tickets.Length; i++)
                tickets[i] = new Ticket();
            string json = File.ReadAllText("users.json");
            var users = JsonSerializer.Deserialize<User[]>(json);
            int mutualPriority = 0;
            if (users == null)
                return;
            foreach (var user in users)
                mutualPriority += user.UserPriority;
            int startIndex = 0;
            foreach (var user in users)
            {
                int amountOfTicketsForUser = Ticket.CountAmountTicketsByPriority(mutualPriority, user.UserPriority, tickets.Length, startIndex);
                user.Tickets = tickets.ElementsFrom<Ticket>(startIndex, amountOfTicketsForUser);
                startIndex = amountOfTicketsForUser;
            }
            for (int i = 0; i < tickets.Length; i++)
            {
                int random = new Random().Next(0, tickets.Length);
                tickets[random].MyProcess.StartProcess();
            }
            Console.WriteLine("\n\n####ANALYSIS####\n");
            foreach (var user in users)
            {
                Console.WriteLine(user);
                for (int i = 0; i < user.Processes.Length; i++)
                    Console.WriteLine(user.Processes[i]);
            }
        }
        public static void FisherYatesShuffle<T>(T[] array)
        {
            Random rng = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                // Swap array[i] and array[j]
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
        public static void CreateUsers()
        {
            var users = new User[]
           {
            new User
            {
                Name= "UserA",
                UserPriority = 1,
                Processes =  new MyProcess[]
                {
                new MyProcess { Name = "Process1 UserA", BurstTime = 10.5, Priority = 1 },
                new MyProcess { Name = "Process2 UserA", BurstTime = 8.0, Priority = 2 },
                },
            },
            new User
            {
                Name= "UserB",
                UserPriority = 2,
                Processes =  new MyProcess[]
                {
                new MyProcess { Name = "Process3 UserB", BurstTime = 10.5, Priority = 1 },
                new MyProcess { Name = "Process4 UserB", BurstTime = 8.0, Priority = 2 },
                },
            }

           };
            string json = JsonSerializer.Serialize(users);
            File.WriteAllText("users.json", json);
        }
    }
}