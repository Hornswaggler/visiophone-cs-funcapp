using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vp
{
    public class Config
    {
        //TODO These Should be Constants
        public static string connectionString { get; set; } = Environment.GetEnvironmentVariable("VP_STORAGE_CONNECTION_STRING");
        public static string containerName { get; set; } = Environment.GetEnvironmentVariable("VP_SAMPLE_CONTAINER_NAME");
    }
}
