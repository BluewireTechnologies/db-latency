using System;

namespace DatabaseLatencyTester
{
    public class SqlTimeStatistics
    {
        public long CpuNs { get; set; }
        public long ElapsedNs { get; set; }
    }
}
