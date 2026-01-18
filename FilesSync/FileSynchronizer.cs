using Serilog;
using System.Security.Cryptography;

namespace FilesSync;

internal static class FileSynchronizer
{
	public static void Sync(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		CopyAndUpdateFiles(sourceDirectoryPath, replicaDirectoryPath);
		DeleteExtraFiles(sourceDirectoryPath, replicaDirectoryPath);
		CleanEmptyDirectories(replicaDirectoryPath);
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

	private static void DeleteExtraFiles(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var file in Directory.EnumerateFiles(replicaDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relativeFilePath = Path.GetRelativePath(replicaDirectoryPath, file);
			var sourceFile = Path.Combine(sourceDirectoryPath, relativeFilePath);

			if (!File.Exists(sourceFile))
			{
				File.Delete(file);
				Log.Information("Deleted: {file}", relativeFilePath);
			}
		}
	}

	private static void CleanEmptyDirectories(string replicaDirectoryPath)
	{
		var directories = Directory.GetDirectories(replicaDirectoryPath, "*", SearchOption.AllDirectories);
		foreach (var directoryPath  in directories.Reverse())
		{
			if (Directory.GetFileSystemEntries(directoryPath ).Length == 0)
			{
				Directory.Delete(directoryPath );
				Log.Information("Removed empty dir: {dir}", Path.GetRelativePath(replicaDirectoryPath, directoryPath ));
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
