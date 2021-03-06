﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.Logging;

namespace System.IO
{
	public static class IoHelpers
	{
		private const string OldExtension = ".old";
		private const string NewExtension = ".new";

		// http://stackoverflow.com/a/14933880/2061103
		public static async Task DeleteRecursivelyWithMagicDustAsync(string destinationDir)
		{
			const int magicDust = 10;
			for (var gnomes = 1; gnomes <= magicDust; gnomes++)
			{
				try
				{
					Directory.Delete(destinationDir, true);
				}
				catch (DirectoryNotFoundException)
				{
					return;  // good!
				}
				catch (IOException)
				{
					if (gnomes == magicDust)
						throw;
					// System.IO.IOException: The directory is not empty
					Logger.LogDebug($"Gnomes prevent deletion of {destinationDir}! Applying magic dust, attempt #{gnomes}.", nameof(IoHelpers));

					// see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true for more magic
					await Task.Delay(100);
					continue;
				}
				catch (UnauthorizedAccessException)
				{
					if (gnomes == magicDust)
						throw;
					// Wait, maybe another software make us authorized a little later
					Logger.LogDebug($"Gnomes prevent deletion of {destinationDir}! Applying magic dust, attempt #{gnomes}.", nameof(IoHelpers));

					// see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true for more magic
					await Task.Delay(100);
					continue;
				}
				return;
			}
			// depending on your use case, consider throwing an exception here
		}

		// https://stackoverflow.com/a/7957634/2061103
		private static void SafeMove(string newPath, string path)
		{
			var oldPath = path + OldExtension;
			if (File.Exists(oldPath))
			{
				File.Delete(oldPath);
			}

			if (File.Exists(path))
			{
				File.Move(path, oldPath);
			}

			File.Move(newPath, path);

			File.Delete(oldPath);
		}

		public static void SafeWriteAllText(string path, string content)
		{
			var newPath = path + NewExtension;
			File.WriteAllText(newPath, content);
			SafeMove(newPath, path);
		}

		public static void SafeWriteAllText(string path, string content, Encoding encoding)
		{
			var newPath = path + NewExtension;
			File.WriteAllText(newPath, content, encoding);
			SafeMove(newPath, path);
		}

		public static async Task SafeWriteAllTextAsync(string path, string content)
		{
			var newPath = path + NewExtension;
			await File.WriteAllTextAsync(newPath, content);
			SafeMove(newPath, path);
		}

		public static async Task SafeWriteAllTextAsync(string path, string content, Encoding encoding)
		{
			var newPath = path + NewExtension;
			await File.WriteAllTextAsync(newPath, content, encoding);
			SafeMove(newPath, path);
		}

		public static void SafeWriteAllLines(string path, IEnumerable<string> content)
		{
			var newPath = path + NewExtension;
			File.WriteAllLines(newPath, content);
			SafeMove(newPath, path);
		}

		public static async Task SafeWriteAllLinesAsync(string path, IEnumerable<string> content)
		{
			var newPath = path + NewExtension;
			await File.WriteAllLinesAsync(newPath, content);
			SafeMove(newPath, path);
		}

		public static async Task SafeWriteAllBytesAsync(string path, byte[] content)
		{
			var newPath = path + NewExtension;
			await File.WriteAllBytesAsync(newPath, content);
			SafeMove(newPath, path);
		}

		// https://stackoverflow.com/a/7957634/2061103
		public static bool TryGetSafestFileVersion(string path, out string safestFilePath)
		{
			var newPath = path + NewExtension;
			var oldPath = path + OldExtension;

			// If foo.data and foo.data.new exist, load foo.data; foo.data.new may be broken (e.g. power off during write).
			if (File.Exists(path) && File.Exists(newPath))
			{
				safestFilePath = path;
				return true;
			}

			// If foo.data.old and foo.data.new exist, both should be valid, but something died very shortly afterwards - you may want to load the foo.data.old version anyway.
			if (File.Exists(oldPath) && File.Exists(newPath))
			{
				safestFilePath = oldPath;
				return true;
			}

			// If foo.data and foo.data.old exist, then foo.data should be fine, but again something went wrong, or possibly the file couldn't be deleted.
			if (File.Exists(oldPath) && File.Exists(path))
			{
				safestFilePath = path;
				return true;
			}

			if (File.Exists(path))
			{
				safestFilePath = path;
				return true;
			}

			safestFilePath = null;
			return false;
		}
	}
}
