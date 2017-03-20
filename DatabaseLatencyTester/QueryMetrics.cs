using Metrics;

namespace DatabaseLatencyTester
{
    public static class QueryMetrics
    {
        private static readonly MetricsContext ClientContext = Metric.Context("Client");
        private static readonly MetricsContext DatabaseContext = Metric.Context("Database");

        public static Timer OpenConnection = ClientContext.Timer("Open Connection", Unit.None);
        public static Timer ExecuteReader = ClientContext.Timer("ExecuteReader()", Unit.None);
        public static Timer ConsumeReader = ClientContext.Timer("Consume Reader", Unit.None);
        public static Timer CompleteTransaction = ClientContext.Timer("Complete Transaction", Unit.None);
        public static Timer Total = ClientContext.Timer("Total", Unit.None);

        public static Meter Succeeded = ClientContext.Meter("Succeeded", Unit.None);
        public static Meter Failed = ClientContext.Meter("Failed", Unit.None);

        public static Timer CPUTime = DatabaseContext.Timer("CPU Time", Unit.None);
        public static Timer ElapsedTime = DatabaseContext.Timer("Elapsed Time", Unit.None);
    }
}
