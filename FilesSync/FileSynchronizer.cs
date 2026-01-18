using Serilog;
using System.Security.Cryptography;

namespace FilesSync;

internal static class FileSynchronizer
{
	public static void Sync(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		CopyAndUpdateFiles(sourceDirectoryPath, replicaDirectoryPath);
	}

	private static void CopyAndUpdateFiles(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var file in Directory.EnumerateFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relativeFilePath = Path.GetRelativePath(sourceDirectoryPath, file);
			var destinationFilePath = Path.Combine(replicaDirectoryPath, relativeFilePath);

			var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
			if (destinationDirectory is null)
			{
				throw new InvalidOperationException($"Invalid destination path {destinationFilePath}");
			}
			Directory.CreateDirectory(destinationDirectory);

			if (!File.Exists(destinationFilePath) || IsDifferent(file, destinationFilePath))
			{
				File.Copy(file, destinationFilePath, true);
				Log.Information("Copied/Updated: {file}", relativeFilePath);
			}
		}
	}

	private static bool IsDifferent(string file1, string file2)
	{
		return GetHash(file1) != GetHash(file2);
	}

	private static string GetHash(string path)
	{
		using var hashAlgorithm = MD5.Create();
		using var stream = File.OpenRead(path);

		var hash = hashAlgorithm.ComputeHash(stream);
		return Convert.ToHexStringLower(hash);
	}
}
