using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bluewire.Common.Console;
using log4net;

namespace DatabaseLatencyTester
{
    class DaemonInstance : IDaemon
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DaemonInstance));

        private readonly List<QueryDefinition> definitions;
        private readonly string connectionString;
        private Timer timer;
        private readonly Random random = new Random();
        private readonly float totalWeights;

        public DaemonInstance(QueryDefinition[] definitions, string connectionString, TimeSpan interval)
        {
            if (definitions.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(definitions));
            this.definitions = definitions.OrderBy(d => d.Weighting).ToList();
            this.connectionString = connectionString;
            totalWeights = definitions.Sum(d => d.Weighting);
            timer = new Timer(_ => Tick(), null, TimeSpan.Zero, interval);
        }

        private void Tick()
        {
            try
            {
                var definition = SelectWeightedQuery(random.NextDouble() * totalWeights);
                new ProfiledQueryExecutor(connectionString).ExecuteQuery(definition);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private QueryDefinition SelectWeightedQuery(double random)
        {
            foreach (var definition in definitions)
            {
                random -= definition.Weighting;
                if (random < 0) return definition;
            }
            return definitions.Last();
        }

        public void Dispose()
        {
            timer.Dispose();
            timer = null;
        }
    }
}
