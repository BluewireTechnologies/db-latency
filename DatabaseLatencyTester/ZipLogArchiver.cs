using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bluewire.MetricsAdapter.Periodic;

namespace DatabaseLatencyTester
{
    public class ZipLogArchiver : ILogArchiver
    {
        public async Task Archive(Stream target, IEnumerable<LogArchivePart> parts)
        {
            using (var zip = new System.IO.Compression.ZipArchive(target, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var part in parts)
                {
                    var entry = zip.CreateEntry(part.Name);
                    using (var stream = entry.Open())
                    {
                        await part.WriteTo(stream);
                    }
                }
            }
        }
    }
}
