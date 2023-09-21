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
    }
}

