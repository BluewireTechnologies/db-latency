using System;
using System.ComponentModel;
using System.Configuration;
using System.Transactions;

namespace DatabaseLatencyTester
{
    public class ServiceConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("metricsPath")]
        public string MetricsPath => (string)base["metricsPath"];

        [ConfigurationProperty("queries", IsRequired = true)]
        public QueriesCollection Queries => (QueriesCollection)base["queries"];

        [ConfigurationCollection(typeof(QueryElement))]
        public class QueriesCollection : ConfigurationElementCollection
        {
            [ConfigurationProperty("interval", IsRequired = true)]
            public TimeSpan Interval => (TimeSpan)base["interval"];

            protected override string ElementName => "query";
            public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

            protected override ConfigurationElement CreateNewElement()
            {
                return new QueryElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((QueryElement)element).Name;
            }
        }

        public class QueryElement : ConfigurationElement
        {
            [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
            public string Name => (string)base["name"];

            [ConfigurationProperty("weighting", DefaultValue = 1.0f)]
            public float Weighting => (float)base["weighting"];

            [ConfigurationProperty("text", IsRequired = true)]
            public string Text => (string)base["text"];

            [ConfigurationProperty("isolation", DefaultValue = IsolationLevel.ReadCommitted)]
            public IsolationLevel Isolation => (IsolationLevel)base["isolation"];
        }
    }
}