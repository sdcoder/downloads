using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ENotices
{
    public class ENoticesModalModel
    {
        private int _applicationId;
        protected CustomerUserIdDataSet _customerUserIdDataSet = null;
        private CorrespondenceCategoryLookup.CorrespondenceCategory? _suppressNoticeType = null;

        public ENoticesModalModel(int applicationId, CustomerUserIdDataSet customerUserIdDataSet, CorrespondenceCategoryLookup.CorrespondenceCategory? suppressNoticeType = null)
        {
            _applicationId = applicationId;
            _customerUserIdDataSet = customerUserIdDataSet;
            _suppressNoticeType = suppressNoticeType;
        }

        public bool IsJoint()
        {
            return _customerUserIdDataSet.Application.Single(a => a.ApplicationId == _applicationId).IsJoint;
        }

        public bool HasMultipleApplicants
        {
            get
            {
                return Docs.Select(x => x.ApplicantId).Distinct().Count() > 1;
            }
        }

        public IEnumerable<ENoticeRow> Docs
        {
            get
            {
                var creditScoreDisclosureApplicantsSeen = new HashSet<ApplicantTypeLookup.ApplicantType>();

                return _customerUserIdDataSet.DocumentStore
                                .Where(a => a.ApplicationId == _applicationId && a.CorrespondenceCategory != _suppressNoticeType && (
                                    (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice && (a.IsViewable || DateTime.Now < a.CreatedDate.AddHours(BusinessConstants.Instance.DeclineFormDelayHours))
                                    || (a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml && a.IsViewable))
                                    )).OrderByDescending(a => Math.Round((a.CreatedDate - DateTime.MinValue).TotalMinutes)).ThenBy(a => a.ApplicantTypeId).ThenBy(a => a.CorrespondenceCategoryId).ToList()
                                .Where(
                                    x => x.CorrespondenceCategory != CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml
                                    || creditScoreDisclosureApplicantsSeen.Add(x.ApplicantType))
                                .Select(x => PopulateENoticeRow(x));
            }
        }

        private ENoticeRow PopulateENoticeRow(CustomerUserIdDataSet.DocumentStoreRow row)
        {
            var result = new ENoticeRow();
            switch (row.CorrespondenceCategory)
            {
                case CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice:
                    result.ENoticeTitle = "Decline Notice";
                    break;
                case CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml:
                    result.ENoticeTitle = "Credit Score Disclosure Notice";
                    result.PdfVersionEDocId = GetCreditScoreDisclosurePdf(row);
                    break;
                default:
                    result.ENoticeTitle = CorrespondenceCategoryLookup.GetCaption(row.CorrespondenceCategory);
                    break;
            }

            result.ENoticeDate = string.Format("{0:MM/dd/yyyy}", row.CreatedDate);
            result.EDocId = row.EdocId;
            result.Category = row.CorrespondenceCategory;

            var applicant = _customerUserIdDataSet.Applicant.Single(a => a.ApplicationId == _applicationId && a.ApplicantTypeId == row.ApplicantTypeId);
            result.ApplicantName = string.Format("{0} {1}", applicant.FirstName, applicant.LastName);
            result.ApplicantId = applicant.ApplicantId;

            return result;
        }

        private long? GetCreditScoreDisclosurePdf(CustomerUserIdDataSet.DocumentStoreRow row)
        {
            CustomerUserIdDataSet.DocumentStoreRow pdfRow = _customerUserIdDataSet.DocumentStore.Where(a =>
                a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosurePdf && 
                (a.IsDocumentDescriptionNull() || !a.DocumentDescription.Contains("Copied from original")) &&
                a.ApplicationId == row.ApplicationId && a.ApplicantTypeId == row.ApplicantTypeId)
                .OrderByDescending(a => a.CreatedDate).ThenByDescending(a => a.EdocId)
                .FirstOrDefault();

            return pdfRow == null ? null : (long?)pdfRow.EdocId;
        }

        public class ENoticeRow
        {
            public string ENoticeDate { get; set; }
            public string ENoticeTitle { get; set; }

            public long EDocId { get; set; }
            public long? PdfVersionEDocId { get; set; }
            public string ApplicantName { get; set; }
            public int ApplicantId { get; set; }
            public CorrespondenceCategoryLookup.CorrespondenceCategory Category { get; set; }

            public bool HasPDF
            {
                get
                {
                    return PdfVersionEDocId.HasValue;
                }
            }
        }
    }
}