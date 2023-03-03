namespace vp.orchestrations
{
    static class ActivityNames
    {
        public const string StageAudioForTranscode = "A_StageAudioForTranscode";
        public const string PublishAudio = "A_PublishAudio";
        public const string GetTranscodeProfiles = "A_GetTranscodeProfiles";
        public const string TranscodeAudio = "A_TranscodeAudio";
        public const string UpsertStripeData = "A_UpsertStripeData";
        public const string UpsertSamplePackMetadata = "A_UpsertSamplePackMetaData";
        public const string UpsertSamplePackTransferImage = "A_UpsertSamplePackTransferImage";
        public const string CleanupStagingData = "CleanupStagingData";
    }
}

