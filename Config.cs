using System;

namespace vp
{
    public class Config
    {
        private static string BASE_URL = "BASE_URL";
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
        private static string SAMPLE_PREVIEW_FILE_FORMAT = "SAMPLE_PREVIEW_FILE_FORMAT";
        private static string RESULTS_PER_REQUEST = "RESULTS_PER_REQUEST";
        private static string DATABASE_NAME = "DATABASE_NAME";
        private static string SAMPLE_PACK_COLLECTION_NAME = "SAMPLE_PACK_COLLECTION_NAME";
        private static string SAMPLE_COLLECTION_NAME = "SAMPLE_COLLECTION_NAME";
        private static string PURCHASE_COLLECTION_NAME = "PURCHASE_COLLECTION_NAME";
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
        public static string CHECKOUT_SESSION_COMPLETED_SECRET = "CHECKOUT_SESSION_COMPLETED_SECRET";
        public static string UPLOAD_STAGING_CONTAINER = "UPLOAD_STAGING_CONTAINER";
        public static string SAMPLE_FILES_CONTAINER_NAME = "SAMPLE_FILES_CONTAINER_NAME";
        public static int BufferSize  = 1 * 1024 * 1024;
        public static int PreviewBitrate = 128;

        public static string BaseUrl = Environment.GetEnvironmentVariable(BASE_URL);
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
        public static string CheckoutSessionCompletedSecret = Environment.GetEnvironmentVariable(CHECKOUT_SESSION_COMPLETED_SECRET);
        public static string SamplePreviewFileFormat = Environment.GetEnvironmentVariable(SAMPLE_PREVIEW_FILE_FORMAT);
        public static string DatabaseName = Environment.GetEnvironmentVariable(DATABASE_NAME);
        public static string SampleCollectionName = Environment.GetEnvironmentVariable(SAMPLE_COLLECTION_NAME);
        public static string PurchaseCollectionName = Environment.GetEnvironmentVariable(PURCHASE_COLLECTION_NAME);
        public static string SamplePackCollectionName = Environment.GetEnvironmentVariable(SAMPLE_PACK_COLLECTION_NAME);
        public static string UploadStagingContainerName = Environment.GetEnvironmentVariable(UPLOAD_STAGING_CONTAINER);
        public static string SampleFilesContainerName = Environment.GetEnvironmentVariable(SAMPLE_FILES_CONTAINER_NAME);

        public static int ResultsPerRequest;

        static Config() {
            if(int.TryParse(Environment.GetEnvironmentVariable(RESULTS_PER_REQUEST), out int resultsPerRequestOut))
            {
                ResultsPerRequest = resultsPerRequestOut;
            } else
            {
                throw new Exception($"Failed to parse {RESULTS_PER_REQUEST}");
            }
        }
    }
}