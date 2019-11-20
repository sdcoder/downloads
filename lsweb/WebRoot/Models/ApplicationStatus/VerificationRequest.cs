using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System.Collections.Generic;
using System.Linq;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class VerificationRequest
    {
        public string Caption { get; set; }

        public string Definition { get; set; }

        public VerificationRequestStatusLookup.VerificationRequestStatus Status { get; set; }

        public ReceivedStatus ReceivedStatus { get; set; }

        public SatisfiedStatus SatisfiedStatus { get; set; }

        public VerificationTypeLookup.VerificationType VerificationType { get; set; }

        public ApplicantTypeLookup.ApplicantType ApplicantType { get; set; }

        public int VerificationRequestId { get; set; }

        public string Instruction { get { return VerificationRequestInstructions.GetInstruction(VerificationType); } }

        public int NumberOfDocuments => FileNames?.Count ?? 0;

        public List<string> FileNames { get; set; }

        public string FileNamesAsJavaScriptArray => FileNames == null ? "[]" : $"[{string.Join(",", FileNames.Select(a => $"\"{a}\""))}]";

        public bool IsItemReceiveWaivedAndSatisfiedWaived =>
            ReceivedStatus == ReceivedStatus.Waived && SatisfiedStatus == SatisfiedStatus.Waived;

        private string _id = string.Empty;

        public string ID
        {
            get
            {
                if (_id == string.Empty)
                {
                    _id = "VI_" + VerificationType.ToString() + "_" + ApplicantType.ToString();
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }
    }
}