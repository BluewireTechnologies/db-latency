using System.Transactions;

namespace DatabaseLatencyTester
{
    public class QueryDefinition
    {
        public string Name { get; set; }
        public float Weighting { get; set; }
        public string Text { get; set; }
        public IsolationLevel Isolation { get; set; }
    }
}
