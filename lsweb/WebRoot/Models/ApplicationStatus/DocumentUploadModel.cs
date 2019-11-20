using FirstAgain.Common;
using FirstAgain.Common.Logging;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class DocumentUploadModel
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }

        // TODO e3p0 9/10 release - remove verificationType and applicantType, use verificationRequestId only
        public void DoUpload(IEnumerable<HttpPostedFileBase> files,
                                ICurrentUser webUser,
                                CustomerUserIdDataSet customerData,
                                int applicationId,
                                int verificationRequestId)
        {
            string[] allowedFileExtensions = { ".jpg", ".jpeg", ".gif", ".png", ".tif", ".tiff", ".pdf" };
            var docs = new List<Document>(2);

            // default
            ErrorMessage = "Please review the file format and resubmit your documents.";

            if (files != null)
            {
                if (files.All(f => f.ContentLength == 0))
                {
                    LightStreamLogger.WriteWarning("DocumentUploadModel.DoUpload: All files have ContentLenght=0. ApplicationId: {ApplicationId}", applicationId);
                }

                // check that all are in the proper format
                foreach (var file in files.Where(f => f.ContentLength > 0))
                {
                    string cleanFileName = string.Join("", file.FileName.Split(Path.GetInvalidFileNameChars()));
                    if (allowedFileExtensions.Contains(Path.GetExtension(cleanFileName), StringComparer.OrdinalIgnoreCase))
                    {
                        Document doc = new Document();
                        doc.SetContent(file.InputStream);
                        doc.FileName = cleanFileName;
                        docs.Add(doc);
                    }
                    else
                    {
                        ErrorMessage = $"Please review the format for document {cleanFileName} and re-submit.";
                    }
                }

                var applicationRow = customerData.Application.FirstOrDefault(x => x.ApplicationId == applicationId);
                var webActivity = Helpers.WebActivityDataSetHelper.Populate(webUser, applicationRow.ApplicationStatusType);

                foreach (var doc in docs)
                {

                    var result = CorrespondenceServiceCorrespondenceOperations.UploadDocument(applicationId, verificationRequestId, doc, webActivity);

                    switch (result)
                    {
                        case DocumentConversionFailureReason.PasswordProtected:
                            ErrorMessage = $"Document \"{doc.FileName}\" is password protected. Please submit a file that is not password protected.";
                            break;
                        case DocumentConversionFailureReason.FileIsDamagedOrCorrupt:
                        case DocumentConversionFailureReason.UnrecognizedDocumentType:
                        case DocumentConversionFailureReason.Unknown:
                            ErrorMessage = $"Document \"{doc.FileName}\" did not fully upload or the content of the document is corrupt. Please re-submit the document or upload another file.";
                            break;
                        case DocumentConversionFailureReason.FileIsDuplicate:
                            ErrorMessage = $"Document \"{doc.FileName}\" was already submitted.";
                            break;
                        default:
                            ErrorMessage = null;
                            break;
                    }
                }
            }
            else
            {   
                LightStreamLogger.WriteWarning("DocumentUploadModel.DoUpload: No files were received to upload. ApplicationId: {ApplicationId}", applicationId);
            }

            Success = string.IsNullOrEmpty(ErrorMessage);
            FileName = string.Join(", ", docs.Select(a => a.FileName).ToArray());
        }

        internal void RecordDocumentUploads(CustomerUserIdDataSet customerData, int applicationId, int verificationRequestId)
        {
            if(Success)
            {
                var vrRow = customerData.VerificationRequest.Single(vr => vr.VerificationRequestId == verificationRequestId);
                CustomerUserIdDataSet.VerificationRequestStatusRow vrsRow = vrRow.NewRelatedVerificationRequestStatusRow();
                vrsRow.VerificationRequestStatus = VerificationRequestStatusLookup.VerificationRequestStatus.Submitted;
                customerData.VerificationRequestStatus.AddVerificationRequestStatusRow(vrsRow);

                var qRow = customerData.QueueFileUpload.NewQueueFileUploadRow();
                qRow.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Ready;
                qRow.ApplicationId = applicationId;
                qRow.VerificationRequestId = verificationRequestId;
                qRow.FileName = FileName;
                customerData.QueueFileUpload.AddQueueFileUploadRow(qRow);
                qRow.AcceptChanges();
            }
        }
    }
}