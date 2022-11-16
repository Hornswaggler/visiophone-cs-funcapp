using System;

namespace vp
{
    public class Config
    {
        private static string STORAGE_CONNECTION_STRING = "STORAGE_CONNECTION_STRING";
        private static string SAMPLE_CONTAINER_NAME = "SAMPLE_CONTAINER_NAME";
        private static string COVER_ART_CONTAINER_NAME = "COVER_ART_CONTAINER_NAME";
        private static string MONGO_CONNECTION_STRING = "MONGO_CONNECTION_STRING";
        private static string SAMPLE_TRANSCODES_CONTAINER_NAME = "SAMPLE_TRANSCODES_CONTAINER_NAME";
        private static string TRANSCODE_PROFILES = "TRANSCODE_PROFILES";
        private static string USER_LIBRARY_CONTAINER_NAME = "USER_LIBRARY_CONTAINER_NAME";
        private static string PROVISION_STRIPE_STANDARD_RETURN_URL = "PROVISION_STRIPE_STANDARD_RETURN_URL";
        private static string PROVISION_STRIPE_STANDARD_REFRESH_URL = "PROVISION_STRIPE_STANDARD_REFRESH_URL";
        public static string FFMPEG_PATH = "site\\wwwroot\\Tools\\ffmpeg.exe";
        public static string HOME = "..\\Tools\\ffmpeg.exe";
        public static string WAV_CONTENT_TYPE = "audio/wav";
        public static string STRIPE_API_KEY = "STRIPE_API_KEY";
        public static int BufferSize { get; set; } = 1 * 1024 * 1024;
        public static int PreviewBitrate = 128;

        public static string StorageConnectionString { get; set; } = Environment.GetEnvironmentVariable(STORAGE_CONNECTION_STRING);
        public static string SampleBlobContainerName { get; set; } = Environment.GetEnvironmentVariable(SAMPLE_CONTAINER_NAME);
        public static string MongoConnectionString { get; set; } = Environment.GetEnvironmentVariable(MONGO_CONNECTION_STRING);
        public static string SampleTranscodeContainerName { get; set; } = Environment.GetEnvironmentVariable(SAMPLE_TRANSCODES_CONTAINER_NAME);
        public static string CoverArtContainerName { get; set; } = Environment.GetEnvironmentVariable(COVER_ART_CONTAINER_NAME);
        public static string TranscodeProfiles { get; set; } = Environment.GetEnvironmentVariable(TRANSCODE_PROFILES);
        public static string UserLibraryContainerName { get; set; } = Environment.GetEnvironmentVariable(USER_LIBRARY_CONTAINER_NAME);
        public static string StripeAPIKey { get; set; } = Environment.GetEnvironmentVariable(STRIPE_API_KEY);
        public static string ProvisionStripeStandardReturnUrl = Environment.GetEnvironmentVariable(PROVISION_STRIPE_STANDARD_RETURN_URL);
        public static string ProvisionStripeStandardRefreshUrl = Environment.GetEnvironmentVariable(PROVISION_STRIPE_STANDARD_REFRESH_URL);


    }
}