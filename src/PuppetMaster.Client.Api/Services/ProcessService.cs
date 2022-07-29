using System.Diagnostics;
using System.Runtime.InteropServices;
using PuppetMaster.Client.Valorant.Api.Models.Events;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    public class ProcessService : IDisposable, IProcessService
    {
        private readonly string _processName;
        private Timer? _timer;
        private Process? _process;

        public ProcessService(string processName)
        {
            _processName = processName;
            _process = GetProcess();
            if (_process != null)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessExited;
            }
            else
            {
                _timer = GetTimer();
            }
        }

        public event EventHandler<ProcessStateEventArgs>? ProcessChangeEvent;

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool IsRunning()
        {
            return _process != null;
        }

        public void SetGameToForeground()
        {
            if (_process != null)
            {
                SetForegroundWindow(_process.MainWindowHandle);
            }
        }
        
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void ProcessExited(object? sender, EventArgs e)
        {
            _process!.Exited -= ProcessExited;
            _process = null;

            _timer = GetTimer();

            var handler = ProcessChangeEvent;
            handler?.Invoke(this, new ProcessStateEventArgs()
            {
                IsRunning = false
            });
        }

        private void CheckIsRunning(object? state)
        {
            _process = GetProcess();
            if (_process != null)
            {
                _timer!.Dispose();
                _timer = null;

                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessExited;

                var handler = ProcessChangeEvent;
                handler?.Invoke(this, new ProcessStateEventArgs()
                {
                    IsRunning = true
                });
            }
        }

        private Process? GetProcess()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_processName)).FirstOrDefault();
        }

        private Timer GetTimer()
        {
            return new Timer(CheckIsRunning, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }
    }
}