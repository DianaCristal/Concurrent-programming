using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
    public interface ILogger
    {
        void Log(LogEntry entry);
        void Stop();
    }

    public class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntry> _queue = new(500);
        private readonly Thread _writerThread;
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private int bufferFull = 0;
        public Logger()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logDirectory = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            _filePath = Path.Combine(logDirectory, $"diagnostics_{timestamp}.json");

            _jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false
            };

            _writerThread = new Thread(Consume)
            {
                Name = "LoggerWriterThread"
            };
            _writerThread.Start();
        }

        public void Log(LogEntry entry)
        {
            if (!_queue.TryAdd(entry))
            {
                Interlocked.Increment(ref bufferFull);
            }
        }

  
        private void Consume()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                writer.AutoFlush = true;
                foreach (LogEntry entry in _queue.GetConsumingEnumerable())
                {
                    string json = JsonSerializer.Serialize(entry, _jsonOptions);

                    writer.WriteLine(json);
                }
                writer.Close();

            }
            catch (Exception) { }
        }


        public void Stop()
        {
            _queue.CompleteAdding();
            _writerThread.Join();

            if (bufferFull > 0)
            {
                LogEntry overflowEntry = new BufferFullLogEntry(
                    bufferFull,
                    DateTime.UtcNow
                );

                try
                {
                    using StreamWriter writer = new StreamWriter(_filePath, append: true);
                    writer.WriteLine(JsonSerializer.Serialize(overflowEntry, _jsonOptions));
                }
                catch (Exception) { }
            }
        }
    }


}


