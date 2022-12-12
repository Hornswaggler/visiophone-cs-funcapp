using System;
using System.Collections.Generic;

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
        private static string PURCHASE_SAMPLE_STRIPE_CANCEL_URL = "PURCHASE_SAMPLE_STRIPE_CANCEL_URL";
        public static string FFMPEG_PATH = "site\\wwwroot\\Tools\\ffmpeg.exe";
        public static string HOME = "..\\Tools\\ffmpeg.exe";
        public static string WAV_CONTENT_TYPE = "audio/wav";
        public static string STRIPE_API_KEY = "STRIPE_API_KEY";
        public static string AUTH_CLAIM_SIGN_IN_AUTHORITY = "AUTH_CLAIM_SIGN_IN_AUTHORITY";
        public static string AUTH_CLAIM_ID = "AUTH_CLAIM_ID";
        public static string AUTH_SIGN_IN_AUTHORITY = "AUTH_SIGN_IN_AUTHORITY";
        public static string PURCHASE_SAMPLE_STRIPE_RETURN_URL = "PURCHASE_SAMPLE_STRIPE_RETURN_URL";
        public static string AUTH_VALID_AUDIENCE = "AUTH_VALID_AUDIENCE";
        public static string AUTH_VALID_ISSUER = "AUTH_VALID_ISSUER";
        public static string AUTH_OPENID_CONNECT_CONFIG = "AUTH_OPENID_CONNECT_CONFIG";
        public static int BufferSize  = 1 * 1024 * 1024;
        public static int PreviewBitrate = 128;

        public static string StorageConnectionString  = Environment.GetEnvironmentVariable(STORAGE_CONNECTION_STRING);
        public static string SampleBlobContainerName  = Environment.GetEnvironmentVariable(SAMPLE_CONTAINER_NAME);
        public static string MongoConnectionString  = Environment.GetEnvironmentVariable(MONGO_CONNECTION_STRING);
        public static string SampleTranscodeContainerName  = Environment.GetEnvironmentVariable(SAMPLE_TRANSCODES_CONTAINER_NAME);
        public static string CoverArtContainerName  = Environment.GetEnvironmentVariable(COVER_ART_CONTAINER_NAME);
        public static string TranscodeProfiles  = Environment.GetEnvironmentVariable(TRANSCODE_PROFILES);
        public static string UserLibraryContainerName  = Environment.GetEnvironmentVariable(USER_LIBRARY_CONTAINER_NAME);
        public static string StripeAPIKey  = Environment.GetEnvironmentVariable(STRIPE_API_KEY);
        public static string ProvisionStripeStandardReturnUrl = Environment.GetEnvironmentVariable(PROVISION_STRIPE_STANDARD_RETURN_URL);
        public static string ProvisionStripeStandardRefreshUrl = Environment.GetEnvironmentVariable(PROVISION_STRIPE_STANDARD_REFRESH_URL);
        public static string PurchaseSampleStripeReturnUrl = Environment.GetEnvironmentVariable(PURCHASE_SAMPLE_STRIPE_RETURN_URL);
        public static string AuthSignInAuthority  = Environment.GetEnvironmentVariable(AUTH_SIGN_IN_AUTHORITY);
        public static string PurchaseSampleStripeCancelUrl = Environment.GetEnvironmentVariable(PURCHASE_SAMPLE_STRIPE_CANCEL_URL);
        public static string AuthClaimSignInAuthority  = Environment.GetEnvironmentVariable(AUTH_CLAIM_SIGN_IN_AUTHORITY);
        public static string AuthClaimId  = Environment.GetEnvironmentVariable(AUTH_CLAIM_ID);
        public static string AuthValidAudience  = Environment.GetEnvironmentVariable(AUTH_VALID_AUDIENCE);
        public static string AuthValidIssuer = Environment.GetEnvironmentVariable(AUTH_VALID_ISSUER);
        public static string AuthOpenidConnectConfig = Environment.GetEnvironmentVariable(AUTH_OPENID_CONNECT_CONFIG);
    }
}