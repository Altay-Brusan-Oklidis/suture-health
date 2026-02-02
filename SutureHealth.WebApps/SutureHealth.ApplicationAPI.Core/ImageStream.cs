using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SutureHealth.Application
{
    public class ImageStream
    {
        public Stream Stream { get; set; }

        public static string ToBase64Image(ImageStream stream)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                stream.Stream.CopyTo(ms);
                bytes = ms.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }
        public string ContentType { get; set; }
        public string Error { get; set; }
    }
}
