using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.TestingCommon.Builders;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.UnitTests.Builders;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class VerificationRequestsModelTests
    {
        [Test]
        public void Given_NoVerificationRequests_WhenRendered_ThenHasOnlyIdentityVerificationIsFalse()
        {
            var model = new VerificationRequestsModel();
            Assert.IsFalse(model.HasOnlyIdentityVerification());
        }

        [Test]
        public void Given_OnlyIdentityVerificationRequest_WhenRendered_ThenHasOnlyIdentityVerificationIsTrue()
        {
            var model = new VerificationRequestsModel();
            var customerUserIdData = new CustomerUserIdDataSet();
            customerUserIdData.EnforceConstraints = false;
            int applicationId = 1;

            var applicationRow = customerUserIdData.Application.NewApplicationRow();
            applicationRow.ApplicationId = applicationId;
            applicationRow.ApplicationStatusTypeId = (short) ApplicationStatusTypeLookup.ApplicationStatusType.InProcess;
            customerUserIdData.Application.Rows.Add(applicationRow);

            var detail = customerUserIdData.ApplicationDetail.NewApplicationDetailRow();
            detail.ApplicationId = applicationId;
            detail.LoanApplicationVersion = 1709m;
            customerUserIdData.ApplicationDetail.AddApplicationDetailRow(detail);

            var applicantRow = new ApplicantRowBuilder().Build(customerUserIdData, applicationRow);
            customerUserIdData.Applicant.Rows.Add(applicantRow);

            var verificationRequestRow = CreateVerificationRequestRow(customerUserIdData, applicationRow, 1, (short)VerificationTypeLookup.VerificationType.IdentityVerification, ApplicantTypeLookup.ApplicantType.Primary);
            customerUserIdData.VerificationRequest.Rows.Add(verificationRequestRow);

            var verificationRequestStatusRow = CreateVerificationRequestStatusRow(customerUserIdData, verificationRequestRow, 1);
            customerUserIdData.VerificationRequestStatus.Rows.Add(verificationRequestStatusRow);

            model.Populate(customerUserIdData, applicationRow.ApplicationId, PurposeOfLoanLookup.PurposeOfLoan.AutoPurchase);

            Assert.IsTrue(model.HasOnlyIdentityVerification());
        }

        [Test]
        public void Given_MultipleVerificationRequests_WhenRendered_ThenHasOnlyIdentityVerificationIsFalse()
        {
            var model = new VerificationRequestsModel();
            var customerUserIdData = new CustomerUserIdDataSet();
            customerUserIdData.EnforceConstraints = false;
            int applicationId = 1;

            var applicationRow = customerUserIdData.Application.NewApplicationRow();
            applicationRow.ApplicationId = applicationId;
            applicationRow.ApplicationStatusTypeId = (short)ApplicationStatusTypeLookup.ApplicationStatusType.InProcess;
            customerUserIdData.Application.Rows.Add(applicationRow);

            var detail = customerUserIdData.ApplicationDetail.NewApplicationDetailRow();
            detail.ApplicationId = applicationId;
            detail.LoanApplicationVersion = 1709m;
            customerUserIdData.ApplicationDetail.AddApplicationDetailRow(detail);

            var applicantRow = new ApplicantRowBuilder().Build(customerUserIdData, applicationRow);
            customerUserIdData.Applicant.Rows.Add(applicantRow);

            var verificationRequestRow1 = CreateVerificationRequestRow(customerUserIdData, applicationRow, 1, (short)VerificationTypeLookup.VerificationType.BankingOrBrokerage, ApplicantTypeLookup.ApplicantType.Primary);
            customerUserIdData.VerificationRequest.Rows.Add(verificationRequestRow1);

            var verificationRequestStatusRow1 = CreateVerificationRequestStatusRow(customerUserIdData, verificationRequestRow1, 1);
            customerUserIdData.VerificationRequestStatus.Rows.Add(verificationRequestStatusRow1);

            var verificationRequestRow2 = CreateVerificationRequestRow(customerUserIdData, applicationRow, 2, (short)VerificationTypeLookup.VerificationType.IdentityVerification, ApplicantTypeLookup.ApplicantType.Primary);
            customerUserIdData.VerificationRequest.Rows.Add(verificationRequestRow2);

            var verificationRequestStatusRow2 = CreateVerificationRequestStatusRow(customerUserIdData, verificationRequestRow1, 2);
            customerUserIdData.VerificationRequestStatus.Rows.Add(verificationRequestStatusRow2);

            model.Populate(customerUserIdData, applicationRow.ApplicationId, PurposeOfLoanLookup.PurposeOfLoan.AutoPurchase);

            Assert.IsFalse(model.HasOnlyIdentityVerification());
        }

        [Test]
        public void Given_MultipleIdentityVerificationRequests_WhenRendered_ThenHasOnlyIdentityVerificationIsTrue()
        {
            var model = new VerificationRequestsModel();
            var customerUserIdData = new CustomerUserIdDataSet();
            customerUserIdData.EnforceConstraints = false;
            int applicationId = 1;

            var applicationRow = customerUserIdData.Application.NewApplicationRow();
            applicationRow.ApplicationId = applicationId;
            applicationRow.ApplicationStatusTypeId = (short)ApplicationStatusTypeLookup.ApplicationStatusType.InProcess;
            customerUserIdData.Application.Rows.Add(applicationRow);

            var detail = customerUserIdData.ApplicationDetail.NewApplicationDetailRow();
            detail.ApplicationId = applicationId;
            detail.LoanApplicationVersion = 1709m;
            customerUserIdData.ApplicationDetail.AddApplicationDetailRow(detail);

            var applicantRowBuilder = new ApplicantRowBuilder();
            var primaryApplicantRow = applicantRowBuilder.WithApplicantType(ApplicantTypeLookup.ApplicantType.Primary).Build(customerUserIdData, applicationRow);
            customerUserIdData.Applicant.Rows.Add(primaryApplicantRow);

            var secondaryApplicantRow = applicantRowBuilder.WithApplicantType(ApplicantTypeLookup.ApplicantType.Secondary).Build(customerUserIdData, applicationRow);
            customerUserIdData.Applicant.Rows.Add(secondaryApplicantRow);

            var verificationRequestRow1 = CreateVerificationRequestRow(customerUserIdData, applicationRow, 1, (short)VerificationTypeLookup.VerificationType.IdentityVerification, ApplicantTypeLookup.ApplicantType.Primary);
            customerUserIdData.VerificationRequest.Rows.Add(verificationRequestRow1);

            var verificationRequestStatusRow1 = CreateVerificationRequestStatusRow(customerUserIdData, verificationRequestRow1, 1);
            customerUserIdData.VerificationRequestStatus.Rows.Add(verificationRequestStatusRow1);

            var verificationRequestRow2 = CreateVerificationRequestRow(customerUserIdData, applicationRow, 2, (short)VerificationTypeLookup.VerificationType.IdentityVerification, ApplicantTypeLookup.ApplicantType.Secondary);
            customerUserIdData.VerificationRequest.Rows.Add(verificationRequestRow2);

            var verificationRequestStatusRow2 = CreateVerificationRequestStatusRow(customerUserIdData, verificationRequestRow1, 2);
            customerUserIdData.VerificationRequestStatus.Rows.Add(verificationRequestStatusRow2);

            model.Populate(customerUserIdData, applicationRow.ApplicationId, PurposeOfLoanLookup.PurposeOfLoan.AutoPurchase);

            Assert.IsTrue(model.HasOnlyIdentityVerification());
        }

        [Test]
        public void Resubmitted_Documents_Are_Designated_Submitted()
        {
            var builder = new VerificationRequestDataSetBuilder()
                .WithPayStub(ApplicantTypeLookup.ApplicantType.Primary)
                .WithAcceptChanges();

            var version = 1711m;

            var vrds = builder.Build();
            var cuid = new CustomerUserIdDataSet();

            cuid.Merge(vrds);
            var model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest, 
                vrds.VerificationRequest, 
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(0, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Requested, model.Items().First().Status);

            cuid = new CustomerUserIdDataSet();
            cuid.EnforceConstraints = false;
            cuid.Merge(vrds);

            // Add uploaded doc
            var qfu = cuid.QueueFileUpload.NewQueueFileUploadRow();

            qfu.VerificationRequestId = vrds.VerificationRequest.Single().VerificationRequestId;
            qfu.FileName = "Panda.jpg";
            qfu.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Complete;
            cuid.QueueFileUpload.AddQueueFileUploadRow(qfu);

            model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest, 
                vrds.VerificationRequest, 
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(1, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Submitted, model.Items().First().Status);

            // Set resubmit status
            builder.WithAddedStatus(VerificationRequestStatusLookup.VerificationRequestStatus.Resubmit);
            vrds = builder.Build();
            cuid = new CustomerUserIdDataSet();
            cuid.EnforceConstraints = false;
            cuid.Merge(vrds);

            // Add a doc
            qfu = cuid.QueueFileUpload.NewQueueFileUploadRow();
            qfu.VerificationRequestId = vrds.VerificationRequest.Single().VerificationRequestId;
            qfu.FileName = "Panda.jpg";
            qfu.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Complete;
            cuid.QueueFileUpload.AddQueueFileUploadRow(qfu);

            model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest, 
                vrds.VerificationRequest, 
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(1, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Resubmit, model.Items().First().Status);

            // Add another doc
            qfu = cuid.QueueFileUpload.NewQueueFileUploadRow();
            qfu.VerificationRequestId = vrds.VerificationRequest.Single().VerificationRequestId;
            qfu.FileName = "PandaPanda.jpg";
            qfu.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Complete;
            qfu.CreatedDate = vrds.VerificationRequest.Single().CurrentStatusRow.CreatedDate.AddSeconds(1);
            cuid.QueueFileUpload.AddQueueFileUploadRow(qfu);

            model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest, 
                vrds.VerificationRequest, 
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(2, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Submitted, model.Items().First().Status);

            // Add a queued file that has errored out
            var upload = cuid.QueueFileUpload.NewQueueFileUploadRow();
            upload.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Error;
            upload.VerificationRequestId = cuid.VerificationRequest[0].VerificationRequestId;
            upload.FileName = "PandaPandaPanda.doc";
            upload.CreatedDate = DateTime.Now;
            cuid.QueueFileUpload.AddQueueFileUploadRow(upload);

            model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest,
                vrds.VerificationRequest,
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(2, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Submitted, model.Items().First().Status);

            // Add a queued file that has not errored out
            upload = cuid.QueueFileUpload.NewQueueFileUploadRow();
            upload.QueueItemStatus = QueueItemStatusTypeLookup.QueueItemStatusType.Ready;
            upload.VerificationRequestId = cuid.VerificationRequest[0].VerificationRequestId;
            upload.FileName = "PandaPandaPandaPanda.doc";
            upload.CreatedDate = DateTime.Now;
            cuid.QueueFileUpload.AddQueueFileUploadRow(upload);

            model = new VerificationRequestsModel.DocumentRequestModel(
                cuid.VerificationRequest,
                vrds.VerificationRequest,
                ApplicationStatusTypeLookup.ApplicationStatusType.PendingV,
                cuid.QueueFileUpload,
                version);

            Assert.AreEqual(1, model.Items().Count());
            Assert.AreEqual(3, model.Items().First().NumberOfDocuments);
            Assert.AreEqual(VerificationRequestStatusLookup.VerificationRequestStatus.Submitted, model.Items().First().Status);
        }

        private static CustomerUserIdDataSet.VerificationRequestStatusRow CreateVerificationRequestStatusRow(CustomerUserIdDataSet customerUserIdData, CustomerUserIdDataSet.VerificationRequestRow verificationRequestRow, short verificationRequestStatusId)
        {
            var verificationRequestStatusRow = customerUserIdData.VerificationRequestStatus.NewVerificationRequestStatusRow();
            verificationRequestStatusRow.VerificationRequestStatusId = verificationRequestStatusId;
            verificationRequestStatusRow.VerificationRequestRow = verificationRequestRow;
            return verificationRequestStatusRow;
        }

        private static CustomerUserIdDataSet.VerificationRequestRow CreateVerificationRequestRow(CustomerUserIdDataSet customerUserIdData, CustomerUserIdDataSet.ApplicationRow applicationRow, int verificationRequestId, short verificationTypeId, ApplicantTypeLookup.ApplicantType applicantType)
        {
            var verificationRequestRow = customerUserIdData.VerificationRequest.NewVerificationRequestRow();
            verificationRequestRow.VerificationRequestId = verificationRequestId;
            verificationRequestRow.ApplicationRow = applicationRow;
            verificationRequestRow.ApplicantTypeId = (short) applicantType;
            verificationRequestRow.VerificationTypeId = verificationTypeId;
            verificationRequestRow.IsSatisfiedViaChannelLink = false;
            return verificationRequestRow;
        }
    }
}