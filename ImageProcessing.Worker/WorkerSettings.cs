namespace ImageProcessing.Worker
{
    public class WorkerSettings
    {
        public int Port { get; set; }
        public string CoordinatorIp { get; set; }
        public int CoordinatorPort { get; set; }
        public string WorkerId { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор по умолчанию
    }
}