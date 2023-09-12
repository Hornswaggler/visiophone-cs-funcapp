using System;

namespace vp
{
    public class Config
    {
        public static string BaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        public static string StorageBaseUrl = Environment.GetEnvironmentVariable("STORAGE_BASE_URL");

        public static string AuthSignInAuthority = Environment.GetEnvironmentVariable("AUTH_SIGN_IN_AUTHORITY");
        public static string AuthClaimSignInAuthority = Environment.GetEnvironmentVariable("AUTH_CLAIM_SIGN_IN_AUTHORITY");
        public static string AuthClaimId = Environment.GetEnvironmentVariable("AUTH_CLAIM_ID");
        public static string AuthValidAudience = Environment.GetEnvironmentVariable("AUTH_VALID_AUDIENCE");
        public static string AuthValidIssuer = Environment.GetEnvironmentVariable("AUTH_VALID_ISSUER");
        public static string AuthOpenidConnectConfig = Environment.GetEnvironmentVariable("AUTH_OPENID_CONNECT_CONFIG");

        public static string StorageAccountName = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME");
        public static string StorageAccountKey = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY");
        public static string StorageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
        public static string StorageSubscriptionId = Environment.GetEnvironmentVariable("STORAGE_SUBSCRIPTION_ID");
        public static string StorageResourceGroupName = Environment.GetEnvironmentVariable("STORAGE_RESOURCE_GROUP_NAME");

        //$"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey};EndpointSuffix=core.windows.net";
        public static string CosmosEndpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
        public static string CosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
        public static string CosmosConnectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");

        public static string SampleFilesContainerName = Environment.GetEnvironmentVariable("SAMPLE_FILES_CONTAINER_NAME");
        public static string CoverArtContainerName = Environment.GetEnvironmentVariable("COVER_ART_CONTAINER_NAME");
        public static string SampleTranscodeContainerName = Environment.GetEnvironmentVariable("SAMPLE_TRANSCODES_CONTAINER_NAME");
        //TODO: Rename to "Staging or Uploads"
        public static string SampleBlobContainerName = Environment.GetEnvironmentVariable("SAMPLE_CONTAINER_NAME");
        public static string StorageContainerNameAvatars = Environment.GetEnvironmentVariable("STORAGE_CONTAINER_NAME_AVATARS");

        public static string DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
        public static string SampleCollectionName = Environment.GetEnvironmentVariable("SAMPLE_COLLECTION_NAME");
        public static string PurchaseCollectionName = Environment.GetEnvironmentVariable("PURCHASE_COLLECTION_NAME");
        public static string StripeProfileCollectionName = Environment.GetEnvironmentVariable("STRIPE_PROFILE_COLLECTION_NAME");
        public static string SamplePackCollectionName = Environment.GetEnvironmentVariable("SAMPLE_PACK_COLLECTION_NAME");
        public static string UploadStagingContainerName = Environment.GetEnvironmentVariable("UPLOAD_STAGING_CONTAINER");

        public static string CloudConvertAPIKey = Environment.GetEnvironmentVariable("CLOUD_CONVERT_API_KEY");

        //TODO: Cleanup FFMPEG stuff...
        public static string FfmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        public static string Home = Environment.GetEnvironmentVariable("HOME_PATH");
        public static int BufferSize  = 1 * 1024 * 1024;
        public static int PreviewBitrate = 128;
        public static string SamplePreviewFileFormat = Environment.GetEnvironmentVariable("SAMPLE_PREVIEW_FILE_FORMAT");
        public static string TranscodeProfiles  = Environment.GetEnvironmentVariable("TRANSCODE_PROFILES");
        
        public static string StripeAPIKey  = Environment.GetEnvironmentVariable("STRIPE_API_KEY");
        public static string StripeWebhookSigningSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SIGNING_SECRET");
        public static string ProvisionStripeStandardReturnUrl = Environment.GetEnvironmentVariable("PROVISION_STRIPE_STANDARD_RETURN_URL");
        public static string ProvisionStripeStandardRefreshUrl = Environment.GetEnvironmentVariable("PROVISION_STRIPE_STANDARD_REFRESH_URL");
        public static string PurchaseSampleStripeReturnUrl = Environment.GetEnvironmentVariable("PURCHASE_SAMPLE_STRIPE_RETURN_URL");
        public static string PurchaseSampleStripeCancelUrl = Environment.GetEnvironmentVariable("PURCHASE_SAMPLE_STRIPE_CANCEL_URL");

        public static int ResultsPerRequest;

        static Config()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("RESULTS_PER_REQUEST"), out int resultsPerRequestOut))
            {
                ResultsPerRequest = resultsPerRequestOut;
            }
            else
            {
                throw new Exception($"Failed to parse RESULTS_PER_REQUEST");
            }
        }
    }
}