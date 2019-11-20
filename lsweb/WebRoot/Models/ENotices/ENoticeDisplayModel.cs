using FirstAgain.Common.Text;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using FirstAgain.Correspondence.SharedTypes;

namespace LightStreamWeb.Models.ENotices
{
    public class ENoticeDisplayModel
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public void LoadEnoticeContent(CustomerUserIdDataSet customerData, long eDocId)
        {
            var row = DoAccessCheck(customerData, eDocId);
            
            string formatType;
            byte[] doc = CorrespondenceServiceCorrespondenceOperations.GetEDoc(eDocId, out formatType);

            Content = FixUpEdocHtml(TextDecoder.GetString(doc));
            Title = GetDocumentName(customerData, row);
        }

        public void LoadLoanAgreement(int applicationId, CustomerUserIdDataSet customerData, int? id)
        {
            var documentStore = CorrespondenceServiceCorrespondenceOperations.GetApplicationDocumentStore(applicationId);

            DocumentStoreDataSet.DocumentStoreRow agreementHtml;
            if (id.HasValue)
            {
                agreementHtml = documentStore.DocumentStore
                .Where(a => a.ApplicationId == applicationId && a.EdocId == id)
                .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            }
            else
            {
                agreementHtml = documentStore.DocumentStore
                    .Where(a => a.ApplicationId == applicationId && a.CorrespondenceCategory == CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementHtml)
                    .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            }

            byte[] htmlContent = CorrespondenceServiceCorrespondenceOperations.GetEDoc(agreementHtml.EdocId);
            string html = FirstAgain.Common.Text.TextDecoder.GetString(htmlContent);
            Content = LoanAgreementHtmlHelper.GetHtmlLoanAgreement(html, applicationId, documentStore, customerData);

            Title = "Loan Agreement";
        }

        private string GetDocumentName(CustomerUserIdDataSet customerData, CustomerUserIdDataSet.DocumentStoreRow row)
        {
            CustomerUserIdDataSet.ApplicantRow applicant = null;
            if (!row.IsApplicantTypeIdNull())
            {
                applicant = customerData.Applicant.Single(a => a.ApplicantTypeId == row.ApplicantTypeId && a.ApplicationId == row.ApplicationId);
            }

            string documentName;

            switch (row.CorrespondenceCategory)
            {
                case CorrespondenceCategoryLookup.CorrespondenceCategory.DeclineNotice:
                    documentName = "Decline Notice";
                    break;
                case CorrespondenceCategoryLookup.CorrespondenceCategory.CreditScoreDisclosureHtml:
                    documentName = "Credit Score Disclosure Notice";
                    break;
                default:
                    documentName = CorrespondenceCategoryLookup.GetCaption(row.CorrespondenceCategory);
                    break;
            }
            if (applicant != null)
            {
                return string.Format("{0:MM/dd/yyyy} {1} {2} {3}", row.CreatedDate, documentName, applicant.FirstName, applicant.LastName);
            }

            return documentName;
        }


        public byte[] GetEnoticPdf(CustomerUserIdDataSet customerData, long eDocId, out string fileName)
        {
            var row = DoAccessCheck(customerData, eDocId);
            fileName = row.CorrespondenceCategory.ToString();

            string formatType;
            return CorrespondenceServiceCorrespondenceOperations.GetEDoc(eDocId, out formatType);
        }

        protected string FixUpEdocHtml(string html)
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(html)))
            {
                reader.Namespaces = false;
                reader.DtdProcessing = DtdProcessing.Ignore;
                reader.ReadToFollowing("body");

                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(reader.ReadOuterXml());
                return doc.DocumentElement.InnerXml;
            }
        }

        private static FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.DocumentStoreRow DoAccessCheck(CustomerUserIdDataSet customerData, long eDocId)
        {
            //cludge to fix BUG-44612
            var edoc = customerData.DocumentStore.SingleOrDefault(x => x.EdocId == eDocId &&
                                                                       (x.IsDocumentDescriptionNull() ||
                                                                       !x.DocumentDescription.Contains("Copied from original")));

            if (edoc != null)
                return edoc;

            LightStreamLogger.WriteError(string.Format("EDocID {0} not found in CustomerUserIdDataSet for UserId {1}", eDocId, customerData.UserId));
            throw new Exception("Document not found");
        }

    }
}