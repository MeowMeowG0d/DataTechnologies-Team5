namespace DataTechnologies.Services
{
    public class BackupScheduler : IDisposable
    {
        private readonly BackupService _backupService;
        private readonly TimeSpan _interval;
        private CancellationTokenSource? _cts;

        public BackupScheduler(BackupService backupService, int intervalHours = 24)
        {
            _backupService = backupService;
            _interval = TimeSpan.FromHours(intervalHours);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _ = RunAsync(_cts.Token);
        }

        public void Stop() => _cts?.Cancel();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _cts?.Dispose();
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(_interval);
            while (await timer.WaitForNextTickAsync(ct))
            {
                try
                {
                    var result = await _backupService.RunFullBackupAsync();
                    var status = result.Success ? "OK" : $"ERRORS: {string.Join(", ", result.Errors)}";
                    await File.AppendAllTextAsync(
                        Path.Combine(_backupService.OutputBasePath, "backup.log"),
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Scheduled backup {status}{Environment.NewLine}",
                        CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await File.AppendAllTextAsync(
                        Path.Combine(_backupService.OutputBasePath, "backup.log"),
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Scheduled backup FAILED: {ex.Message}{Environment.NewLine}",
                        CancellationToken.None);
                }
            }
        }
    }
}
