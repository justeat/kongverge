namespace Kongverge.DTOs
{
    public class KongvergeWorkflowArguments
    {
        public string InputFolder { get; set; }
        public string[] IgnoreTags { get; set; }
        public bool DryRun { get; set; } = false;
    }
}
