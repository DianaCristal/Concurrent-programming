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
                writer.AutoFlush = true; // zapisywanie do pliku po każdym WriteLine
                foreach (LogEntry entry in _queue.GetConsumingEnumerable()) // blokuje do momentu, aż nie zostanie zamknięta kolekcja - czeka pasywnie az cos sie pojawi do zapisu, az nie bedzie jawnie wywolanie zamkniecia
                {
                    string json = JsonSerializer.Serialize(entry, _jsonOptions);

                    writer.WriteLine(json);
                }
                writer.Close(); // zamknie się i tak, ale jawne zamknięcie dla przejrzystości

            }
            catch (Exception) { }
        }


        public void Stop()
        {
            _queue.CompleteAdding(); //blokuje dalsze dodawanie i zamyka enumerator
            _writerThread.Join();   //czeka, aż wątek zakończy zapisywanie danych

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


