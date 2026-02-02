// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/Helpers/TokenHelpers.cs

using System.IO;
using SutureHealth.Patients.Resources;

namespace SutureHealth.Patients.Helpers
{
    public static class TokenHelpers
    {
        public static IToken GetRefreshToken(MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath();

            return File.Exists(path)
                ? ApiHelper.Deserialize<AuthResponse>(File.ReadAllText(path), defaultMediaType)
                : new AuthResponse();
        }

        public static void Save(this IToken authResponse, MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath();

            File.WriteAllText(path, ApiHelper.Serialize(authResponse, defaultMediaType));
        }
    }
}
