namespace visiophone_cs_funcapp.Orchestrations.UpsertSamplePack
{
    public static class UpsertSamplePackStatus
    {
        public const string Queued = "QUEUED";
        public const string Converting = "CONVERTING";
        public const string Migrating = "MIGRATING";
        public const string Cleanup = "CLEANUP";
        public const string Failed = "FAILED";
    }
}
