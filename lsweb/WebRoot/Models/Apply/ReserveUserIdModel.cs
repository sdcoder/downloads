using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Extensions;
using FirstAgain.Common.Logging;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;

namespace LightStreamWeb.Models.Apply
{
    public class ReserveUserIdModel
    {
        public bool Success { get; set; }
        public int? UserId { get; set; }
        public string TemporaryUserName { get; set; }
        public string ErrorMessage  { get; set; }

        public void ReserveUserId(CustomerUserCredentialsPostData userCredentials)
        {
            try
            {
                TemporaryUserName = null;
                if (userCredentials == null || userCredentials.UserName.IsNullOrEmpty() || userCredentials.SecurityQuestionAnswer.IsNullOrEmpty())
                {
                    Success = false;
                    return;
                }

                CreateUserIdResult result = DomainServiceCustomerOperations.CreateUserId(new CreateUserInfo()
                {
                    CustomerUserId = userCredentials.UserName.Trim(),
                    Password = userCredentials.Password,
                    SecurityAnswer = userCredentials.SecurityQuestionAnswer.Trim(),
                    SecurityQuestion = userCredentials.SecurityQuestionType
                });

                ErrorMessage = string.Empty;
                switch (result.Result)
                {
                    case CreateUserIdResultEnum.Success:
                        UserId = result.UserId;
                        Success = true;
                        break;
                    case CreateUserIdResultEnum.AlreadyExists:
                        ErrorMessage = Resources.LoanAppErrorMessages.ErrorCustomerUserIdExists;
                        Success = false;
                        break;
                    case CreateUserIdResultEnum.UseTemporary:
                        TemporaryUserName = result.TemporaryUserId;
                        Success = false;
                        break;
                    case CreateUserIdResultEnum.FailsRegEx:
                        Success = false;
                        break;
                    case CreateUserIdResultEnum.UsesMnemonic:
                        Success = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex, "Error reserving user id: len={0}, ua={1}", userCredentials.UserName.Trim().Length, HttpContext.Current.Request.UserAgent);
                Success = false;
                ErrorMessage = "We're sorry, but there was an error reserving your user id. Please try again.";
            }
        }
    }
}