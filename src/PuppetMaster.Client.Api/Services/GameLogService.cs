using System.Text;
using PuppetMaster.Client.Valorant.Api.Models.Events;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    internal class GameLogService : IDisposable, IGameLogService
    {
        private const char NewLine = '\n';
        private readonly string _filePath;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly TimeSpan _pollingRate;
        private string? _lastLine;

        public GameLogService(string filePath, TimeSpan pollingRate)
        {
            _filePath = filePath;
            _pollingRate = pollingRate;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        public void Start()
        {
            Task.Run(
                async () =>
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(_pollingRate);
                        var newLines = GetNewLines();
                        if (newLines.Any())
                        {
                            _lastLine = newLines.First();
                        }

                        foreach (var line in newLines.Reverse())
                        {
                            var handler = LogMessageEvent;
                            handler?.Invoke(this, new LogMessageEventArgs()
                            {
                                Message = line
                            });
                        }
                    }
                },
                _cancellationTokenSource.Token);        
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private static string PreviousLine(Stream stream)
        {
            var lineLength = 0;
            while (stream.Position > 0)
            {
                stream.Position--;
                var byteFromFile = stream.ReadByte();

                if (byteFromFile < 0)
                {
                    return string.Empty;
                }
                else if (byteFromFile == NewLine)
                {
                    break;
                }

                lineLength++;
                stream.Position--;
            }

            if (lineLength == 0)
            {
                return string.Empty;
            }

            var oldPosition = stream.Position;
            var bytes = new BinaryReader(stream).ReadBytes(lineLength - 1);

            stream.Position = oldPosition > 0 ? oldPosition - 1 : oldPosition;
            return Encoding.UTF8.GetString(bytes).Replace(Environment.NewLine, string.Empty);
        }

        private IEnumerable<string> GetNewLines()
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (stream.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            stream.Position = stream.Length - 1;

            string currentNewLine = PreviousLine(stream);
            var newLines = new List<string>();

            while (_lastLine != currentNewLine)
            {
                if (!string.IsNullOrEmpty(currentNewLine))
                {
                    newLines.Add(currentNewLine);
                }

                if (stream.Position == 0)
                {
                    break;
                }

                currentNewLine = PreviousLine(stream);
            }

            return newLines;
        }
    }
}
