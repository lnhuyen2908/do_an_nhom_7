namespace web_do_an1.Services
{
    public static class LectureFileStorage
    {
        public const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly IReadOnlyDictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };

        public static bool IsAllowedExtension(string extension)
        {
            return ContentTypes.ContainsKey(extension);
        }

        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return ContentTypes.GetValueOrDefault(extension, "application/octet-stream");
        }

        public static string GetPrivateDirectory(string contentRootPath)
        {
            return Path.Combine(contentRootPath, "App_Data", "Lectures");
        }

        public static string GetStoredFileName(string fileReference)
        {
            return Path.GetFileName(fileReference.Replace('\\', '/'));
        }

        public static string? ResolveExistingPath(string contentRootPath, string fileReference)
        {
            var storedName = GetStoredFileName(fileReference);
            if (string.IsNullOrWhiteSpace(storedName))
            {
                return null;
            }

            var privatePath = SafePath(GetPrivateDirectory(contentRootPath), storedName);
            if (privatePath != null && File.Exists(privatePath))
            {
                return privatePath;
            }

            var legacyPath = SafePath(Path.Combine(contentRootPath, "wwwroot", "uploads", "lectures"), storedName);
            return legacyPath != null && File.Exists(legacyPath) ? legacyPath : null;
        }

        public static string CreatePrivatePath(string contentRootPath, string storedName)
        {
            var directory = GetPrivateDirectory(contentRootPath);
            Directory.CreateDirectory(directory);
            return SafePath(directory, storedName)
                ?? throw new InvalidOperationException("Tên file bài giảng không hợp lệ.");
        }

        public static void DeleteIfExists(string contentRootPath, string fileReference)
        {
            var path = ResolveExistingPath(contentRootPath, fileReference);
            if (path != null)
            {
                File.Delete(path);
            }
        }

        public static void MigratePublicFiles(string contentRootPath)
        {
            var publicDirectory = Path.Combine(contentRootPath, "wwwroot", "uploads", "lectures");
            if (!Directory.Exists(publicDirectory))
            {
                return;
            }

            var privateDirectory = GetPrivateDirectory(contentRootPath);
            Directory.CreateDirectory(privateDirectory);
            foreach (var sourcePath in Directory.EnumerateFiles(publicDirectory))
            {
                var destinationPath = SafePath(privateDirectory, Path.GetFileName(sourcePath));
                if (destinationPath == null)
                {
                    continue;
                }

                if (File.Exists(destinationPath))
                {
                    if (new FileInfo(sourcePath).Length == new FileInfo(destinationPath).Length)
                    {
                        File.Delete(sourcePath);
                    }
                    continue;
                }

                File.Move(sourcePath, destinationPath);
            }
        }

        private static string? SafePath(string directory, string fileName)
        {
            var root = Path.GetFullPath(directory);
            var candidate = Path.GetFullPath(Path.Combine(root, Path.GetFileName(fileName)));
            return candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                ? candidate
                : null;
        }
    }
}
