using System;

namespace LightStreamWeb.ServerState
{
    [Serializable]
    public class SubmittedApplicationResult
    {
        public bool IsSubmitted { get; set; }
        public bool SubmitFailed { get; set; }
    }
}