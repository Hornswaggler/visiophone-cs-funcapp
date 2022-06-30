namespace DurableFunctionVideoProcessor;

static class OrchestratorNames
{
    public const string ProcessVideo = "OProcessVideo";
    public const string Transcode = "OTranscode";
    public const string GetApprovalResult = "OGetApprovalResult";
    public const string PeriodicTask = "OPeriodicTask";
}