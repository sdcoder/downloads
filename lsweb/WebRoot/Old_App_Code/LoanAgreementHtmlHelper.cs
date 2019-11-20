using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Web;
using FirstAgain.Common.Text;
using FirstAgain.Web.UI;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.ServerState;

public static class LoanAgreementHtmlHelper
{
    public static string GetHtmlLoanAgreement(string html, int applicationId, DocumentStoreDataSet docsDataSet, CustomerUserIdDataSet cuidDataSet, 
        LoanOfferDataSet loanOfferDataSet = null)
    {
        LoanAgreementHtml agreement = new LoanAgreementHtml(html);

        byte[] appSignature = GetSignature(CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementAppSignature, applicationId, docsDataSet, cuidDataSet, loanOfferDataSet);
        SessionUtility.PrimarySignatureImageBytes = appSignature;

        agreement.Signature1ImgSrc = GetSignatureUrl(false);

        bool isJoint = cuidDataSet.Application.Where(a => a.ApplicationId == applicationId).Single().IsJoint;

        if (isJoint)
        {
            byte[] coAppSignature = GetSignature(CorrespondenceCategoryLookup.CorrespondenceCategory.LoanAgreementCoAppSignature, applicationId, docsDataSet, cuidDataSet, loanOfferDataSet);
            SessionUtility.SecondarySignatureImageBytes = coAppSignature;

            agreement.Signature2ImgSrc = GetSignatureUrl(true);
        }

        return agreement.InnerHtml;
    }

    private static string GetSignatureUrl(bool isCoApplicant)
    {
        return string.Format("/signature/display?isCoApplicant={0}&rnd={1}",
                      isCoApplicant,
                      Guid.NewGuid().ToString("N")); //random guid is used to prevent caching
    }

    private static byte[] GetSignature(CorrespondenceCategoryLookup.CorrespondenceCategory correspondenceCategory, 
        int applicationId, DocumentStoreDataSet docsDataSet, CustomerUserIdDataSet cuidDataSet, LoanOfferDataSet loanOfferDataSet)
    {
        DocumentStoreDataSet.DocumentStoreRow row;

        DocumentStoreDataSet.DocumentStoreDataTable docsTable = docsDataSet.DocumentStore;

        // If the DocumentStore table in the DocumentStoreDataSet is empty, use the DocumentStore table in 
        // CustomerUserIdDataSet.  The website should really not use multiple DataSet types for storing 
        // the same data in session.
        if (docsTable.Count == 0 && cuidDataSet.DocumentStore.Count > 0)
        {
            DocumentStoreDataSet ds = new DocumentStoreDataSet();
            docsTable = ds.DocumentStore;
            docsTable.Merge(cuidDataSet.DocumentStore);
        }
        
        if (loanOfferDataSet == null)
        {
            row = docsTable
                .Where(a => a.ApplicationId == applicationId && a.CorrespondenceCategory == correspondenceCategory)
                .OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if (row == null)
                throw new MissingSignatureException(string.Format("There is no {0} in DocumentStore for ApplicationId {1}", correspondenceCategory, applicationId));
        }
        else
        {
            int ltrId = loanOfferDataSet == null ? 0 : loanOfferDataSet.LatestApprovedLoanTerms.LoanTermsRequestId;

            row = docsTable
                .Where(a => !a.IsLoanTermsRequestIdNull() && a.LoanTermsRequestId == ltrId && a.CorrespondenceCategory == correspondenceCategory)
                .OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if (row == null)
                throw new MissingSignatureException(string.Format("There is no {0} in DocumentStore for LoanTermsRequestId {1}", correspondenceCategory, ltrId));
        }

        return CorrespondenceServiceCorrespondenceOperations.GetEDoc(row.EdocId);
    }

    public class MissingSignatureException : InvalidOperationException
    {
        public MissingSignatureException(string err)
            : base(err) { }
    }
}