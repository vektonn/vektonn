using System;
using System.IO;

namespace Vektonn.Hosting
{
    public static class FileSystemHelpers
    {
        public static string PatchFileName(string fileName)
        {
            return Path.GetFullPath(Path.IsPathRooted(fileName) ? fileName : WalkDirectoryTree(fileName, File.Exists));
        }

        public static string PatchDirectoryName(string dirName)
        {
            return Path.GetFullPath(Path.IsPathRooted(dirName) ? dirName : WalkDirectoryTree(dirName, Directory.Exists));
        }

        private static string WalkDirectoryTree(string fileSystemObjectName, Func<string, bool> fileSystemObjectExists)
        {
            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (baseDirectory != null)
            {
                var candidateName = Path.Combine(baseDirectory.FullName, fileSystemObjectName);
                if (fileSystemObjectExists(candidateName))
                    return candidateName;

                baseDirectory = baseDirectory.Parent;
            }

            return fileSystemObjectName;
        }
    }
}
