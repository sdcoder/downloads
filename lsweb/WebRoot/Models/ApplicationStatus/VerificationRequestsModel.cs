using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;

using VerificationStatus = FirstAgain.Domain.Lookups.FirstLook.VerificationRequestStatusLookup.VerificationRequestStatus;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LightStreamWeb.Models.Middleware;
using Ninject;
using LightStreamWeb.Filters;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class VerificationRequestsModel
    {
        private CustomerUserIdDataSet _customerData = null;
        private CustomerUserIdDataSet.ApplicationRow _applicationRow = null;
        private IEnumerable<CustomerUserIdDataSet.VerificationRequestRow> _verificationRequestRows = null;
        private int _numberOfParts = 0;
        private int _currentPartNumber = 0;

        public SubjectPropertyAddressModel SubjectPropertyAddress { get; private set; }
        public CollateralInformationRequestModel CollateralInformation { get; private set; }
        public CustomerIdentificationRequestModel CustomerIdentification { get; private set; }
        public DocumentRequestModel Documents { get; private set; }

        public bool AllDocumentsReceivedOrWaived
        {
            get
            {
                if (Documents.Any())
                {
                    return Documents.Items().All(i => i.Status == VerificationStatus.Received || i.Status == VerificationStatus.Waived);
                }
                else { return false; }
            }
        }

        public bool AllDocumentsAreInASatisfiedStatus
        {
            get
            {
                if (Documents.Any())
                {
                    return Documents.Items().All(i => i.Status.IsOneOf(VerificationStatus.Satisfied,
                                                                       VerificationStatus.NotSatisfied,
                                                                       VerificationStatus.Waived));
                }
                else { return false; }
            }
        }

        public bool AnyDocumentReceivedWaivedAndSatisfiedWaived
        {
            get
            {
                if (Documents.Any())
                {
                    return Documents.Items().Any(d => d.IsItemReceiveWaivedAndSatisfiedWaived);
                }
                else { return false; }
            }
        }

        public string CustomerInstruction { get; private set; }
        public int ApplicationId { get; private set; }

        public ApplicationStatusTypeLookup.ApplicationStatusType CurrentStatus { get; private set; }

        public string CdnBaseUrl { get; set; }

        public void Populate(CustomerUserIdDataSet customerData, int applicationId, PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan, LoanApplicationDataSet lads = null)
        {
            this.ApplicationId = applicationId;
            _customerData = customerData;
            _applicationRow = customerData.Application.First(x => x.ApplicationId == applicationId);
            var detail = customerData.ApplicationDetail.First(x => x.ApplicationId == applicationId);

            CurrentStatus = _applicationRow.ApplicationStatusType;

            CustomerInstruction = _applicationRow.GetLatestVerificationCustomerInstruction(_applicationRow.FundingDate);

            _verificationRequestRows = _applicationRow.GetVerificationRequestRows();

            var fileUploadRows = customerData.QueueFileUpload.Where(a => a.ApplicationId == applicationId).ToList();

            if (!CurrentStatus.HasBeenFunded())
            {
                CustomerIdentification = new CustomerIdentificationRequestModel(_verificationRequestRows, _applicationRow);
                CollateralInformation = new CollateralInformationRequestModel(_verificationRequestRows, _applicationRow, purposeOfLoan);
                SubjectPropertyAddress = new SubjectPropertyAddressModel(_verificationRequestRows, _applicationRow, purposeOfLoan, lads);
            }
            else
            {
                CustomerIdentification = new CustomerIdentificationRequestModel(null, null);
                CollateralInformation = new CollateralInformationRequestModel(null, null, PurposeOfLoanLookup.PurposeOfLoan.NotSelected);
                SubjectPropertyAddress = new SubjectPropertyAddressModel(null, null, purposeOfLoan, null);
            }
            Documents = new DocumentRequestModel(_verificationRequestRows,
                                                 ConvertToVerificationRequestRows(applicationId, customerData),
                                                 CurrentStatus,
                                                 fileUploadRows,
                                                 detail.LoanApplicationVersion);

            _numberOfParts =
                Convert.ToInt32(CollateralInformation.IsRequired()) +
                Convert.ToInt32(SubjectPropertyAddress.IsRequired()) +
                Convert.ToInt32(CustomerIdentification.IsRequired()) +
                Convert.ToInt32(Documents.IsRequired());

            CdnBaseUrl = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
        }

        public string GetAllItemsCompletedMessage()
        {
            if (HasOnlyIdentityVerification())
            {
                return "Thank you for completing the Identity Verification step";
            }
            else if (HasOnlySubjectPropertyAddressVerification())
            {
                return "Thank you for submitting the information for the Property being Improved";
            }

            if (CustomerIdentification.Items().Any() && Documents.Any())
            {
                return "Thank you for completing the Identity Protection step and submitting the requested verification document(s)";
            }

            return "Thank you for submitting the requested verification document(s)";
        }

        public string GetPartNumber(bool advance = true)
        {
            if (_numberOfParts == 1)
            {
                return string.Empty;
            }

            if (advance)
                _currentPartNumber++;

            return string.Format("Part {0}: ", _currentPartNumber);
        }

        public DateTime DueDate
        {
            get
            {
                return DateUtility.GetVerificationDocDeadlineDate(_applicationRow.ApplicationId, _applicationRow.ApplicationStatusType, _customerData).GetValueOrDefault();
            }
        }

        public string Description
        {
            get
            {
                List<string> items = new List<string>();
                if (CustomerIdentification.IsRequired())
                {
                    items.Add("identity verification");
                }
                if (SubjectPropertyAddress.IsRequired())
                {
                    items.Add("property verification");
                }
                if (CollateralInformation.IsRequired())
                {
                    items.Add("vehicle verification");
                }
                if (Documents.IsRequired())
                {
                    items.Add(Documents.PluralOrSingluarString().ToLower());
                }

                switch (items.Count)
                {
                    case 0:
                    case 1:
                        return items.FirstOrDefault();
                    case 2:
                        return items.First() + " and " + items.Last();
                    default:
                        return string.Join(", ", items.Take(items.Count - 1).ToArray()) + " and  " + items.Last();
                }
            }
        }

        public bool Any()
        {
            return CustomerIdentification.IsRequired() || SubjectPropertyAddress.IsRequired() || CollateralInformation.IsRequired() || Documents.IsRequired();
        }

        public bool HasOnlyIdentityVerification()
        {
            return _verificationRequestRows.IsNotNull() &&
                   _verificationRequestRows.Any(vrr => vrr.VerificationType == VerificationTypeLookup.VerificationType.IdentityVerification) &&
                   _verificationRequestRows.All(vrr => vrr.VerificationType == VerificationTypeLookup.VerificationType.IdentityVerification);
        }

        public bool HasOnlySubjectPropertyAddressVerification()
        {
            return _verificationRequestRows.IsNotNull() &&
                   _verificationRequestRows.Any(vrr => vrr.VerificationType == VerificationTypeLookup.VerificationType.SubjectPropertyAddress) &&
                   _verificationRequestRows.All(vrr => vrr.VerificationType == VerificationTypeLookup.VerificationType.SubjectPropertyAddress);
        }

        private IEnumerable<VerificationRequestDataSet.VerificationRequestRow> ConvertToVerificationRequestRows(int applicationId, CustomerUserIdDataSet customerData)
        {
            var vrds = new VerificationRequestDataSet();

            var application = vrds.Application.NewApplicationRow();

            application.ApplicationId = applicationId;
            application.ApplicationStatusType = CurrentStatus;
            vrds.Application.AddApplicationRow(application);

            vrds.Merge(customerData.VerificationRequest.Where(vr => vr.ApplicationId == applicationId).ToArray());
            vrds.Merge(customerData.VerificationRequestStatus.Where(a => vrds.VerificationRequest.Any(r => r.VerificationRequestId == a.VerificationRequestId)).ToArray());
            vrds.Merge(customerData.VerificationRequestDocumentation.Where(vrd => vrd.ApplicationId == applicationId).ToArray());

            return vrds.Application.FirstOrDefault(x => x.ApplicationId == applicationId).GetVerificationRequestRows();
        }

        public class CustomerIdentificationRequestModel
        {
            private IEnumerable<VerificationRequest> _verificationRequests = null;

            public bool Received { get; private set; }

            public IEnumerable<VerificationRequest> Items() =>
                (_verificationRequests != null) ? _verificationRequests : new List<VerificationRequest>();

            public bool IsRequired() =>
                _verificationRequests != null &&
                _verificationRequests.Any(x => !x.Status.IsReceivedOrWaitingToBeReceived());

            public CustomerIdentificationRequestModel(IEnumerable<CustomerUserIdDataSet.VerificationRequestRow> rows, FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicationRow applicationRow)
            {
                //_vrds.VerificationRequest.First().ReceivedStatus
                if (rows != null)
                {
                    _verificationRequests =
                        from x in rows
                        where x.CurrentStatus != VerificationStatus.Satisfied && x.VerificationType == VerificationTypeLookup.VerificationType.IdentityVerification
                        select new VerificationRequest
                        {
                            VerificationType = VerificationTypeLookup.VerificationType.IdentityVerification,
                            Definition = VerificationTypeLookup.GetDefinition(x.VerificationType),
                            Status = x.CurrentStatus,
                            ApplicantType = x.ApplicantType,
                            Caption = applicationRow.Applicants.First(ap => ap.ApplicantType == x.ApplicantType).Name,
                        };

                    Received = rows.Any(x => x.VerificationType == VerificationTypeLookup.VerificationType.IdentityVerification &&
                                                           x.CurrentStatus.IsReceivedOrWaitingToBeReceived());
                }
            }
        }

        public class SubjectPropertyAddressModel
        {
            private bool _isRequired;
            public bool Received { get; private set; }
            public bool SubjectPropertySameAsResidentAddress { get; set; }
            public HmdaCompliancePropertyPostData HmdaComplianceProperty { get; set; }
            public HmdaCompliancePropertyPostData Residence { get; set; }
            public int ApplicationId { get; private set; }
            public string AddressLine1 { get; private set; }
            public string AddressLine2 { get; private set; }

            public static string GetFullAddressLine1(AddressPostData address) => address.SecondaryUnit.Value.IsWhitespace()
                    ? $"{address.AddressLine}"
                    : $"{address.AddressLine} {address.SecondaryUnit.Type} {address.SecondaryUnit.Value}";

            public static string GetFullAddressLine2(AddressPostData address) => $"{address.City}, {StateLookup.GetCaption(address.State)} {address.ZipCode}";

            public SubjectPropertyAddressModel(IEnumerable<CustomerUserIdDataSet.VerificationRequestRow> rows,
                                                     CustomerUserIdDataSet.ApplicationRow applicationRow,
                                                     PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan,
                                                     LoanApplicationDataSet lads)
            {
                if (rows == null)
                    return;

                _isRequired = rows.Any(a => a.VerificationType == VerificationTypeLookup.VerificationType.SubjectPropertyAddress && a.CurrentStatus.IsOneOf(
                    VerificationStatus.Requested,
                    VerificationStatus.Resubmit));

                if (!_isRequired)
                    return;

                Received = rows.Any(a => a.VerificationType == VerificationTypeLookup.VerificationType.SubjectPropertyAddress
                    && a.CurrentStatus.IsNoneOf(VerificationStatus.Requested, VerificationStatus.Resubmit));

                HmdaComplianceProperty = new HmdaCompliancePropertyPostData();
                HmdaComplianceProperty.Address.SecondaryUnit.Type = PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected;

                ApplicationId = applicationRow.ApplicationId;

                Residence = new HmdaCompliancePropertyPostData();

                if (lads != null)
                {
                    var residentAddress = lads.ApplicantPostalAddress.SingleOrDefault(a => a.ApplicantRow.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary
                        && a.PostalAddressType == PostalAddressTypeLookup.PostalAddressType.PrimaryResidence);


                    if (residentAddress != null)
                    {
                        var occupancyType = lads.ApplicantHousingStatus.FirstOrDefault(a => a.ApplicantRow.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary)?.ApplicantHousingStatus;

                        Residence.Address.AddressLine = residentAddress.AddressLine1;
                        Residence.Address.City = residentAddress.City;
                        Residence.Address.State = residentAddress.State;
                        Residence.Address.ZipCode = residentAddress.ZipCode;
                        Residence.Address.SecondaryUnit.Type = residentAddress.SecondaryUnitType;
                        Residence.Address.SecondaryUnit.Value = residentAddress.IsSecondaryUnitValueNull() ? null : residentAddress.SecondaryUnitValue;
                        Residence.OccupancyType = (occupancyType ?? ApplicantHousingStatusLookup.ApplicantHousingStatus.NotSelected) ==
                            ApplicantHousingStatusLookup.ApplicantHousingStatus.Own ? OccupancyTypeLookup.OccupancyType.OwnerOccupied : OccupancyTypeLookup.OccupancyType.NotSelected;
                    }
                }
            }

            public bool IsRequired() => _isRequired;
            public string ToJSON()
            {
                return JsonConvert.SerializeObject(this, JsonNetResult.Settings).Replace("'", "\\'");
            }
        }

        public class CollateralInformationRequestModel
        {
            private bool _isRequired = false;

            public decimal RequestedLoanAmount { get; private set; }
            public string LoanNumber { get; private set; }
            public List<string> ApplicantNames { get; private set; }
            public string VIN { get; private set; }
            public decimal? Mileage { get; private set; }
            public decimal? TransactionAmount { get; private set; }
            public string MakeAndModel { get; private set; }
            public string Year { get; private set; }
            public bool Received { get; private set; }
            public string TransactionDescription { get; private set; }
            public int ApplicationId { get; private set; }
            public CollateralInformationRequestModel(IEnumerable<CustomerUserIdDataSet.VerificationRequestRow> rows,
                                                     FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicationRow applicationRow,
                                                     PurposeOfLoanLookup.PurposeOfLoan purposeOfLoan)
            {
                if (rows != null)
                {
                    _isRequired = rows.Any(x => x.VerificationType == VerificationTypeLookup.VerificationType.CollateralInfo &&
                                                x.CurrentStatus != VerificationStatus.Satisfied);
                    if (_isRequired)
                    {
                        Received = rows.Any(x => x.VerificationType == VerificationTypeLookup.VerificationType.CollateralInfo &&
                                                    x.CurrentStatus == VerificationStatus.Received);

                        RequestedLoanAmount = applicationRow.GetApplicationDetailRows()[0].AmountMinusFees;
                        LoanNumber = applicationRow.ApplicationId.ToString();
                        ApplicantNames = applicationRow.Applicants.Select(a => a.Name).ToList();
                        if (applicationRow.GetApplicationCollateralRows().Count() > 0)
                        {
                            var collateralRow = applicationRow.GetApplicationCollateralRows()[0];
                            if (collateralRow != null && !string.IsNullOrWhiteSpace(collateralRow.VIN))
                            {
                                VIN = collateralRow.VIN;
                                Mileage = (collateralRow.IsMileageNull()) ? (decimal?)null : collateralRow.Mileage;
                                TransactionAmount = (collateralRow.IsTransactionAmountNull()) ? (decimal?)null : collateralRow.TransactionAmount;
                                if (!collateralRow.IsMakeNull() && !collateralRow.IsModelNull())
                                {
                                    MakeAndModel = collateralRow.Make + " " + collateralRow.Model;
                                }
                                if (!collateralRow.IsYearNull())
                                {
                                    Year = collateralRow.Year.ToString();
                                }

                                // treat as if it was received
                                Received = true;
                            }
                        }
                        ApplicationId = applicationRow.ApplicationId;

                        switch (purposeOfLoan)
                        {
                            case PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchaseSecured:
                                TransactionDescription = "Total Amount of Loan Proceeds Paid To Dealer";
                                break;
                            case PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchaseSecured:
                                TransactionDescription = "Total Amount of Loan Proceeds Paid To Dealer";
                                break;
                            case PurposeOfLoanLookup.PurposeOfLoan.PrivatePartyPurchaseSecured:
                                TransactionDescription = "Selling Price of Vehicle";
                                break;
                            case PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured:
                                TransactionDescription = "Auto Loan Payoff Amount";
                                break;
                            case PurposeOfLoanLookup.PurposeOfLoan.LeaseBuyOutSecured:
                                TransactionDescription = "Selling Price of Vehicle";
                                break;
                        }

                    }
                }
            }

            public bool IsRequired()
            {
                return _isRequired;
            }
            public string ToJSON()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this).Replace("'", "\\'");
            }
        }

        public class DocumentRequestModel
        {
            private IEnumerable<VerificationRequest> _verificationRequests = null;

            public DocumentRequestModel(IEnumerable<CustomerUserIdDataSet.VerificationRequestRow> verificationRequestRows,
                                        IEnumerable<VerificationRequestDataSet.VerificationRequestRow> convertedRequestRows,
                                        ApplicationStatusTypeLookup.ApplicationStatusType currentStatus,
                                        IEnumerable<CustomerUserIdDataSet.QueueFileUploadRow> queueFileUploadRows,
                                        decimal loanApplicationVersion) // remove this parameter eventually and just use queueFileUploadRows
            {
                int _documentCount = 0;

                _verificationRequests = verificationRequestRows.Where(a => a.VerificationType != VerificationTypeLookup.VerificationType.CollateralInfo &&
                                              a.VerificationType != VerificationTypeLookup.VerificationType.IdentityVerification &&
                                              a.VerificationType != VerificationTypeLookup.VerificationType.SubjectPropertyAddress &&
                                              a.CurrentStatus != VerificationStatus.Satisfied).Select(r =>
                           new VerificationRequest
                           {
                               Caption = GetCaption(r),
                               Definition = GetDefinition(r),
                               Status = r.CurrentStatus,
                               VerificationType = r.VerificationType,
                               ApplicantType = r.ApplicantType,
                               VerificationRequestId = r.VerificationRequestId,
                               ID = "VI_" + r.VerificationType.ToString() + "_" + r.ApplicantType.ToString() + (_documentCount++).ToString(),
                               ReceivedStatus = convertedRequestRows.Single(cr => cr.VerificationRequestId == r.VerificationRequestId).ReceivedStatus,
                               SatisfiedStatus = convertedRequestRows.Single(cr => cr.VerificationRequestId == r.VerificationRequestId).SatisfiedStatus
                           }
                       ).ToList();

                queueFileUploadRows = queueFileUploadRows
                           .Where(a => a.QueueItemStatus.IsOneOf(
                                       QueueItemStatusTypeLookup.QueueItemStatusType.Ready,
                                       QueueItemStatusTypeLookup.QueueItemStatusType.InProcess,
                                       QueueItemStatusTypeLookup.QueueItemStatusType.Complete)).ToList();

                // Note: The QueueFileUpload table is a staging table subject to regular purging for rows > 30 days old (maybe not exactly 30 days but something like that).
                foreach (var request in _verificationRequests)
                {
                    request.FileNames = queueFileUploadRows
                            .Where(a => a.VerificationRequestId == request.VerificationRequestId)
                            .OrderBy(a => a.CreatedDate)
                            .Select(a => a.FileName).ToList();
                }

                // for post-funded verifications - they don't even want to see received or satisfied items
                // and remove anything that lingers from Pending or Counter V
                if (currentStatus.HasBeenFunded())
                {
                    _verificationRequests = _verificationRequests.Where(r => r.Status != VerificationStatus.Received && r.Status != VerificationStatus.Waived);
                    _verificationRequests = _verificationRequests.Where(r => r.VerificationType.IsPostFundingOnly());
                }

                // Remove any Received-Yes, Satisified-Waived items now that we know this info
                _verificationRequests = _verificationRequests.Where(vr => !(vr.ReceivedStatus == ReceivedStatus.Yes && vr.SatisfiedStatus == SatisfiedStatus.Waived));

                // mock items that are still "requested" to appear as "submitted" if documents have been uploaded
                foreach (var verificationRequest in
                                _verificationRequests.Where(r =>
                                    r.Status.IsOneOf(VerificationStatus.Requested,
                                                     VerificationStatus.NotSatisfied)
                                                     && r.NumberOfDocuments > 0))
                {
                    verificationRequest.Status = VerificationStatus.Submitted;
                }

                // mock items marked as "Resubmit" to "Submitted" if there were docs added after the status was set to resubmit.
                foreach (var verificationRequest in
                           _verificationRequests.Where(r =>
                               r.Status == VerificationStatus.Resubmit && r.NumberOfDocuments > 1))
                {
                    var request = convertedRequestRows.Single(a => a.VerificationRequestId == verificationRequest.VerificationRequestId);
                    var info = request.GetStatusInfo(VerificationStatus.Resubmit);

                    if (queueFileUploadRows.Any(a => a.VerificationRequestId == verificationRequest.VerificationRequestId
                                                && a.CreatedDate > info.CreatedDate))
                    {
                        verificationRequest.Status = VerificationStatus.Submitted;
                    }
                }

                foreach (var verificationRequest in _verificationRequests.Where(r => r.ReceivedStatus == ReceivedStatus.Yes && r.NumberOfDocuments > 1))
                {
                    verificationRequest.Status = VerificationStatus.Received;
                }
            }

            private string GetDefinition(CustomerUserIdDataSet.VerificationRequestRow r)
            {
                if (r.VerificationType == VerificationTypeLookup.VerificationType.Other || r.VerificationType == VerificationTypeLookup.VerificationType.OtherPostFunding)
                {
                    if (!r.IsCommentNull())
                        return r.Comment;

                    return string.Empty;
                }

                return VerificationTypeLookup.GetDefinition(r.VerificationType);
            }

            private string GetCaption(CustomerUserIdDataSet.VerificationRequestRow r)
            {
                if (r.VerificationType == VerificationTypeLookup.VerificationType.Other || r.VerificationType == VerificationTypeLookup.VerificationType.OtherPostFunding)
                {
                    return $"{r.RenderingCaption}{(r.IsSupplementalInformationNull() ? string.Empty : $": {r.SupplementalInformation}")}";
                }

                return r.RenderingCaption;
            }

            public bool IsRequired()
            {
                return _verificationRequests != null &&
                    _verificationRequests.Any(x => x.Status != VerificationStatus.Satisfied);
            }

            public string PluralOrSingluarString()
            {
                if (_verificationRequests == null || _verificationRequests.Count() == 0)
                {
                    return string.Empty;
                }

                return (_verificationRequests.Count() > 1) ? "documents" : "document";
            }

            public bool Any()
            {
                return Items().Any();
            }

            public IEnumerable<VerificationRequest> Items()
            {
                if (_verificationRequests == null)
                {
                    return new List<VerificationRequest>().AsEnumerable();
                }

                return _verificationRequests.OrderBy(x => x.Status != VerificationStatus.Received);
            }

            public int Count { get { return _verificationRequests == null ? 0 : _verificationRequests.Count(); } }
        }
    }
}