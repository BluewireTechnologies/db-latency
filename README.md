# Simple Latency Test Tool

Periodically chooses a random query from those configured, then times its execution against a database server.

The following metrics are recorded:
* time to open the connection,
* time to call ExecuteReader(), ie. start getting data from the database server,
* time taken to read the entire resultset into objects,
* time taken to commit the transaction,
* total time for all of the above,
* the CPU time reported by the database server (via `SET STATISTICS TIME ON`)
* the total elapsed time reported by the database server (via `SET STATISTICS TIME ON`).

The Metrics.NET library is used to record this information.
Bluewire.MetricsAdapter is used to periodically log the statistics to disk.
Metrics are logged in JSON format.

The following behaviour is defined by the configuration file:
* The target database server (and database) is specified via the `Target` connection string.
* The set of queries is defined in the <queries> element.
* The query interval is defined by the `interval` attribute of the `<queries>` element.
* Metrics logging to disk is on by default, for both per minute and per hour logging, but the bundled configuration file currently disables per hour logging.

The application can be run as a console tool, in which case it will also log a metrics summary to the console every five seconds, or it can be installed to run as a service. Use the `--help` switch to see available options.
