namespace ClassLibrary
{
    public class MyProcess
    {
        public int ScheduledTimes = 0;
        public string Name { get; set; }
        public double BurstTime { get; set; } // In milliseconds 
        public int Priority { get; set; }
        public bool IsFinished = false;
        public virtual void StartProcess()
        {
            ScheduledTimes++;
            Console.WriteLine($"Starting: {Name} | BurstTime: {BurstTime} | Priority: {Priority}");
            double elapsedTime = new Random().Next((int)(2 * BurstTime)) + new Random().NextDouble();//наданий час 
            if (BurstTime <= elapsedTime) //Імітуємо потрібний час виконання та наданий 
            {
                IsFinished = true;
                Console.WriteLine($"{Name} is finished | BurstTime: {BurstTime} | ElapsedTime: {elapsedTime} |Priority: {Priority}");
            }
        }
        public override string ToString()
        {
            return $"Process: {Name} | Sheduled: {ScheduledTimes} times | Priority: {Priority}";
        }
    }
}
