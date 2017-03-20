using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Environment;
using Bluewire.Common.Console.Logging;
using Bluewire.MetricsAdapter;
using Bluewire.MetricsAdapter.Configuration;
using Bluewire.MetricsAdapter.Periodic;
using log4net;
using Metrics;

namespace DatabaseLatencyTester
{
    class Program
    {
        static void Main(string[] args)
        {
            DaemonRunner.Run(args, new Daemonisable());
        }
    }

    class Daemonisable : DaemonisableBase
    {
        public Daemonisable() : base("DatabaseLatencyTester")
        {
        }

        public override IDaemon Start(object args)
        {
            Log.Configure();

            var serviceConfiguration = (ServiceConfigurationSection)ConfigurationManager.GetSection("service");

            if (serviceConfiguration.Queries.Interval <= TimeSpan.Zero) throw new ConfigurationErrorsException("Ping interval must be positive.");
            if (serviceConfiguration.Queries.Interval < TimeSpan.FromMilliseconds(1)) throw new ConfigurationErrorsException("Ping interval is less than 1ms.");
            var connectionString = ConfigurationManager.ConnectionStrings["Target"];
            if (String.IsNullOrWhiteSpace(connectionString.ConnectionString)) throw new ConfigurationErrorsException("No Target connection string specified.");

            var definitions = serviceConfiguration.Queries
                .Cast<ServiceConfigurationSection.QueryElement>()
                .Select(q => new QueryDefinition
                {
                    Name = q.Name,
                    Weighting = q.Weighting,
                    Text = q.Text
                })
                .ToArray();

            var metricsBasePath = Path.GetFullPath(String.IsNullOrWhiteSpace(serviceConfiguration.MetricsPath) ? Environment.CurrentDirectory : serviceConfiguration.MetricsPath);

            var metricsConfiguration = (MetricsConfigurationSection)ConfigurationManager.GetSection("metrics");
            if(metricsConfiguration.PerMinute.Enabled)
            {
                Metric.Config.WithReporting(r => r.WithJsonReport(metricsConfiguration.PerMinute.GetLogLocation(metricsBasePath, "perMinute"), new PerMinuteLogPolicy(TimeSpan.FromDays(metricsConfiguration.PerMinute.DaysToKeep ?? 7)), new ZipLogArchiver()));
            }
            if(metricsConfiguration.PerHour.Enabled)
            {
                Metric.Config.WithReporting(r => r.WithJsonReport(metricsConfiguration.PerHour.GetLogLocation(metricsBasePath, "perHour"), new PerHourLogPolicy(TimeSpan.FromDays(metricsConfiguration.PerHour.DaysToKeep ?? 366)), new ZipLogArchiver()));
            }
            if (new EnvironmentAnalyser().GetEnvironment() is ApplicationEnvironment)
            {
                Metric.Config.WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(5)));
            }

            return new DaemonInstance(definitions, connectionString.ConnectionString, serviceConfiguration.Queries.Interval);
        }

    }
}
