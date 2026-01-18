

using Serilog;

namespace FilesSync;

internal static class Program
{
	static void Main(string[] args)
	{

		if (args.Length != 4)
		{
			Console.WriteLine("It should be: <source> <replica> <intervalSec> <logFile>");
			return;
		}

		var sourceDirectoryPath = Path.GetFullPath(args[0]);
		var replicaDirectoryPath = Path.GetFullPath(args[1]);
		if (!double.TryParse(args[2], out var intervalSeconds))
		{
			Console.WriteLine("Invalid interval. Please enter a value in seconds");
			return;
		}

		if (intervalSeconds <= 0)
		{
			Console.WriteLine("Interval must be greater than 0");
			return;
		}

		var interval = TimeSpan.FromSeconds(intervalSeconds);
		var logFile = Path.GetFullPath(args[3]);
		var logDirectoryName = Path.GetDirectoryName(logFile);


		if (string.IsNullOrEmpty(logDirectoryName))
		{
			Console.WriteLine("Invalid log file path");
			return;
		}

		Directory.CreateDirectory(sourceDirectoryPath);
		Directory.CreateDirectory(replicaDirectoryPath);
		Directory.CreateDirectory(logDirectoryName);

		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
			.CreateLogger();

		while (true)
		{
			try
			{
				FileSynchronizer.Sync(sourceDirectoryPath, replicaDirectoryPath);
			}
			catch (Exception exception)
			{
				Log.Error(exception, "Error during sync");
			}

			Thread.Sleep(interval);
		}
	}


}

