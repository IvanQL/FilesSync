

using Serilog;

namespace FilesSync;

class Program
{
	static void Main(string[] args)
	{

		if (args.Length != 4)
		{
			Console.WriteLine("It should be: <source> <replica> <intervalSec> <logFile>");
		}

		var source = Path.GetFullPath(args[0]);
		var replica = Path.GetFullPath(args[1]);
		if (!double.TryParse(args[2], out var intervalSeconds))
		{
			throw new ArgumentException("Invalid interval. Please enter a value in seconds.");
		}

		var interval = TimeSpan.FromSeconds(intervalSeconds);
		var logFile = Path.GetFullPath(args[3]);

		
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
			.CreateLogger();
	}
}