// credis:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/FileIoExtensions.cs

using System.IO;

namespace SutureHealth.Patients.Helpers
{
    public static class FileIoExtensions
    {
        public static string AsAppPath(this string filePath)
        {
            string directoryName = Path.GetDirectoryName(@"/tmp/");
            if (string.IsNullOrWhiteSpace(directoryName)) return filePath;

            return Path.Combine(directoryName, filePath);
        }
    }
}
