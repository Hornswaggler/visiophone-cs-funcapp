using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vp
{

    public class Config
    {
        private static string STORAGE_CONNECTION_STRING = "STORAGE_CONNECTION_STRING";
        private static string SAMPLE_CONTAINER_NAME = "SAMPLE_CONTAINER_NAME";
        private static string MONGO_CONNECTION_STRING = "MONGO_CONNECTION_STRING";

        public static string StorageConnectionString { get; set; } = Environment.GetEnvironmentVariable(STORAGE_CONNECTION_STRING);
        public static string SampleBlobContainerName { get; set; } = Environment.GetEnvironmentVariable(SAMPLE_CONTAINER_NAME);
        public static string MongoConnectionString { get; set; } = Environment.GetEnvironmentVariable(MONGO_CONNECTION_STRING);
        public static int BufferSize { get; set; } =   1 * 1024 * 1024;

        public static int PreviewBitrate = 128;
    }
}
