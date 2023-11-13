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
            string json = File.ReadAllText("users.json");//Ініціалізуючий масив 
            List<User> users = JsonSerializer.Deserialize<User[]>(json).ToList();
            DistributeTickets(users, tickets);
            int it = 0, newProcessId = 1;
            while (users.Any(x => x.Processes.Any(y => !y.IsFinished))) //працює поки всі процеси не завершаться 
            {
                int random = new Random().Next(0, tickets.Length);
                tickets[random].MyProcess.StartProcess();
                if (tickets[random].MyProcess.IsFinished) //якщо процес завершився розподіляємо тікети заново 
                {
                    it = 0;
                    DistributeTickets(users, tickets); //Розподіляємо тікети заново 
                }
                if (new Random().Next(100) < 5) // випадковим чином створюємо новий процес зі шансом 5%
                {
                    User user = new User();
                    user.Name = $"NewUser{newProcessId}";
                    user.UserPriority = new Random().Next(10) + 1;
                    user.Processes = new List<MyProcess> { new MyProcess { Name = $"NewProcess{newProcessId}", BurstTime = new Random().NextDouble() + new Random().Next(10), Priority = new Random().Next(10) + 1 } };
                    users.Add(user);
                    newProcessId++;
                    DistributeTickets(users, tickets); //Розподіляємо тікети заново 
                }
                it++;
            }
            Console.WriteLine("\n\n####ANALYSIS####\n");
            foreach (var user in users)
            {
                Console.WriteLine(user);
                for (int i = 0; i < user.Processes.Count; i++)
                    Console.WriteLine(user.Processes[i]);
            }
        }
        public static void DistributeTickets(List<User> users, Ticket[] tickets)
        {
            if (users == null || users.Count <= 0)
                return;
            int mutualPriority = 0;
            foreach (var user in users)
                mutualPriority += user.UserPriority;
            int startIndex = 0;
            foreach (var user in users.FindAll(x => x.Processes.Any(y => !y.IsFinished))) //Розподіляємо тікеті тільки між тими процесами, які не завершилися
            {
                int amountOfTicketsForUser = Ticket.CountAmountTicketsByPriority(mutualPriority, user.UserPriority, tickets.Length, startIndex);
                user.Tickets = tickets.ElementsFrom<Ticket>(startIndex, amountOfTicketsForUser);
                startIndex += amountOfTicketsForUser;
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
                Processes =  new List<MyProcess>
                {
                new MyProcess { Name = "Process1 UserA", BurstTime = 10.5, Priority = 1 },
                new MyProcess { Name = "Process2 UserA", BurstTime = 8.0, Priority = 2 },
                },
            },
            new User
            {
                Name= "UserB",
                UserPriority = 2,
                Processes =  new List<MyProcess>
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