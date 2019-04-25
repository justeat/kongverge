namespace Kongverge.DTOs
{
    public class KongvergeWorkflowArguments
    {
        public string InputFolder { get; set; }
        public bool DryRun { get; set; } = false;
        public bool FaultTolerance { get; set; } = false;
    }
}
