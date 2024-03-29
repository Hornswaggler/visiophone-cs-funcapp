﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        //public static string SampleTranscodeContainerName = Environment.GetEnvironmentVariable("SAMPLE_TRANSCODES_CONTAINER_NAME");
        //TODO: Rename to "Staging or Uploads"
        //public static string UploadStagingBlobContainerName = Environment.GetEnvironmentVariable("SAMPLE_CONTAINER_NAME");
        //public static string AvatarBlobContainerName = Environment.GetEnvironmentVariable("STORAGE_CONTAINER_NAME_AVATARS");

        //Database
        public static string DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
        public static string SampleCollectionName = Environment.GetEnvironmentVariable("SAMPLE_COLLECTION_NAME");
        public static string PurchaseCollectionName = Environment.GetEnvironmentVariable("PURCHASE_COLLECTION_NAME");
        public static string StripeProfileCollectionName = Environment.GetEnvironmentVariable("STRIPE_PROFILE_COLLECTION_NAME");
        public static string SamplePackCollectionName = Environment.GetEnvironmentVariable("SAMPLE_PACK_COLLECTION_NAME");
        public static string SamplePackCollectionPartitionKey = "/sellerId";


        //Storage
        public static string UploadStagingBlobContainerName = Environment.GetEnvironmentVariable("UPLOAD_STAGING_CONTAINER");
        public static string SampleHDBlobContainerName = Environment.GetEnvironmentVariable("SAMPLE_FILES_CONTAINER_NAME");
        public static string SamplePreviewBlobContainerName = Environment.GetEnvironmentVariable("SAMPLE_PREVIEW_CONTAINER_NAME");
        public static string AvatarBlobContainerName = Environment.GetEnvironmentVariable("STORAGE_CONTAINER_NAME_AVATARS");
        public static string SamplePackCoverArtBlobContainerName = Environment.GetEnvironmentVariable("COVER_ART_CONTAINER_NAME");

        public static string CloudConvertAPIKey = Environment.GetEnvironmentVariable("CLOUD_CONVERT_API_KEY");
        public static string SamplePreviewClipLengthSS = Environment.GetEnvironmentVariable("SAMPLE_PREVIEW_CLIP_LENGTH_SS");

        //Import stuff...
        public static string BlobImportDirectoryName = Environment.GetEnvironmentVariable("BLOB_IMPORT_DIRECTORY_NAME");
        public static string BlobExportDirectoryName = Environment.GetEnvironmentVariable("BLOB_EXPORT_DIRECTORY_NAME");
        public static string ClipExportFileFormat = Environment.GetEnvironmentVariable("CLIP_EXPORT_FILE_FORMAT");

        public static string ImageExportFileFormat = Environment.GetEnvironmentVariable("IMAGE_EXPORT_FILE_FORMAT");
        public static int ImageExportWidth;
        public static int ImageExportHeight;
        public static int ImageExportQuality;

        public static int BufferSize  = 1 * 1024 * 1024;
        
        public static string StripeAPIKey  = Environment.GetEnvironmentVariable("STRIPE_API_KEY");
        public static string StripeWebhookSigningSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SIGNING_SECRET");
        
        //TODO: Is this being Used???
        public static string ProvisionStripeStandardReturnUrl = Environment.GetEnvironmentVariable("PROVISION_STRIPE_STANDARD_RETURN_URL");
        //TODO: Is this being used???
        public static string ProvisionStripeStandardRefreshUrl = Environment.GetEnvironmentVariable("PROVISION_STRIPE_STANDARD_REFRESH_URL");
        
        public static string PurchaseSampleStripeReturnUrl = Environment.GetEnvironmentVariable("PURCHASE_SAMPLE_STRIPE_RETURN_URL");
        public static string PurchaseSampleStripeCancelUrl = Environment.GetEnvironmentVariable("PURCHASE_SAMPLE_STRIPE_CANCEL_URL");

        public static int ResultsPerRequest;

        public static RetryOptions OrchestratorRetryOptions;

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

            if (int.TryParse(Environment.GetEnvironmentVariable("IMAGE_EXPORT_WIDTH"), out int imageExportWidthOut))
            {
                ImageExportWidth = imageExportWidthOut;
            }
            else
            {
                throw new Exception($"Failed to parse IMAGE_EXPORT_WIDTH");
            }

            if (int.TryParse(Environment.GetEnvironmentVariable("IMAGE_EXPORT_HEIGHT"), out int imageExportHeightOut))
            {
                ImageExportHeight = imageExportHeightOut;
            }
            else
            {
                throw new Exception($"Failed to parse IMAGE_EXPORT_HEIGHT");
            }

            if (int.TryParse(Environment.GetEnvironmentVariable("IMAGE_EXPORT_QUALITY"), out int imageExportQualityOut))
            {
                ImageExportQuality = imageExportQualityOut;
            }
            else
            {
                throw new Exception($"Failed to parse IMAGE_EXPORT_QUALITY");
            }

            if (
                int.TryParse(Environment.GetEnvironmentVariable("ORCHESTRATION_RETRY_SECONDS"), out int orchestrationRetrySecondsOut)
                && int.TryParse(Environment.GetEnvironmentVariable("ORCHESTRATION_RETRY_ATTEMPTS"), out int orchestrationRetryAttemptsOut))
            {
                OrchestratorRetryOptions = new RetryOptions(TimeSpan.FromSeconds(orchestrationRetrySecondsOut), orchestrationRetryAttemptsOut);
            }
            else
            {
                throw new Exception("ORCHESTRATION_RETRY_SECONDS or ORCHESTRATION_RETRY_ATTEMPTS");
            }
        }
    }
}