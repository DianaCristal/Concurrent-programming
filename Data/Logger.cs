using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using System;
using System.Threading;

namespace TP.ConcurrentProgramming.Infrastructure
{
    public interface ILogger
    {
        void Log(LogEntry entry);
        void Stop();
    }
    public record LogEntry(
       string Source,
       int BallId,
       double X,
       double Y,
       DateTime Timestamp,
       string Type
    );
    public class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntry> _queue = new(500);
        private readonly Thread _writerThread;
        private readonly string _filePath;
        private static readonly object fileLock = new();

        public Logger()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logDirectory = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            _filePath = Path.Combine(logDirectory, $"diagnostics_{timestamp}.json");

            Console.WriteLine("[LOGGER] Initialized: " + _filePath);

            _writerThread = new Thread(Consume)
            {
                IsBackground = true,
                Name = "LoggerWriterThread"
            };
            _writerThread.Start();
        }

        public void Log(LogEntry entry)
        {
            if (!_queue.TryAdd(entry))
            {
                Console.WriteLine("[LOGGER] Queue full – dropping log.");

                LogEntry overflowEntry = new LogEntry(
                    Source: "Logger",
                    BallId: -1,
                    X: 0,
                    Y: 0,
                    Timestamp: DateTime.Now,
                    Type: "BUFFER_FULL"
                );

                try
                {
                    _queue.Add(overflowEntry);
                }
                catch
                {
                    Console.WriteLine("[LOGGER] Failed to log BUFFER_FULL event.");
                }
            }
        }

        private void Consume()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                foreach (LogEntry entry in _queue.GetConsumingEnumerable())
                {
                    string json = JsonSerializer.Serialize(entry);

                    lock (fileLock)
                    {
                        writer.WriteLine(json);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LOGGER] ERROR: " + ex.Message);
            }
        }

        public void Stop()
        {
            _queue.CompleteAdding();
            _writerThread.Join();
            Console.WriteLine("[LOGGER] Stopped.");
        }
    }


}


