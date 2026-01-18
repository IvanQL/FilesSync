using Serilog;
using System.Security.Cryptography;

namespace FilesSync;

internal static class FileSynchronizer
{
	public static void Sync(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		SyncDirectories(sourceDirectoryPath, replicaDirectoryPath);
		CopyAndUpdateFiles(sourceDirectoryPath, replicaDirectoryPath);
		DeleteExtraFiles(sourceDirectoryPath, replicaDirectoryPath);
	}

	private static void CopyAndUpdateFiles(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var patch in Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relativeFilePath = Path.GetRelativePath(sourceDirectoryPath, patch);
			var destinationFilePath = Path.Combine(replicaDirectoryPath, relativeFilePath);

			var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
			if (destinationDirectory is null)
			{
				throw new InvalidOperationException($"Invalid destination path {destinationFilePath}");
			}
			Directory.CreateDirectory(destinationDirectory);

			if (!File.Exists(destinationFilePath) || AreFilesDifferent(patch, destinationFilePath))
			{
				File.Copy(patch, destinationFilePath, true);
				Log.Information("File {file} is Copied or Updated", relativeFilePath);
			}
		}
	}

	private static void DeleteExtraFiles(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var file in Directory.GetFiles(replicaDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relativeFilePath = Path.GetRelativePath(replicaDirectoryPath, file);
			var sourceFile = Path.Combine(sourceDirectoryPath, relativeFilePath);

			if (!File.Exists(sourceFile))
			{
				File.Delete(file);
				Log.Information("File: {file} is deleted", relativeFilePath);
			}
		}
	}

	private static void SyncDirectories(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		CreateMissingDirectories(sourceDirectoryPath, replicaDirectoryPath);
		RemoveExtraDirectories(sourceDirectoryPath, replicaDirectoryPath);
	}

	private static void CreateMissingDirectories(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var sourceDirectory in Directory.GetDirectories(sourceDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(sourceDirectoryPath, sourceDirectory);
			var destinationDirectory = Path.Combine(replicaDirectoryPath, relativePath);
			Directory.CreateDirectory(destinationDirectory);
			Log.Information("Directory: {directory} is created or exists", relativePath);
		}
	}

	private static void RemoveExtraDirectories(string sourceDirectoryPath, string replicaDirectoryPath)
	{
		foreach (var path in Directory.GetDirectories(replicaDirectoryPath, "*", SearchOption.AllDirectories))
		{
			var relative = Path.GetRelativePath(replicaDirectoryPath, path);
			var sourceDir = Path.Combine(sourceDirectoryPath, relative);

			if (!Directory.Exists(sourceDir))
			{
				Directory.Delete(path, true);
				Log.Information("Deleted directory: {directory}", relative);
			}
		}
	}

	private static bool AreFilesDifferent(string file1, string file2)
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
