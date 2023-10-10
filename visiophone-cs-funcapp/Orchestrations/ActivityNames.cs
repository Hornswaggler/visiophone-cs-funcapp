namespace vp.orchestrations
{
    static class ActivityNames
    {
        public const string UpsertStripeData = "A_UpsertStripeData";
        public const string UpsertSamplePackMetadata = "A_UpsertSamplePackMetaData";
        //public const string UpsertSamplePackTransferImage = "A_UpsertSamplePackTransferImage";
        public const string CleanupStagingData = "A_CleanupStagingData";
        public const string ConvertSamplePackAssets = "A_ConvertSamplePackAssets";
        public const string MigrateSamplePackAssets = "A_MigrateSamplePackAssets";
        public const string RollbackSamplePackUpload = "A_RollbackUploadContainer";
        public const string RollbackStripeProduct = "A_RollbackStripeProduct";
        public const string RollbackSamplePackMetadata = "A_RollbackSamplePackMetadata";
    }
}

