using FirstAgain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common.Logging;
using FirstAgain.Web.Cookie;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace LightStreamWeb.Helpers
{
    // TODO e3p0: DI for unit testing
    internal class EventAuditLogHelper
    {
        private static HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        public static void Submit(CustomerUserIdDataSet customerUserIdDataSet, ICurrentUser webUser, EventTypeLookup.EventType eventType, string note, int? applicationId = null)
        {
            Submit(PopulateEventAuditLogDataSet(customerUserIdDataSet, webUser, eventType, note, applicationId));
        }

        private static void Submit(EventAuditLogDataSet ealds)
        {
            // Make this asynchronous so the customer doesn't have to wait for it to complete.
            ThreadPool.QueueUserWorkItem(a =>
            {
                try
                {
                    DomainServiceLoanApplicationOperations.SubmitEventAuditLog(ealds);
                }
                catch (Exception ex)
                {
                    LightStreamLogger.WriteError(ex);
                }
            });
        }

        internal static EventAuditLogDataSet PopulateEventAuditLogDataSet(CustomerUserIdDataSet customerUserIdDataSet, ICurrentUser webUser, EventTypeLookup.EventType eventType, string note, int? applicationId = null)
        {
            // application id can be supplied, or derived from the WebUser
            if (!applicationId.HasValue)
            {
                applicationId = webUser.ApplicationId;
            }
            
            EventAuditLogDataSet eads = new EventAuditLogDataSet();

            //create eventauditlog
            EventAuditLogDataSet.EventAuditLogRow eventRow = eads.EventAuditLog.NewEventAuditLogRow();
            eventRow.EventAuditLogId = -1;
            eventRow.EventInitiatorType = EventInitiatorTypeLookup.EventInitiatorType.Web;
            eventRow.EventType = eventType;
            eventRow.UserId = customerUserIdDataSet.UserId;

            //create webactivity datatable
            EventAuditLogDataSet.WebActivityRow webActivtyRow = eventRow.NewRelatedWebActivityRow();
            webActivtyRow.Cookie = webUser.UniqueCookie;
            webActivtyRow.IPAddress = webUser.IPAddress;
            webActivtyRow.UserAgent = SafeUserAgent(webUser.UserAgent);
            webActivtyRow.AcceptLanguage = webUser.AcceptLanguage;

            //set application specific data, find correct data if account passed in
            if (applicationId.HasValue)
            {
                var applicationRow = customerUserIdDataSet.Application.FindByApplicationId(applicationId.Value);
                if (applicationRow != null)
                {
                    //grab applicant rows that correspond to this applicationId
                    CustomerUserIdDataSet.ApplicantRow[] applicantRows = applicationRow.GetApplicantRows();

                    //doesn't matter what applicant to get the zip from, as primary zip is the same between both applicant and co applicant
                    eads.ZipCode =
                        customerUserIdDataSet.ApplicantPostalAddress.FindByApplicantIdPostalAddressTypeId(applicantRows[0].ApplicantId,
                                                                                            (short)
                                                                                            PostalAddressTypeLookup.
                                                                                                PostalAddressType.
                                                                                                PrimaryResidence).ZipCode;
                    eventRow.ApplicationId = applicationId.Value;
                    webActivtyRow.ApplicationStatusTypeId = applicationRow.ApplicationStatusTypeId;
                }
            }
            else if (customerUserIdDataSet.Application.Count == 1)
            //if account not passed in try to query for it if only 1 account exists under that customer id
            {
                //can grab first in array as there is only one, the same goes for the applicant as the zipcode will be the same for both borrower and co
                eads.ZipCode =
                    customerUserIdDataSet.ApplicantPostalAddress.FindByApplicantIdPostalAddressTypeId(
                        customerUserIdDataSet.Applicant[0].ApplicantId,
                        (short)PostalAddressTypeLookup.PostalAddressType.PrimaryResidence).ZipCode;
                eventRow.ApplicationId = customerUserIdDataSet.Application[0].ApplicationId;
                webActivtyRow.ApplicationStatusTypeId = customerUserIdDataSet.Application[0].ApplicationStatusTypeId;
            }
            else //otherwise allow nulls and set status to not selected.
            {
                webActivtyRow.ApplicationStatusTypeId =
                    (short)ApplicationStatusTypeLookup.ApplicationStatusType.NotSelected;
            }

            //add rows
            eads.EventAuditLog.AddEventAuditLogRow(eventRow);
            eads.WebActivity.AddWebActivityRow(webActivtyRow);

            //add note row if exist
            if (!string.IsNullOrEmpty(note))
            {
                EventAuditLogDataSet.NoteRow noteRow = eventRow.NewRelatedNoteRow();
                noteRow.NoteBody = note;
                noteRow.NoteType = NoteTypeLookup.NoteType.UserNotes;
                noteRow.IsLeadNote = false;
                noteRow.IsViewableByCustomer = false;
                eads.Note.AddNoteRow(noteRow);
            }

            return eads;
        }

        private static string SafeUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return string.Empty;
            }
            if (userAgent.Length <= 255)
            {
                return userAgent;
            }
            else
            {
                return userAgent.Substring(0, 254);
            }
        }
    }
}