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
        void Stop(); // zakończ i zapisz wszystko do pliku
    }

    public record LogEntry(
       string Source,    // np. "Data", "Logic"
       int BallId,
       double X,
       double Y,
       DateTime Timestamp,
       string Type
    );

    public record CollisionLogEntry(
       string Source,    // np. "Data", "Logic"
       int BallId,
       double ballX,
       double ballY,
       double otherBallX,
       double otherBallY,
       DateTime Timestamp,
       string Type
    );
    //internal class Logger : ILogger
    //public class Logger : ILogger

    //{
    //    private readonly BlockingCollection<LogEntry> _queue = new(200);
    //    private readonly Thread _workerThread;
    //    private readonly string _filePath = "diagnostics.json";

    //    public Logger()
    //    {
    //        Console.WriteLine("Logger created");

    //        _workerThread = new Thread(Consume) { IsBackground = true };
    //        _workerThread.Start();
    //    }

    //    //public void Log(LogEntry entry)
    //    //{
    //    //    if (!_queue.TryAdd(entry))
    //    //    {
    //    //        // Możesz dodać fallback lub logowanie awaryjne
    //    //    }
    //    //}

    //    public void Log(LogEntry entry)
    //    {
    //        Console.WriteLine($"LOG: {entry.Source} - {entry.BallId} at {entry.Timestamp}");
    //        if (!_queue.TryAdd(entry))
    //        {
    //            Console.WriteLine("Logger queue full or closed.");
    //        }
    //    }


    //    //private void Consume()
    //    //{
    //    //    using StreamWriter writer = new(_filePath, append: true);
    //    //    foreach (var entry in _queue.GetConsumingEnumerable())
    //    //    {
    //    //        string json = JsonSerializer.Serialize(entry);
    //    //        writer.WriteLine(json);
    //    //        writer.Flush(); // natychmiastowy zapis
    //    //    }
    //    //}

    //    //private void Consume()
    //    //{
    //    //    try
    //    //    {
    //    //        Console.WriteLine("Consume() thread started");

    //    //        using StreamWriter writer = new(_filePath, append: true);
    //    //        foreach (var entry in _queue.GetConsumingEnumerable())
    //    //        {
    //    //            string json = JsonSerializer.Serialize(entry);
    //    //            writer.WriteLine(json);
    //    //            writer.Flush();
    //    //        }
    //    //    }
    //    //    catch (IOException ex)
    //    //    {
    //    //        Console.WriteLine("LOGGER ERROR: " + ex.Message);
    //    //    }
    //    //}


    //    //private void Consume()
    //    //{
    //    //    try
    //    //    {
    //    //        Console.WriteLine("Logger thread entering file write loop");

    //    //        using StreamWriter writer = new StreamWriter(_filePath, append: true);
    //    //        foreach (var entry in _queue.GetConsumingEnumerable())
    //    //        {
    //    //            string json = JsonSerializer.Serialize(entry);
    //    //            writer.WriteLine(json);
    //    //            writer.Flush();
    //    //            Console.WriteLine("Wrote log: " + json);
    //    //        }
    //    //    }
    //    //    catch (IOException ex)
    //    //    {
    //    //        Console.WriteLine("LOGGER IOException: " + ex.Message);
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        Console.WriteLine("LOGGER UNEXPECTED EXCEPTION: " + ex.Message);
    //    //    }
    //    //}

    //    private void Consume()
    //    {
    //        try
    //        {
    //            Console.WriteLine("Logger thread running.");
    //            foreach (var entry in _queue.GetConsumingEnumerable())
    //            {
    //                string json = JsonSerializer.Serialize(entry);
    //                File.AppendAllText(_filePath, json + Environment.NewLine);
    //                Console.WriteLine("Logged: " + json);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("LOGGER ERROR: " + ex.Message);
    //        }
    //    }



    //    public void Stop()
    //    {
    //        _queue.CompleteAdding();
    //        _workerThread.Join(); // poczekaj aż wszystko zapisze
    //    }


    //public class Logger : ILogger
    //{
    //    private static readonly object fileLock = new();
    //    private readonly string _filePath;

    //    public Logger()
    //    {
    //        _filePath = Path.Combine(
    //            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
    //            "diagnostics.json"
    //        );

    //        Console.WriteLine("Logger initialized. Writing to: " + _filePath);
    //    }

    //    public void Log(LogEntry entry)
    //    {
    //        try
    //        {
    //            string json = JsonSerializer.Serialize(entry);

    //            lock (fileLock) // sekcja krytyczna!
    //            {
    //                File.AppendAllText(_filePath, json + Environment.NewLine);
    //            }

    //            Console.WriteLine("Logged: " + json);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("Logger EXCEPTION: " + ex.Message);
    //        }
    //    }

    //    public void Stop()
    //    {
    //        // nic nie trzeba robić w tej wersji
    //        Console.WriteLine("Logger stopped.");
    //    }
    //}


    public class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntry> _queue = new(500);
        private readonly Thread _writerThread;
        private readonly string _filePath;
        private static readonly object fileLock = new(); // sekcja krytyczna

        public Logger()
        {
            //_filePath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //    "diagnostics.json"
            //);

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
            }
        }

        private void Consume()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                foreach (var entry in _queue.GetConsumingEnumerable())
                {
                    string json = JsonSerializer.Serialize(entry);

                    // Sekcja krytyczna dla bezpieczeństwa dostępu
                    lock (fileLock)
                    {
                        writer.WriteLine(json);
                        writer.Flush(); // natychmiastowy zapis
                    }

                    // Można tymczasowo wypisywać do konsoli
                    // Console.WriteLine("[LOGGER] Wrote: " + json);
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
            _writerThread.Join(); // czekaj aż zapisze wszystko
            Console.WriteLine("[LOGGER] Stopped.");
        }
    }


}


