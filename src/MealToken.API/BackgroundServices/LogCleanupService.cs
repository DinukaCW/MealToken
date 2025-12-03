
namespace MealToken.API.BackgroundServices
{
	public class LogCleanupService : BackgroundService
	{
		private readonly ILogger<LogCleanupService> _logger;
		private readonly string _logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

		public LogCleanupService(ILogger<LogCleanupService> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					CleanupOldLogs();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Log cleanup failed.");
				}

				// Run once every 24 hours
				await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
			}
		}

		private void CleanupOldLogs()
		{
			if (!Directory.Exists(_logDirectory))
				return;

			var files = Directory.GetFiles(_logDirectory, "*.txt");

			foreach (var file in files)
			{
				var fileInfo = new FileInfo(file);

				// Delete files older than 30 days
				if (fileInfo.CreationTime < DateTime.Now.AddDays(-30))
				{
					fileInfo.Delete();
					_logger.LogInformation("Deleted old log file: {FileName}", fileInfo.Name);
				}
			}
		}
	}
}
