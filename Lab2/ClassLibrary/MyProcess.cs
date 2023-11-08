namespace ClassLibrary
{
    public class MyProcess
    {
        public int ScheduledTimes = 0;
        public string Name { get; set; }
        public double BurstTime { get; set; }
        public int Priority { get; set; }
        public virtual void StartProcess()
        {
            ScheduledTimes++;
            Console.WriteLine($"Starting: {Name} | BurstTime: {BurstTime} | Priority: {Priority}");
        }
        public override string ToString()
        {
            return $"Process: {Name} | Sheduled: {ScheduledTimes} times | Priority: {Priority}";
        }
    }
}
