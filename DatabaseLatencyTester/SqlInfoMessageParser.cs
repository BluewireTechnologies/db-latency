using System.Text.RegularExpressions;

namespace DatabaseLatencyTester
{
    public class SqlInfoMessageParser
    {
        private static readonly Regex rxTimeStatisticsMessage = new Regex(@"CPU time = (?<cpu>[\d\.]+) ms,  elapsed time = (?<elapsed>[\d\.]+) ms.");

        public bool TryParse(string message, ref SqlTimeStatistics time)
        {
            var m = rxTimeStatisticsMessage.Match(message);
            if (!m.Success) return false;

            try
            {
                var cpu = NanosecondsFromMilliseconds(m.Groups["cpu"].Value);
                var elapsed = NanosecondsFromMilliseconds(m.Groups["elapsed"].Value);
                time.CpuNs += cpu;
                time.ElapsedNs += elapsed;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static long NanosecondsFromMilliseconds(string msString)
        {
            return (long)(double.Parse(msString) * 1000 * 1000);
        }
    }
}
