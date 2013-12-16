using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using System.Xml.Linq;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.Files;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.editorControls.SettingControls.Pickers;
using umbraco.providers.members;
using DocumentType = umbraco.cms.businesslogic.web.DocumentType;
using File = System.IO.File;
using Log = umbraco.BusinessLogic.Log;
using Path = System.IO.Path;
using Property = umbraco.cms.businesslogic.property.Property;


public class MemberController : ApiController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.Http.ApiController"/> class.
    /// </summary>
    public MemberController()
    {
    }

    #region Account

    /// <summary>
    /// Login a client
    /// </summary>
    /// <param name="login">LoginName / Password / RememberMe </param>
    /// <returns></returns>
    /// <error name="200">Member login successfully</error>
    /// <error name="500">Exception</error>
    [HttpPost]
    public HttpResponseMessage Login(Login login)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        login.RememberMe = true;
        try
        {
            Member member = Member.GetMemberFromLoginAndEncodedPassword(login.LoginName, UmbracoCustom.EncodePassword(login.Password));
            Property isActive = member.getProperty("isActive");
            if (Membership.ValidateUser(login.LoginName, login.Password) && isActive.Value.ToString() == "1")
            {
                FormsAuthentication.SetAuthCookie(login.LoginName, login.RememberMe);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("Member login successfully");
            }
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(login),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "Login"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    /// <summary>
    /// LogOff a client
    /// </summary>
    /// <returns></returns>
    /// <error name="200">Member logout successfully</error>
    /// <error name="500">Exception</error>
    [HttpGet]
    public HttpResponseMessage LogOff()
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            if (HttpContext.Current.Request.Cookies["yourAuthCookie"] != null)
            {
                HttpCookie cookie = new HttpCookie("yourAuthCookie");
                cookie.Expires = DateTime.Now.AddYears(-20);
                HttpContext.Current.Response.Cookies.Add(cookie);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("Member logout successfully");
            }
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "LogOff"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    /// <summary>
    /// Check if Member is logged
    /// </summary>
    /// <returns>Account</returns>
    [HttpGet]
    public Account MemberIsLoggedOn()
    {
        Member member = Member.GetCurrentMember();
        Document document = new Document(Convert.ToInt32(member.getProperty("gymnast").Value));
        Account account = new Account
        {
            Id = member.Id,
            Name = member.Text,
            Email = member.Email,
            LoginName = member.LoginName,
            FirstName = member.getProperty("firstName").Value.ToString(),
            LastName = member.getProperty("lastName").Value.ToString(),
            Birthday = member.getProperty("birthday").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("birthday").Value) : (DateTime?)null,
            Gender = UmbracoCustom.PropertyValue(UmbracoType.Gender, member.getProperty("gender")),
            City = member.getProperty("city").Value.ToString(),
            State = member.getProperty("state").Value.ToString(),
            Country = member.getProperty("country").Value.ToString(),
            ZipCode = member.getProperty("zipCode").Value.ToString(),
            Address = member.getProperty("address").Value.ToString(),
            Phone = member.getProperty("phone").Value.ToString(),
            IsActive = member.getProperty("isActive").Value.ToString() == "1",
            Facebook = member.getProperty("facebook").Value.ToString(),
            Twitter = member.getProperty("twitter").Value.ToString(),
            Google = member.getProperty("google").Value.ToString(),
            Gymnast = Convert.ToInt32(member.getProperty("gymnast").Value),
            Height = member.getProperty("height").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("height").Value) : (decimal?)null,
            StartWeight = member.getProperty("startWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("startWeight").Value) : (decimal?)null,
            GoalWeight = member.getProperty("goalWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("goalWeight").Value) : (decimal?)null,
            //DOB = member.getProperty("dob").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("dob").Value) : (DateTime?)null,
            //UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
            TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : 0,
            EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
            EnablePrivateMessage = member.getProperty("enablePrivateMessage").Value.ToString() == "1",
            PurchaseId = member.getProperty("purchase").Value.ToString() != string.Empty ? Convert.ToInt32(member.getProperty("purchase").Value) : (int?)null,
            Purchase = member.getProperty("purchase").Value.ToString() != string.Empty ? UmbracoCustom.PropertyValue(UmbracoType.Purchase, member.getProperty("purchase")) : string.Empty,
        };
        return account;
    }

    /// <summary>
    /// Change Password of the client
    /// </summary>
    /// <param name="changePassword">LoginName / Password / NewPassword </param>
    /// <returns></returns>
    [HttpPost]
    public HttpResponseMessage ChangePassword(ChangePassword changePassword)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            if (Membership.ValidateUser(changePassword.LoginName, changePassword.Password))
            {
                MembershipUser user = Membership.GetUser(changePassword.LoginName);
                user.ChangePassword(changePassword.Password, changePassword.NewPassword);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("Password changed successfully");
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("LoginName or Password invalid");
            }
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(changePassword),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "ChangePassword"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    /// <summary>
    /// Reset Password of the client
    /// </summary>
    /// <param name="loginName">The LoginName of the client </param>
    /// <returns></returns>
    /// <exception cref="HttpResponseException"></exception>
    [HttpPost]
    public HttpResponseMessage ForgotPassword(string loginName)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            MembershipUser user = Membership.GetUser(loginName);
            string newPassword = user.ResetPassword();
            SendEmail(new EmailMessage
                {
                    Email = user.Email,
                    Subject = "MetaFitness Password Reset",
                    Message = string.Format("<h2>Password Reset</h2><div>Your password has been reset through the MetaFitness App. You new password is:</div><div></div><div style='background-color: grey; color: white; height: 40px; width: 150px; text-align: center; padding-top: 16px;; font-size: 14pt;'>{0}</div><div></div><div>Your login is your email address.</div><div></div><div>Thank you for using MetaFitness.</div>", newPassword)
                });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Reset Password successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = loginName,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "ForgotPassword"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    #endregion

    #region Client

    [HttpGet]
    public Account SelectMember(string loginname, string password)
    {
        Member member = Member.GetMemberFromLoginAndEncodedPassword(loginname, UmbracoCustom.EncodePassword(password));
        Document document = new Document(Convert.ToInt32(member.getProperty("gymnast").Value));
        Account account = new Account
        {
            Id = member.Id,
            Name = member.Text,
            Email = member.Email,
            LoginName = member.LoginName,
            FirstName = member.getProperty("firstName").Value.ToString(),
            LastName = member.getProperty("lastName").Value.ToString(),
            Birthday = member.getProperty("birthday").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("birthday").Value) : (DateTime?)null,
            Gender = UmbracoCustom.PropertyValue(UmbracoType.Gender, member.getProperty("gender")),
            City = member.getProperty("city").Value.ToString(),
            State = member.getProperty("state").Value.ToString(),
            Country = member.getProperty("country").Value.ToString(),
            ZipCode = member.getProperty("zipCode").Value.ToString(),
            Address = member.getProperty("address").Value.ToString(),
            Phone = member.getProperty("phone").Value.ToString(),
            IsActive = member.getProperty("isActive").Value.ToString() == "1",
            Facebook = member.getProperty("facebook").Value.ToString(),
            Twitter = member.getProperty("twitter").Value.ToString(),
            Google = member.getProperty("google").Value.ToString(),
            Gymnast = Convert.ToInt32(member.getProperty("gymnast").Value),
            Height = member.getProperty("height").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("height").Value) : (decimal?)null,
            StartWeight = member.getProperty("startWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("startWeight").Value) : (decimal?)null,
            GoalWeight = member.getProperty("goalWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("goalWeight").Value) : (decimal?)null,
            //DOB = member.getProperty("dob").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("dob").Value) : (DateTime?)null,
            //UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
            TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : 0,
            EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
            EnablePrivateMessage = member.getProperty("enablePrivateMessage").Value.ToString() == "1",
            PurchaseId = member.getProperty("purchase").Value.ToString() != string.Empty ? Convert.ToInt32(member.getProperty("purchase").Value) : (int?)null,
            Purchase = member.getProperty("purchase").Value.ToString() != string.Empty ? UmbracoCustom.PropertyValue(UmbracoType.Purchase, member.getProperty("purchase")) : string.Empty,
        };
        return account;
    }

    [HttpGet]
    public Account SelectMemberById(int id)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = new Member(id);
        Document document = new Document(Convert.ToInt32(member.getProperty("gymnast").Value));
        Account account = new Account
        {
            Id = member.Id,
            Name = member.Text,
            Email = member.Email,
            LoginName = member.LoginName,
            FirstName = member.getProperty("firstName").Value.ToString(),
            LastName = member.getProperty("lastName").Value.ToString(),
            Birthday = member.getProperty("birthday").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("birthday").Value) : (DateTime?)null,
            Gender = UmbracoCustom.PropertyValue(UmbracoType.Gender, member.getProperty("gender")),
            City = member.getProperty("city").Value.ToString(),
            State = member.getProperty("state").Value.ToString(),
            Country = member.getProperty("country").Value.ToString(),
            ZipCode = member.getProperty("zipCode").Value.ToString(),
            Address = member.getProperty("address").Value.ToString(),
            Phone = member.getProperty("phone").Value.ToString(),
            IsActive = member.getProperty("isActive").Value.ToString() == "1",
            Facebook = member.getProperty("facebook").Value.ToString(),
            Twitter = member.getProperty("twitter").Value.ToString(),
            Google = member.getProperty("google").Value.ToString(),
            Gymnast = Convert.ToInt32(member.getProperty("gymnast").Value),
            Height = member.getProperty("height").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("height").Value) : (decimal?)null,
            StartWeight = member.getProperty("startWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("startWeight").Value) : (decimal?)null,
            GoalWeight = member.getProperty("goalWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("goalWeight").Value) : (decimal?)null,
            //DOB = member.getProperty("dob").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("dob").Value) : (DateTime?)null,
            //UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
            TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : 0,
            EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
            EnablePrivateMessage = member.getProperty("enablePrivateMessage").Value.ToString() == "1",
            PurchaseId = member.getProperty("purchase").Value.ToString() != string.Empty ? Convert.ToInt32(member.getProperty("purchase").Value) : (int?)null,
            Purchase = member.getProperty("purchase").Value.ToString() != string.Empty ? UmbracoCustom.PropertyValue(UmbracoType.Purchase, member.getProperty("purchase")) : string.Empty,
        };
        return account;
    }

    [HttpGet]
    public Account SelectMemberByGymnast(int gymnastid)
    {
        Document gymnast = new Document(gymnastid);
        int memberId = Convert.ToInt32(gymnast.getProperty("member").Value);
        Member member = new Member(memberId);
        Account account = new Account();
        //Document document = new Document(Convert.ToInt32(member.getProperty("gymnast").Value));
        try
        {
            account = new Account
            {
                Id = member.Id,
                Name = member.Text,
                Email = member.Email,
                LoginName = member.LoginName,
                FirstName = member.getProperty("firstName").Value.ToString(),
                LastName = member.getProperty("lastName").Value.ToString(),
                Birthday = member.getProperty("birthday").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("birthday").Value) : (DateTime?)null,
                Gender = UmbracoCustom.PropertyValue(UmbracoType.Gender, member.getProperty("gender")),
                City = member.getProperty("city").Value.ToString(),
                State = member.getProperty("state").Value.ToString(),
                Country = member.getProperty("country").Value.ToString(),
                ZipCode = member.getProperty("zipCode").Value.ToString(),
                Address = member.getProperty("address").Value.ToString(),
                Phone = member.getProperty("phone").Value.ToString(),
                IsActive = member.getProperty("isActive").Value.ToString() == "1",
                Facebook = member.getProperty("facebook").Value.ToString(),
                Twitter = member.getProperty("twitter").Value.ToString(),
                Google = member.getProperty("google").Value.ToString(),
                Gymnast = Convert.ToInt32(member.getProperty("gymnast").Value),
                Height = member.getProperty("height").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("height").Value) : (decimal?)null,
                StartWeight = member.getProperty("startWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("startWeight").Value) : (decimal?)null,
                GoalWeight = member.getProperty("goalWeight").Value.ToString() != string.Empty ? Convert.ToDecimal(member.getProperty("goalWeight").Value) : (decimal?)null,
                //DOB = member.getProperty("dob").Value.ToString() != string.Empty ? Convert.ToDateTime(member.getProperty("dob").Value) : (DateTime?)null,
                //UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
                TrainerId = gymnast.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(gymnast.getProperty("trainer").Value) : 0,
                EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
                EnablePrivateMessage = member.getProperty("enablePrivateMessage").Value.ToString() == "1",
                PurchaseId = member.getProperty("purchase").Value.ToString() != string.Empty ? Convert.ToInt32(member.getProperty("purchase").Value) : (int?)null,
                Purchase = member.getProperty("purchase").Value.ToString() != string.Empty ? UmbracoCustom.PropertyValue(UmbracoType.Purchase, member.getProperty("purchase")) : string.Empty,
            };
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = "api/member/SelectMemberByGymnast",
                RequestObject = gymnastid.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "SelectMemberByGymnast"
            });
        }
        
        return account;
    }

    public IEnumerable<Member> SelectAllMember()
    {
        return Member.GetAllAsList().Where(m => Roles.GetRolesForUser(m.LoginName).Contains("Users"));
    }

    [HttpPost]
    public HttpResponseMessage InsertMember(Account account)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            MembershipUser user = Membership.CreateUser(account.LoginName, account.Password, account.Email);
            Roles.AddUserToRole(account.LoginName, "Users");
            Member member = Member.GetMemberFromLoginAndEncodedPassword(account.LoginName, UmbracoCustom.EncodePassword(account.Password));
            member.Text = account.Name ?? string.Empty;
            member.getProperty("isActive").Value = "1";
            member.getProperty("gender").Value = UmbracoCustom.PropertyId(UmbracoType.Gender, account.Gender);
            member.getProperty("firstName").Value = account.FirstName;
            member.getProperty("lastName").Value = account.LastName;
            member.getProperty("birthday").Value = account.Birthday.HasValue ? (object)account.Birthday.Value : null;
            member.getProperty("city").Value = account.City ?? string.Empty;
            member.getProperty("state").Value = account.State ?? string.Empty;
            member.getProperty("country").Value = account.Country ?? string.Empty;
            member.getProperty("zipCode").Value = account.ZipCode ?? string.Empty;
            member.getProperty("address").Value = account.Address ?? string.Empty;
            member.getProperty("phone").Value = account.Phone ?? string.Empty;
            member.getProperty("height").Value = account.Height.HasValue ? account.Height.ToString() : string.Empty;
            member.getProperty("startWeight").Value = account.StartWeight.HasValue ? account.StartWeight.ToString() : string.Empty;
            member.getProperty("goalWeight").Value = account.GoalWeight.HasValue ? account.GoalWeight.ToString() : string.Empty;
            //member.getProperty("dob").Value = account.Birthday.HasValue ? account.Birthday.Value.ToShortDateString() : string.Empty;
            //member.getProperty("useMetric").Value = account.UseMetric ? "1" : "0";
            member.getProperty("emailAlert").Value = account.EmailAlert ? "1" : "0";
            member.getProperty("purchase").Value = account.PurchaseId.HasValue ? account.PurchaseId.ToString() : string.Empty;
            member.getProperty("purchaseTemplate").Value = account.Gender.ToLower() == "male"
                ? new Document(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Male))).Children[0].Id
                : new Document(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Female))).Children[0].Id;  //new Document(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Template))).Children[0].Id;
            member.Save();
            FormsAuthentication.SetAuthCookie(account.LoginName, true);
            //CreateGymnast(new Gymnast { MemberId = member.Id, Name = member.Text });

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Member was registered successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(account),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertMember"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateMember(Account account)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            //member.Email = account.Email ?? member.Email;
            member.getProperty("gender").Value = !string.IsNullOrEmpty(account.Gender) ? UmbracoCustom.PropertyId(UmbracoType.Gender, account.Gender) : member.getProperty("gender").Value;
            member.getProperty("firstName").Value = !string.IsNullOrEmpty(account.FirstName) ? account.FirstName : member.getProperty("firstName").Value;
            member.getProperty("lastName").Value = !string.IsNullOrEmpty(account.LastName) ? account.LastName : member.getProperty("lastName").Value;
            member.getProperty("birthday").Value = account.Birthday.HasValue ? account.Birthday.Value : member.getProperty("birthday").Value;
            member.getProperty("phone").Value = !string.IsNullOrEmpty(account.Phone) ? account.Phone : member.getProperty("phone").Value;
            member.getProperty("height").Value = account.Height.HasValue ? account.Height.Value : member.getProperty("height").Value;
            member.getProperty("startWeight").Value = account.StartWeight.HasValue ? account.StartWeight.Value : member.getProperty("startWeight").Value;
            member.getProperty("goalWeight").Value = account.GoalWeight.HasValue ? account.GoalWeight.Value : member.getProperty("goalWeight").Value;
            member.getProperty("purchase").Value = account.PurchaseId.HasValue ? account.PurchaseId.Value : member.getProperty("purchase").Value;
            member.Save();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Member was updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId =  Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(account),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertMember"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateEmailAlert(bool emailAlert)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            member.getProperty("emailAlert").Value = emailAlert ? "1" : "0";
            member.Save();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Email Alert was updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(emailAlert),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateEmailAlert"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    [HttpPost]
    public Task<HttpResponseMessage> InsertAvatar()
    {
        Member member = Member.GetCurrentMember();
        if (!Request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        //string root = HttpContext.Current.Server.MapPath("~/App_Data");
        string root = UmbracoCustom.GetParameterValue(UmbracoType.Temp);
        CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(root);

        var task = Request.Content.ReadAsMultipartAsync(provider).
            ContinueWith<HttpResponseMessage>(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                foreach (MultipartFileData file in provider.FileData)
                {
                    try
                    {
                        FileInfo info = new FileInfo(file.LocalFileName);
                        Property image = member.getProperty("photo");
                        Stream stream = new FileStream(file.LocalFileName, FileMode.Open, FileAccess.Read);
                        stream.Position = 0;
                        umbraco.BusinessLogic.Log.Add(LogTypes.New, member.Id, file.LocalFileName);
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(UmbracoCustom.GetParameterValue(UmbracoType.Media) + image.Id);
                        umbraco.BusinessLogic.Log.Add(LogTypes.New, image.Id, directoryInfo.FullName);
                        UmbracoFile imageUpload = UmbracoFile.Save(stream, string.Format(@"{0}\{1}", directoryInfo.FullName, info.Name));
                        imageUpload.Resize(100, "thumb");
                        image.Value = string.Format("/media/{0}/{1}", image.Id, info.Name);
                        member.Save();
                    }
                    catch (Exception ex)
                    {
                        int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
                        InsertLog(new global::Log
                        {
                            UserId = Member.GetCurrentMember().Id,
                            UserType = clientId,
                            Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                            RequestObject = JsonConvert.SerializeObject(file),
                            ExceptionMessage = ex.Message,
                            ExceptionType = ex.GetType().Name,
                            StackTrace = ex.StackTrace,
                            Source = ex.Source,
                            Operation = "InsertAvatar"
                        });
                        umbraco.BusinessLogic.Log.Add(LogTypes.New, -1, ex.Message);
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Image was updated successfully");
            });

        return task;
    }

    public HttpResponseMessage GetAvatar()
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            string umbracoPath = member.getProperty("photo").Value.ToString();
            string mediaPath = UmbracoCustom.GetParameterValue(UmbracoType.Media);
            Image image = Image.FromFile(mediaPath.Replace(@"media\", umbracoPath));
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    public string GetMemberAvatar(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        string result = string.Empty;
        try
        {
            User u = umbraco.BusinessLogic.User.GetCurrent();
            Member member = new Member(id);
            string umbracoPath = member.getProperty("photo").Value.ToString();
            if (!string.IsNullOrEmpty(umbracoPath))
            {
                string mediaPath = UmbracoCustom.GetParameterValue(UmbracoType.Media);
                Image image = Image.FromFile(mediaPath.Replace(@"media\", umbracoPath));
                MemoryStream memoryStream = new MemoryStream();
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                response.StatusCode = HttpStatusCode.OK;
                result = string.Format("data:image/jpeg;base64,{0}", UmbracoCustom.ImageToBase64(image, ImageFormat.Png));
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return result;
    }

    [HttpPost]
    public JObject VerifyAppleReceipt(string receipt)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        Member member = Member.GetCurrentMember();
        JObject result = new JObject();
        try
        {
            Document document = new Document(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Site)));
            var postData = new Dictionary<string, string>()
                {
                    {"receipt-data", receipt},
                    {"password", document.getProperty("sharedSecretKey").Value.ToString()}
                };

            byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData));
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(document.getProperty("url").Value.ToString()));
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = byteArray.Length;

            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader stream = new StreamReader(webResponse.GetResponseStream());
                    result = JObject.Parse(stream.ReadToEnd());
                }
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Verify Receipt successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = receipt,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "VerifyAppleReceipt"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return result;
    }

    [HttpPost]
    public HttpResponseMessage VerifyReceipt(SubscriptionReceipt receipt)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        Member member = receipt.MemberId != 0 ? new Member(receipt.MemberId) : Member.GetCurrentMember();
        JObject result = VerifyAppleReceipt(receipt.Receipt);
        try
        {
            switch (Convert.ToInt32(result["status"]))
            {
                case 0:
                    PurchaseWorkout();
                    SendTemplateMessage(new TemplateMessage
                    {
                        Id = 1315, //1174,
                        Email = member.Email,
                        MemberId = member.Id,
                        UserId = 7,
                        ObjectType = 151 //Purchase
                    });
                    InsertReceipt(new SubscriptionReceipt
                    {
                        MemberId = member.Id,
                        Status = result["status"].Value<int>(),
                        Receipt = result.ToString()
                        //LatestReceipt = result["latest_receipt"].ToString(),
                        //LatestReceiptInfo = result["latest_receipt_info"].ToString(),
                    });
                    member.getProperty("receipt").Value = receipt.Receipt;
                    member.getProperty("expiresDate").Value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long.Parse(result["receipt"]["expires_date"].ToString())));
                    member.Save();
                    break;
                case 21006:
                    InsertReceipt(new SubscriptionReceipt
                    {
                        MemberId = member.Id,
                        Status = result["status"].Value<int>(),
                        Receipt = result.ToString()
                        //LatestReceipt = result["latest_receipt"].ToString(),
                        //LatestReceiptInfo = result["latest_receipt_info"].ToString(),
                    });
                    member.getProperty("receipt").Value = receipt.Receipt;
                    member.getProperty("expiresDate").Value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long.Parse(result["receipt"]["expires_date"].ToString())));
                    member.getProperty("purchase").Value = 131;
                    member.Save();
                    break;
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Verify Receipt successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(receipt),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "VerifyReceipt"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public void PurchaseAll(int templateid, int objectId)
    {
        foreach (Member member in Member.GetAllAsList().Where(m => m.getProperty("isActive").Value.ToString() == "1"))
        {
            DateTime current = DateTime.Now;
            DateTime expiresDate = Convert.ToDateTime(member.getProperty("expiresDate").Value);
            if (expiresDate.ToShortDateString() == current.ToShortDateString())
            {
                string receipt = member.getProperty("receipt").Value.ToString();
                VerifyReceipt(new SubscriptionReceipt
                {
                    MemberId = member.Id,
                    Receipt = receipt
                });
            }
        }
    }

    [HttpPost]
    public HttpResponseMessage InsertReceipt(SubscriptionReceipt receipt)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        Member member = Member.GetCurrentMember();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = receipt.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertReceipt",
            parameter,
            new SqlParameter { ParameterName = "@MemberId", Value = receipt.MemberId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Status", Value = receipt.Status, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Receipt", Value = receipt.Status, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
                //new SqlParameter { ParameterName = "@LatestReceipt", Value = receipt.LatestReceipt, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 },
                //new SqlParameter { ParameterName = "@LatestReceiptInfo", Value = receipt.LatestReceiptInfo, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Receipt successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(receipt),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertReceipt"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateReceipt(SubscriptionReceipt receipt)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        Member member = Member.GetCurrentMember();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            //SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = receipt.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateReceipt",
            new SqlParameter { ParameterName = "@MemberId", Value = receipt.MemberId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Status", Value = receipt.Status, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Receipt", Value = receipt.Status, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
                //new SqlParameter { ParameterName = "@LatestExpiredReceiptInfo", Value = receipt.LatestExpiredReceiptInfo, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Receipt successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(receipt),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateReceipt"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public SubscriptionReceipt SelectReceipt(int id)
    {
        SubscriptionReceipt receipt = new SubscriptionReceipt();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectReceipt", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                receipt = new SubscriptionReceipt
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Status = Convert.ToInt32(reader.GetValue(1)),
                    Receipt = reader.GetValue(2).ToString(),
                    LatestReceipt = reader.GetValue(3).ToString(),
                    LatestReceiptInfo = reader.GetValue(4).ToString(),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5))
                };
            }

        }
        return receipt;
    }

    #endregion

    #region Workout

    [HttpPost]
    public HttpResponseMessage PurchaseWorkout()
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            //Document gymnast = new Document(Convert.ToInt32(member.getProperty("gymnast").Value));
            int purchaseTemplate = Convert.ToInt32(member.getProperty("purchaseTemplate").Value);
             Document[] documents = new Document[10000];
            string gender = UmbracoCustom.PropertyValue(UmbracoType.Gender, member.getProperty("gender")).ToLower();
            switch (gender)
            {
                case "male":
                    documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Male))).Where(d => d.Id >= purchaseTemplate).Take(30).ToArray();
                    break;
                case "female":
                    documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Female))).Where(d => d.Id >= purchaseTemplate).Take(30).ToArray();
                    break;
            }
            foreach (Document document in documents)
            {
                Workout workout = InsertWorkout(new Workout
                    {
                        Name = document.Text,
                        //ParentId = gymnast.Id,
                        Description = document.getProperty("description").Value.ToString(),
                        StateId = UmbracoCustom.PropertyValueId(UmbracoType.WorkoutState, "Not Viewed")
                    });
                IEnumerable<RoutineViewModel> routines = GetRoutineByWorkout(document.Id).Select((r) =>
                    {
                        r.ObjectId = workout.Id;
                        return new RoutineViewModel
                            {
                                Routine = r,
                                Stories = new List<Story>()
                            };
                    });
                InsertRoutines(routines);
            }
            member.getProperty("purchaseTemplate").Value = documents.LastOrDefault().Id;
            member.Save();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Purchase Workout successfully added");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "PurchaseWorkout"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public Workout InsertWorkout(Workout workout)
    {
        HttpResponseMessage response = new HttpResponseMessage();

        try
        {
            User user = umbraco.BusinessLogic.User.GetCurrent();
            Member member = user != null ? new Member(workout.MemberId) : Member.GetCurrentMember();
            //int trainerId = Convert.ToInt32(member.getProperty("trainer").Value);
            //Document document = new Document(trainerId);
            Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
            Document document = documents.Single(d => d.Text == member.Text);
            //Document document = documents.SingleOrDefault(d => d.Text == member.Text) ?? CreateGymnast(new Gymnast { MemberId = member.Id, Name = member.Text });
            DocumentType documentType = DocumentType.GetByAlias("Workout");
            document = Document.MakeNew(workout.Name, documentType, new User("admin"), document.Id);
            document.getProperty("dateScheduled").Value = workout.DateScheduled;
            document.getProperty("dateCompleted").Value = workout.DateCompleted;
            document.getProperty("description").Value = workout.Description;
            document.getProperty("state").Value = workout.StateId;
            document.getProperty("note").Value = workout.Note;
            document.Save();
            workout = SelectWorkoutById(document.Id, member.Id);

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(workout),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertWorkout"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return workout;
    }

    [HttpGet]
    public Workout SelectWorkoutById(int id, int memberId = 0)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = user != null ? new Member(memberId) : Member.GetCurrentMember();

        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Document document = documents.Single(d => d.Text == member.Text);
        Document workout = document.Children.SingleOrDefault(w => w.Id == id);

        return new Workout
        {
            Id = workout.Id,
            ParentId = workout.ParentId,
            Name = workout.Text,
            DateScheduled = (workout.getProperty("dateScheduled").Value.ToString() != "" ? Convert.ToDateTime(workout.getProperty("dateScheduled").Value) : (DateTime?)null),
            DateCompleted = (workout.getProperty("dateCompleted").Value.ToString() != "" ? Convert.ToDateTime(workout.getProperty("dateCompleted").Value) : (DateTime?)null),
            Description = workout.getProperty("description").Value.ToString(),
            StateId = (!string.IsNullOrEmpty(workout.getProperty("state").Value.ToString()) ? Convert.ToInt32(workout.getProperty("state").Value) : (int?)null),
            State = (!string.IsNullOrEmpty(workout.getProperty("state").Value.ToString()) ? UmbracoCustom.PropertyValue(UmbracoType.WorkoutState, Convert.ToInt32(workout.getProperty("state").Value)) : string.Empty),
            //RateId = (child.getProperty("rate").Value.ToString() != "" ? Convert.ToInt32(child.getProperty("rate").Value) : (int?)null),
            //Rate = UmbracoCustom.PropertyValue(UmbracoType.Rate, child.getProperty("rate")),
            TimeSpent = (!string.IsNullOrEmpty(workout.getProperty("timeSpent").Value.ToString()) ? Convert.ToInt32(workout.getProperty("timeSpent").Value) : (int?)null),
            Note = workout.getProperty("note").Value.ToString(),
            CreatedDate = workout.CreateDateTime,
            UpdatedDate = workout.UpdateDate,
            SortOrder = workout.sortOrder
        };
    }

    [HttpGet]
    public IEnumerable<Workout> SelectWorkoutByMember(int id = 0, DateTime? fromDate = null, DateTime? toDate = null)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = user != null ? new Member(id) : Member.GetCurrentMember();

        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Document document = documents.Single(d => d.Text == member.Text);
        List<Workout> workouts = document.Children.Select(child => new Workout
        {
            Id = child.Id,
            ParentId = child.ParentId,
            Name = child.Text,
            DateScheduled = (child.getProperty("dateScheduled").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateScheduled").Value) : (DateTime?)null),
            DateCompleted = (child.getProperty("dateCompleted").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateCompleted").Value) : (DateTime?)null),
            Description = child.getProperty("description").Value.ToString(),
            StateId = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? Convert.ToInt32(child.getProperty("state").Value) : (int?)null),
            State = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? UmbracoCustom.PropertyValue(UmbracoType.WorkoutState, Convert.ToInt32(child.getProperty("state").Value)) : string.Empty),
            //RateId = (child.getProperty("rate").Value.ToString() != "" ? Convert.ToInt32(child.getProperty("rate").Value) : (int?)null),
            //Rate = UmbracoCustom.PropertyValue(UmbracoType.Rate, child.getProperty("rate")),
            Note = child.getProperty("note").Value.ToString(),
            TimeSpent = (!string.IsNullOrEmpty(child.getProperty("timeSpent").Value.ToString()) ? Convert.ToInt32(child.getProperty("timeSpent").Value) : (int?)null),
            CreatedDate = child.CreateDateTime,
            UpdatedDate = child.UpdateDate,
            SortOrder = child.sortOrder
        }).ToList();

        if (fromDate != null && toDate != null)
        {
            workouts = workouts.Where(w => w.UpdatedDate >= fromDate && w.UpdatedDate <= toDate).ToList();
        }

        return workouts;
    }

    [HttpPost]
    public HttpResponseMessage UpdateWorkoutOrder(Dictionary<int, int> workouts)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
            Document document = documents.Single(d => d.Text == member.Text);
            document.Children.ToList().ForEach(d =>
                {
                    var key = workouts.Keys.SingleOrDefault(w => w == d.Id);
                    if (key != null)
                    {
                        d.sortOrder = workouts[key];
                        d.Save();
                    }
                });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully edited");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateWorkoutOrder"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateWorkout(Workout workout)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {   //2013-06-24T20:32:56.681Z
            User user = umbraco.BusinessLogic.User.GetCurrent();
            Member member = Member.GetCurrentMember();

            Document document = new Document(workout.Id);
            document.getProperty("dateScheduled").Value = workout.DateScheduled.HasValue ? workout.DateScheduled.Value : document.getProperty("dateScheduled").Value;
            document.getProperty("dateCompleted").Value = workout.DateCompleted.HasValue ? workout.DateCompleted.Value : document.getProperty("dateCompleted").Value;
            document.getProperty("description").Value = !string.IsNullOrEmpty(workout.Description) ? workout.Description : document.getProperty("description").Value;
            document.getProperty("state").Value = workout.StateId.HasValue ? workout.StateId : document.getProperty("state").Value;
            //document.getProperty("rate").Value = workout.RateId;
            document.getProperty("note").Value = !string.IsNullOrEmpty(workout.Note) ? workout.Note : document.getProperty("note").Value;
            document.getProperty("timeSpent").Value = workout.TimeSpent.HasValue ? workout.TimeSpent.Value.ToString() : document.getProperty("timeSpent").Value;
            document.Save();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully edited");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateWorkout"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeleteWorkout(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            User user = umbraco.BusinessLogic.User.GetCurrent();
            Member member = Member.GetCurrentMember();

            Document document = new Document(id);
            document.delete();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteWorkout"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public WorkoutViewModel GetWorkoutDetails(int workoutid)
    {
        WorkoutViewModel workoutViewModel = new WorkoutViewModel
        {
            SuperSets = new List<SuperSetViewModel>(),
            Routines = new List<RoutineViewModel>()
        };
        foreach (SuperSet superSet in GetSuperSet(workoutid))
        {
            workoutViewModel.SuperSets.Add(new SuperSetViewModel
            {
                SuperSet = superSet,
                Routines = GetRoutineStories(superSet.Id)
            });
        }
        workoutViewModel.Routines.AddRange(GetRoutineStories(workoutid));
        return workoutViewModel;
    }

    [HttpGet]
    public int SelectNumberViewedWorkout(int id = 0)
    {
        int stateId = UmbracoCustom.PropertyId(UmbracoType.WorkoutState, "Viewed");
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return user != null ? SelectWorkoutByMember(id).Count(x => x.StateId == stateId) : SelectWorkoutByMember().Count(x => x.StateId == stateId);
    }

    [HttpGet]
    public int SelectNumberNotViewedWorkout(int id = 0)
    {
        int stateId = UmbracoCustom.PropertyId(UmbracoType.WorkoutState, "Not Viewed");
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return user != null ? SelectWorkoutByMember(id).Count(x => x.StateId == stateId) : SelectWorkoutByMember().Count(x => x.StateId == stateId);
    }

    [HttpGet]
    public int SelectNumberCompleteWorkout(int id = 0)
    {
        int stateId = UmbracoCustom.PropertyId(UmbracoType.WorkoutState, "Complete");
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return user != null ? SelectWorkoutByMember(id).Count(x => x.StateId == stateId) : SelectWorkoutByMember().Count(x => x.StateId == stateId);
    }

    [HttpGet]
    public int SelectNumberNotCompleteWorkout(int id = 0)
    {
        int stateId = UmbracoCustom.PropertyId(UmbracoType.WorkoutState, "Not Complete");
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return user != null ? SelectWorkoutByMember(id).Count(x => x.StateId == stateId) : SelectWorkoutByMember().Count(x => x.StateId == stateId);
    }

    [HttpGet]
    public int SelectNumberWorkout(int id = 0)
    {
        int stateId = UmbracoCustom.PropertyId(UmbracoType.WorkoutState, "Not Complete");
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return user != null ? SelectWorkoutByMember(id).Count() : SelectWorkoutByMember().Count();
    }

    #endregion

    #region Gymnast

    [HttpGet]
    public Gymnast SelectGymnast()
    {
        Member member = Member.GetCurrentMember();
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Document document = documents.Single(d => d.Text == member.Text);
        return new Gymnast
            {
                Id = document.Id,
                ParentId = document.ParentId,
                Name = document.Text,
                MemberId = Convert.ToInt32(document.getProperty("member").Value),
                TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : (int?)null,
                Trainer = document.getProperty("trainer").Value.ToString() != string.Empty ? SelectAllTrainer().Single(t => t.Id == Convert.ToInt32(document.getProperty("trainer").Value)).Name : string.Empty
            };
    }

    [HttpPost]
    public HttpResponseMessage UpdateGymnast(Gymnast gymnast)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            Document document = new Document(gymnast.Id);
            document.getProperty("trainer").Value = gymnast.TrainerId;
            document.Save();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Gymnast successfully updated");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(gymnast),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateGymnast"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertGymnast(Gymnast gymnast)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            DocumentType documentType = DocumentType.GetByAlias("Gymnast");
            Document document = Document.MakeNew(gymnast.Name, documentType, new User("admin"), int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
            document.getProperty("member").Value = gymnast.MemberId;
            document.getProperty("trainer").Value = gymnast.TrainerId != null ? gymnast.TrainerId.ToString() : string.Empty;
            document.Save();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Gymnast successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(gymnast),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertGymnast"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    #endregion

    #region Trainer

    [HttpGet]
    public IEnumerable<UmbracoUser> SelectAllTrainer()
    {
        IEnumerable<User> trainers = umbraco.BusinessLogic.User.getAll().Where(u => u.UserType.Name == "trainer" && u.Disabled == false);
        return trainers.Select(
                u => new UmbracoUser
                    {
                        //DefaultToLiveEditing = u.DefaultToLiveEditing,
                        //Disabled = u.Disabled,
                        //Email = u.Email,
                        Id = u.Id,
                        //Language = u.Language,
                        LoginName = u.LoginName,
                        Name = u.Name,
                        //NoConsole = u.NoConsole,
                        //Password = u.Password,
                        //StartMediaId = u.StartMediaId,
                        //StartNodeId = u.StartNodeId,
                        //UserType = new UmbracoUserType
                        //    {
                        //        Alias = u.UserType.Alias,
                        //        DefaultPermissions = u.UserType.DefaultPermissions,
                        //        Id = u.UserType.Id,
                        //        Name = u.UserType.Name
                        //    },
                        //Applications = u.Applications.Select(a => new UmbracoUserApplication
                        //    {
                        //        Alias = a.alias,
                        //        Icon = a.icon,
                        //        Name = a.name,
                        //        SortOrder = a.sortOrder
                        //    })
                    });
    }

    [HttpPost]
    public HttpResponseMessage LoginTrainer(Login login)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            if (umbraco.BusinessLogic.User.validateCredentials(login.LoginName, UmbracoCustom.EncodePassword(login.Password)))
            {
                User[] users = umbraco.BusinessLogic.User.getAllByLoginName(login.LoginName);
                Guid guid = Guid.NewGuid();
                string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertUserLogin",
                          new SqlParameter { ParameterName = "@ContextID", Value = guid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.UniqueIdentifier },
                          new SqlParameter { ParameterName = "@UserID", Value = users[0].Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                          new SqlParameter { ParameterName = "@Timeout", Value = DateTime.Now.Ticks + 600000000L * long.Parse(UmbracoCustom.GetParameterValue(UmbracoType.TimeOut)), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.BigInt }
                          );
                FormsAuthentication.Encrypt(new FormsAuthenticationTicket(1, guid.ToString(), DateTime.Now, DateTime.Now.AddDays(1.0), false, guid.ToString(), FormsAuthentication.FormsCookiePath));
                StateHelper.Cookies.UserContext.SetValue(guid.ToString(), 1.0);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("User valid");
            }
            else
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent("User is not registered");
            }
        }
        catch (Exception ex)
        {
            int trainerId = UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer");
            InsertLog(new global::Log
            {
                UserType = trainerId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(login),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "LoginTrainer"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        
        return response;
    }

    [HttpGet]
    public HttpResponseMessage LogOffTrainer()
    {
        User u = umbraco.BusinessLogic.User.GetCurrent();
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            if (HttpContext.Current.Request.Cookies["UMB_UCONTEXT"] != null)
            {
                string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteUserLogin",
                          new SqlParameter { ParameterName = "@ContextID", Value = new Guid(HttpContext.Current.Request.Cookies["UMB_UCONTEXT"].Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.UniqueIdentifier }
                          );

                HttpCookie cookie = new HttpCookie("UMB_UCONTEXT");
                cookie.Expires = DateTime.Now.AddYears(-20);
                HttpContext.Current.Response.Cookies.Add(cookie);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent("Member logout successfully");
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }

        return response;
    }

    [HttpGet]
    public IEnumerable<Gymnast> SelectGymnastByTrainer()
    {
        User u = umbraco.BusinessLogic.User.GetCurrent();
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));

        List<Gymnast> gymnasts = documents.Where(g => g.getProperty("trainer").Value.ToString() == u.Id.ToString()).Select(g => new Gymnast
        {
            Id = g.Id,
            ParentId = g.ParentId,
            Name = g.Text,
            MemberId = Convert.ToInt32(g.getProperty("member").Value),
            TrainerId = Convert.ToInt32(g.getProperty("trainer").Value),
            Trainer = u.Name
        }).ToList();
        return gymnasts;
    }

    [HttpGet]
    public IEnumerable<GymnastViewModel> SelectGymnastInfoByTrainer()
    {
        User u = umbraco.BusinessLogic.User.GetCurrent();
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));

        List<GymnastViewModel> gymnasts = documents.Where(g => g.getProperty("trainer").Value.ToString() == u.Id.ToString()).Select(g => new GymnastViewModel
        {
            Gymnast = new Gymnast
            {
                Id = g.Id,
                ParentId = g.ParentId,
                Name = g.Text,
                MemberId = Convert.ToInt32(g.getProperty("member").Value),
                TrainerId = Convert.ToInt32(g.getProperty("trainer").Value),
                Trainer = u.Name
            },
            Workouts = g.Children.Select(child => new Workout
            {
                Id = child.Id,
                ParentId = child.ParentId,
                Name = child.Text,
                DateScheduled = (child.getProperty("dateScheduled").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateScheduled").Value) : (DateTime?)null),
                DateCompleted = (child.getProperty("dateCompleted").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateCompleted").Value) : (DateTime?)null),
                Description = child.getProperty("description").Value.ToString(),
                StateId = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? Convert.ToInt32(child.getProperty("state").Value) : (int?)null),
                State = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? UmbracoCustom.PropertyValue(UmbracoType.WorkoutState, Convert.ToInt32(child.getProperty("state").Value)) : string.Empty),
                //RateId = (child.getProperty("rate").Value.ToString() != "" ? Convert.ToInt32(child.getProperty("rate").Value) : (int?)null),
                //Rate = UmbracoCustom.PropertyValue(UmbracoType.Rate, child.getProperty("rate")),
                Note = child.getProperty("note").Value.ToString(),
                CreatedDate = child.CreateDateTime,
                UpdatedDate = child.UpdateDate,
                SortOrder = child.sortOrder
            }).ToList()
        }).ToList();
        return gymnasts;
    }

    #endregion

    #region Routine

    [HttpPost]
    public HttpResponseMessage UpdateRoutine(Routine routine)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateRoutine",
              new SqlParameter { ParameterName = "@Id", Value = routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@ExerciseId", Value = routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@Reps", Value = (object)routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@Sets", Value = (object)routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@Resistance", Value = (object)routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@UnitId", Value = (object)routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@Note", Value = routine.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
              );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Exercise updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routine),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateRoutine"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateRoutineExercise(Routine routine)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateRoutineExercise",
              new SqlParameter { ParameterName = "@Id", Value = routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@StateId", Value = routine.StateId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@StartedDate", Value = routine.StartedDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
              new SqlParameter { ParameterName = "@CompletedDate", Value = routine.CompletedDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
              new SqlParameter { ParameterName = "@CanceledDate", Value = routine.CanceledDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime }
              );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Exercise updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routine),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateRoutineExercise"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public IEnumerable<UmbracoPreValue> GetCategory()
    {
        int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Category));
        return UmbracoCustom.DataTypeValue(id).Select(u => new UmbracoPreValue { Id = u.Id, Value = u.Value });
    }

    [HttpGet]
    public IEnumerable<CategoryViewModel> GetCategories()
    {
        int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Category));
        return UmbracoCustom.DataTypeValue(id).Select(u => new UmbracoPreValue { Id = u.Id, Value = u.Value }).Select(category => new CategoryViewModel
            {
                Id = category.Id,
                Value = category.Value,
                Exercises = GetExerciseByCategory(category.Id)
            }).ToList();
    }

    [HttpGet]
    public List<Exercise> GetExerciseByCategory(int categoryid, int trainerid = 0)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        if (user != null)
        {
            trainerid = user.Id;
        }
        Member member = Member.GetCurrentMember();

        List<Exercise> exercises = new List<Exercise>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectExerciseByCategory",
            new SqlParameter { ParameterName = "@CategoryId", Value = categoryid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@TrainerId", Value = trainerid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            ))
        {
            while (reader.Read())
            {
                exercises.Add(new Exercise
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ExerciseName = reader.GetValue(1).ToString(),
                    TrainerId = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(3)),
                    CategoryId = Convert.ToInt32(reader.GetValue(3)),
                    Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(3)),
                    IsActive = Convert.ToBoolean(reader.GetValue(4))
                });
            }
        }
        return exercises;
    }

    //[HttpGet]
    //public dynamic InsertExercise()
    //{
    //    int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode));
    //    Document document = new Document(id);
    //    Property property = document.getProperty("exercise");

    //    string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
    //    var exercises = UmbracoCustom.GetDataTypeGrid(property);
    //    //foreach (dynamic exercise in exercises)
    //    //{
    //    //    SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertExercise",
    //    //          new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
    //    //          new SqlParameter { ParameterName = "@Name", Value = exercise.exercise, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
    //    //        //new SqlParameter { ParameterName = "@Description", Value = exercise.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
    //    //          new SqlParameter { ParameterName = "@TrainerId", Value = DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //    //        //new SqlParameter { ParameterName = "@TypeId", Value = int.Parse(exercise.Type), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //    //        //new SqlParameter { ParameterName = "@UnitId", Value = int.Parse(exercise.Unit), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //    //          new SqlParameter { ParameterName = "@CategoryId", Value = exercise.category, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
    //    //          );
    //    //}
    //    List<dynamic> result = new List<dynamic>();

    //        foreach (XElement element in XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data") + "/exercises.xml").Descendants("item"))
    //        {
    //            dynamic data = new ExpandoObject();
    //            ((IDictionary<string, object>)data).Add("id", element.Attribute("id").Value);
    //            ((IDictionary<string, object>)data).Add("sortorder", element.Attribute("sortOrder").Value);
    //            foreach (XElement item in element.Descendants())
    //            {
    //                ((IDictionary<string, object>)data).Add(item.Name.LocalName, item.Value);
    //            }
    //            result.Add(data);
    //        }
    //    return result;
    //}

    //[HttpPost]
    //public HttpResponseMessage InsertRoutine(Routine routine)
    //{
    //    HttpResponseMessage response = new HttpResponseMessage();
    //    try
    //    {
    //        Member member = Member.GetCurrentMember();
    //        SetRoutineUser(routine);

    //        //Document WorkoutId = new Document(routine.ObjectId);
    //        //Document GymnastId = new Document(WorkoutId.ParentId);

    //        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
    //        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertRoutine",
    //                new SqlParameter { ParameterName = "@Id", Value = routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
    //                //new SqlParameter { ParameterName = "@DocumentId", Value = GymnastId.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ExerciseId", Value = routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserId", Value = routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserType", Value = routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectId", Value = routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectType", Value = routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Reps", Value = (object)routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Sets", Value = (object)routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Resistance", Value = (object)routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
    //                new SqlParameter { ParameterName = "@UnitId", Value = (object)routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Note", Value = routine.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
    //            );

    //        response.StatusCode = HttpStatusCode.OK;
    //        response.Content = new StringContent("Routine successfully created");
    //    }
    //    catch (Exception ex)
    //    {
    //        response.StatusCode = HttpStatusCode.InternalServerError;
    //        response.Content = new StringContent(ex.Message);
    //        throw new HttpResponseException(response);
    //    }
    //    return response;
    //}

    [HttpPost]
    public HttpResponseMessage InsertRoutine(RoutineViewModel routineViewModel)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlConnection connection = new SqlConnection(cn);

        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();
        try
        {
            Member member = Member.GetCurrentMember();
            SetRoutineUser(routineViewModel.Routine);

            //Document WorkoutId = new Document(routine.ObjectId);
            //Document GymnastId = new Document(WorkoutId.ParentId);



            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertRoutine",
                    parameter,
                //new SqlParameter { ParameterName = "@DocumentId", Value = GymnastId.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ExerciseId", Value = routineViewModel.Routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserId", Value = routineViewModel.Routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserType", Value = routineViewModel.Routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectType", Value = routineViewModel.Routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Reps", Value = (object)routineViewModel.Routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Sets", Value = (object)routineViewModel.Routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Resistance", Value = (object)routineViewModel.Routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)routineViewModel.Routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = (object)routineViewModel.Routine.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                );
            routineViewModel.Routine.Id = Convert.ToInt32(parameter.Value);

            foreach (Story story in routineViewModel.Stories)
            {
                SetStoryUser(story);

                SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertStory",
                        new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                        new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Note", Value = (object)story.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                    );
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
           
            InsertLog(new global::Log
            {
                UserId = routineViewModel.Routine.UserId,
                UserType = routineViewModel.Routine.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routineViewModel),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertRoutine"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            transaction.Rollback();
            connection.Close();
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertRoutines(IEnumerable<RoutineViewModel> routineViewModels)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlConnection connection = new SqlConnection(cn);

        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();
        try
        {
            foreach (RoutineViewModel routineViewModel in routineViewModels)
            {
                Member member = Member.GetCurrentMember();
                SetRoutineUser(routineViewModel.Routine);

                //Document WorkoutId = new Document(routine.ObjectId);
                //Document GymnastId = new Document(WorkoutId.ParentId);

                SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
                SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertRoutine",
                        parameter,
                    //new SqlParameter { ParameterName = "@DocumentId", Value = GymnastId.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ExerciseId", Value = routineViewModel.Routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserId", Value = routineViewModel.Routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserType", Value = routineViewModel.Routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectType", Value = routineViewModel.Routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Reps", Value = (object)routineViewModel.Routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Sets", Value = (object)routineViewModel.Routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Resistance", Value = (object)routineViewModel.Routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                        new SqlParameter { ParameterName = "@UnitId", Value = (object)routineViewModel.Routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Note", Value = (object)routineViewModel.Routine.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                    );
                routineViewModel.Routine.Id = Convert.ToInt32(parameter.Value);

                foreach (Story story in routineViewModel.Stories.Where(s => s.ActionId != 0))
                {
                    SetStoryUser(story);

                    SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertStory",
                            new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                            new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@Note", Value = (object)story.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                        );
                }
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
            InsertLog(new global::Log
            {
                UserId = routineViewModels.First().Routine.UserId,
                UserType = routineViewModels.First().Routine.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routineViewModels),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertRoutines"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            transaction.Rollback();
            connection.Close();
            throw new HttpResponseException(response);
        }
        return response;
    }

    //[HttpPost]
    //public HttpResponseMessage InsertRoutineSuperSet(Routine routine)
    //{
    //    HttpResponseMessage response = new HttpResponseMessage();
    //    try
    //    {
    //        Member member = Member.GetCurrentMember();
    //        SetRoutineSuperSetUser(routine);

    //        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
    //        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertRoutine",
    //                new SqlParameter { ParameterName = "@Id", Value = routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
    //                //new SqlParameter { ParameterName = "@DocumentId", Value = routine.DocumentId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ExerciseId", Value = routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserId", Value = routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserType", Value = routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectId", Value = routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectType", Value = routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Reps", Value = (object)routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Sets", Value = (object)routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Resistance", Value = (object)routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
    //                new SqlParameter { ParameterName = "@UnitId", Value = (object)routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Note", Value = routine.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
    //            );

    //        response.StatusCode = HttpStatusCode.OK;
    //        response.Content = new StringContent("Routine successfully created");
    //    }
    //    catch (Exception ex)
    //    {
    //        response.StatusCode = HttpStatusCode.InternalServerError;
    //        response.Content = new StringContent(ex.Message);
    //        throw new HttpResponseException(response);
    //    }
    //    return response;
    //}

    [HttpPost]
    public HttpResponseMessage InsertRoutineSuperSet(RoutineViewModel routineViewModel)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlConnection connection = new SqlConnection(cn);
        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            Member member = Member.GetCurrentMember();
            SetRoutineSuperSetUser(routineViewModel.Routine);

            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertRoutine",
                    parameter,
                //new SqlParameter { ParameterName = "@DocumentId", Value = GymnastId.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ExerciseId", Value = routineViewModel.Routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserId", Value = routineViewModel.Routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserType", Value = routineViewModel.Routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectType", Value = routineViewModel.Routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Reps", Value = (object)routineViewModel.Routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Sets", Value = (object)routineViewModel.Routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Resistance", Value = (object)routineViewModel.Routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)routineViewModel.Routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = (object)routineViewModel.Routine.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                );
            routineViewModel.Routine.Id = Convert.ToInt32(parameter.Value);

            foreach (Story story in routineViewModel.Stories)
            {
                SetStoryUser(story);

                SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertStory",
                        new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                        new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Note", Value = (object)story.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                    );
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
            InsertLog(new global::Log
            {
                UserId = routineViewModel.Routine.UserId,
                UserType = routineViewModel.Routine.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routineViewModel),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertRoutineSuperSet"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            transaction.Rollback();
            connection.Close();
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertRoutinesSuperSet(IEnumerable<RoutineViewModel> routineViewModels)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlConnection connection = new SqlConnection(cn);
        connection.Open();
        SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            foreach (RoutineViewModel routineViewModel in routineViewModels)
            {
                Member member = Member.GetCurrentMember();
                SetRoutineSuperSetUser(routineViewModel.Routine);

                SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
                SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertRoutine",
                        parameter,
                    //new SqlParameter { ParameterName = "@DocumentId", Value = GymnastId.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ExerciseId", Value = routineViewModel.Routine.ExerciseId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserId", Value = routineViewModel.Routine.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@UserType", Value = routineViewModel.Routine.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@ObjectType", Value = routineViewModel.Routine.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Reps", Value = (object)routineViewModel.Routine.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Sets", Value = (object)routineViewModel.Routine.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Resistance", Value = (object)routineViewModel.Routine.Resistance ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                        new SqlParameter { ParameterName = "@UnitId", Value = (object)routineViewModel.Routine.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                        new SqlParameter { ParameterName = "@Note", Value = (object)routineViewModel.Routine.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                    );
                routineViewModel.Routine.Id = Convert.ToInt32(parameter.Value);

                foreach (Story story in routineViewModel.Stories.Where(s => s.ActionId != 0))
                {
                    SetStoryUser(story);

                    SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "InsertStory",
                            new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ObjectId", Value = routineViewModel.Routine.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                            new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                            new SqlParameter { ParameterName = "@Note", Value = (object)story.Note ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                        );
                }
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
            InsertLog(new global::Log
            {
                UserId = routineViewModels.First().Routine.UserId,
                UserType = routineViewModels.First().Routine.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(routineViewModels),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertRoutinesSuperSet"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            transaction.Rollback();
            connection.Close();
            throw new HttpResponseException(response);
        }
        return response;
    }

    private void SetRoutineUser(Routine routine)
    {
        routine.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            routine.UserId = user.Id;
            routine.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            routine.UserId = member.Id;
            routine.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    private void SetRoutineSuperSetUser(Routine routine)
    {
        routine.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "superset").Id;
        //Document WorkoutId = new Document(GetSuperSetById(routine.ObjectId).WorkoutId);
        //routine.DocumentId = new Document(WorkoutId.ParentId).Id;
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            routine.UserId = user.Id;
            routine.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            routine.UserId = member.Id;
            routine.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpPost]
    public HttpResponseMessage DeleteRoutine(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteRoutine",
                 new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteRoutine"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertStory(Story story)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            SetStoryUser(story);

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertStory",
                    new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectId", Value = story.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = story.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Story successfully created");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = story.UserId,
                UserType = story.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(story),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertStory"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    //[HttpPost]
    //public HttpResponseMessage InsertStorySuperSet(Story story)
    //{
    //    HttpResponseMessage response = new HttpResponseMessage();
    //    try
    //    {
    //        Member member = Member.GetCurrentMember();
    //        SetStorySuperSetUser(story);

    //        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
    //        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertStory",
    //                new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectId", Value = story.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@ObjectType", Value = story.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
    //                new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserId", Value = story.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@UserType", Value = story.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //                new SqlParameter { ParameterName = "@Note", Value = story.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
    //            );

    //        response.StatusCode = HttpStatusCode.OK;
    //        response.Content = new StringContent("Story successfully created");
    //    }
    //    catch (Exception ex)
    //    {
    //        response.StatusCode = HttpStatusCode.InternalServerError;
    //        response.Content = new StringContent(ex.Message);
    //        throw new HttpResponseException(response);
    //    }
    //    return response;
    //}

    [HttpPost]
    public HttpResponseMessage UpdateStory(Story story)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateStory",
                    new SqlParameter { ParameterName = "@Id", Value = story.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ActionId", Value = story.ActionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@TrainingId", Value = story.TrainingId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)story.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Value", Value = (object)story.Value ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "@TypeId", Value = (object)story.TypeId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = story.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Story successfully updated");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = story.UserId,
                UserType = story.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(story),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateStory"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeleteStory(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteStory",
                 new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Story successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteStory"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public IEnumerable<Routine> GetRoutineByWorkout(int id)
    {
        Member member = Member.GetCurrentMember();

        List<Routine> routines = new List<Routine>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRoutineByWorkout", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                routines.Add(new Routine
                {
                    Exercise = new Exercise
                    {
                        Id = Convert.ToInt32(reader.GetValue(0)),
                        ExerciseName = reader.GetValue(1).ToString(),
                        TrainerId = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                        CategoryId = Convert.ToInt32(reader.GetValue(3).ToString()),
                        Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(3)),
                        IsActive = Convert.ToBoolean(reader.GetValue(4))
                    },
                    Id = Convert.ToInt32(reader.GetValue(5).ToString()),
                    ExerciseId = Convert.ToInt32(reader.GetValue(0)),
                    Reps = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(6)),
                    Sets = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(7)),
                    Resistance = reader.IsDBNull(8) ? (decimal?)null : Convert.ToDecimal(reader.GetValue(8)),
                    UnitId = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader.GetValue(9)),
                    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(9)),
                    Note = reader.GetValue(10).ToString(),
                    StateId = reader.IsDBNull(11) ? (int?)null : Convert.ToInt32(reader.GetValue(11)),
                    State = UmbracoCustom.PropertyValue(UmbracoType.State, reader.GetValue(11)),
                    SortOrder = Convert.ToInt32(reader.GetValue(12)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(13)),
                    ObjectId = Convert.ToInt32(reader.GetValue(14))
                });
            }
        }
        return routines;
    }

    [HttpGet]
    public IEnumerable<RoutineViewModel> GetRoutineStories(int id)
    {
        Member member = Member.GetCurrentMember();
        List<RoutineViewModel> routineViewModels = new List<RoutineViewModel>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRoutineByWorkout", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                routineViewModels.Add(new RoutineViewModel
                {
                    Routine = new Routine
                    {
                        Exercise = new Exercise
                        {
                            Id = Convert.ToInt32(reader.GetValue(0)),
                            ExerciseName = reader.GetValue(1).ToString(),
                            TrainerId = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                            CategoryId = Convert.ToInt32(reader.GetValue(3).ToString()),
                            Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(3)),
                            IsActive = Convert.ToBoolean(reader.GetValue(4))
                        },
                        Id = Convert.ToInt32(reader.GetValue(5)),
                        Reps = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(6)),
                        Sets = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(7)),
                        Resistance = reader.IsDBNull(8) ? (decimal?)null : Convert.ToDecimal(reader.GetValue(8)),
                        UnitId = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader.GetValue(9)),
                        Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(9)),
                        Note = reader.GetValue(10).ToString(),
                        StateId = reader.IsDBNull(11) ? (int?)null : Convert.ToInt32(reader.GetValue(11)),
                        State = UmbracoCustom.PropertyValue(UmbracoType.State, reader.GetValue(11)),
                        SortOrder = Convert.ToInt32(reader.GetValue(12)),
                        CreatedDate = Convert.ToDateTime(reader.GetValue(13)),
                        ObjectId = Convert.ToInt32(reader.GetValue(14))
                    },
                    Stories = GetStory(Convert.ToInt32(reader.GetValue(5)))
                });
            }
        }
        return routineViewModels;
    }

    [HttpGet]
    public IEnumerable<Story> GetStory(int id)
    {
        Member member = Member.GetCurrentMember();
        List<Story> exerciseStories = new List<Story>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectStory", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                exerciseStories.Add(new Story
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ActionId = Convert.ToInt32(reader.GetValue(1)),
                    Action = UmbracoCustom.PropertyValue(UmbracoType.Action, reader.GetValue(1)),
                    TrainingId = Convert.ToInt32(reader.GetValue(2)),
                    Training = UmbracoCustom.PropertyValue(UmbracoType.Training, reader.GetValue(2)),
                    UnitId = reader.IsDBNull(3) ? (int?)null : Convert.ToInt32(reader.GetValue(3)),
                    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(3)),
                    Value = Convert.ToDecimal(reader.GetValue(4)),
                    TypeId = Convert.ToInt32(reader.GetValue(5)),
                    Type = UmbracoCustom.PropertyValue(UmbracoType.Type, reader.GetValue(5)),
                    Note = reader.GetValue(6).ToString(),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(7))
                });
            }
        }
        return exerciseStories;
    }

    private void SetStoryUser(Story story)
    {
        story.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "routine").Id;
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            story.UserId = user.Id;
            story.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            story.UserId = member.Id;
            story.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    //private void SetStorySuperSetUser(Story story)
    //{
    //    story.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "superset").Id;
    //    User user = umbraco.BusinessLogic.User.GetCurrent();
    //    Member member = Member.GetCurrentMember();
    //    if (user != null)
    //    {
    //        story.UserId = user.Id;
    //        story.UserType =
    //            UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
    //                u => u.Value.ToLower() == "trainer").Id;
    //    }
    //    else
    //    {
    //        story.UserId = member.Id;
    //        story.UserType =
    //            UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
    //                u => u.Value.ToLower() == "client").Id;
    //    }
    //}

    [HttpPost]
    public int InsertSuperSet(SuperSet superSet)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        int id = 0;
        try
        {
            Member member = Member.GetCurrentMember();
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = superSet.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertSuperSet",
                    parameter,
                    new SqlParameter { ParameterName = "@WorkoutId", Value = superSet.WorkoutId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Reps", Value = (object)superSet.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Sets", Value = (object)superSet.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ResistanceId", Value = (object)superSet.ResistanceId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)superSet.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = superSet.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
                );
            id = Convert.ToInt32(parameter.Value);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("SuperSet successfully created");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(superSet),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertSuperSet"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return id;
    }

    [HttpPost]
    public HttpResponseMessage UpdateSuperSet(SuperSet superSet)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateSuperSet",
                    new SqlParameter { ParameterName = "@Id", Value = superSet.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Reps", Value = (object)superSet.Reps ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Sets", Value = (object)superSet.Sets ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ResistanceId", Value = (object)superSet.ResistanceId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@UnitId", Value = (object)superSet.UnitId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Note", Value = superSet.Note, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar }
              );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("SuperSet updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(superSet),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateSuperSet"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeleteSuperSet(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteSuperSet",
                 new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("SuperSet successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteSuperSet"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public IEnumerable<SuperSet> GetSuperSet(int id)
    {
        Member member = Member.GetCurrentMember();
        List<SuperSet> superSets = new List<SuperSet>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectSuperSet", new SqlParameter { ParameterName = "@WorkoutId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                superSets.Add(new SuperSet
                {
                    Id = Convert.ToInt32(reader.GetValue(0).ToString()),
                    Reps = reader.IsDBNull(1) ? (int?)null : Convert.ToInt32(reader.GetValue(1)),
                    Sets = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                    ResistanceId = Convert.ToInt32(reader.GetValue(3)),
                    Resistance = UmbracoCustom.PropertyValue(UmbracoType.Resistance, reader.GetValue(3)),
                    UnitId = reader.IsDBNull(4) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(4)),
                    Note = reader.GetValue(5).ToString(),
                    WorkoutId = Convert.ToInt32(reader.GetValue(6).ToString()),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(7))
                });
            }
        }
        return superSets;
    }

    [HttpGet]
    public IEnumerable<SuperSetViewModel> GetSuperSetByWorkout(int id)
    {
        Member member = Member.GetCurrentMember();
        List<SuperSetViewModel> superSets = new List<SuperSetViewModel>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectSuperSet", new SqlParameter { ParameterName = "@WorkoutId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                superSets.Add(new SuperSetViewModel
                    {
                        SuperSet = new SuperSet
                            {
                                Id = Convert.ToInt32(reader.GetValue(0).ToString()),
                                Reps = reader.IsDBNull(1) ? (int?)null : Convert.ToInt32(reader.GetValue(1)),
                                Sets = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                                ResistanceId = Convert.ToInt32(reader.GetValue(3)),
                                Resistance = UmbracoCustom.PropertyValue(UmbracoType.Resistance, reader.GetValue(3)),
                                UnitId = reader.IsDBNull(4) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                                Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(4)),
                                Note = reader.GetValue(5).ToString(),
                                WorkoutId = Convert.ToInt32(reader.GetValue(6).ToString()),
                                CreatedDate = Convert.ToDateTime(reader.GetValue(7))
                            },
                        Routines = GetRoutineStories(Convert.ToInt32(reader.GetValue(0).ToString()))
                    });
            }
        }
        return superSets;
    }

    [HttpGet]
    public SuperSet GetSuperSetById(int id)
    {
        SuperSet superSet = new SuperSet();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectSuperSetById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                superSet = new SuperSet
                {
                    Id = Convert.ToInt32(reader.GetValue(0).ToString()),
                    Reps = reader.IsDBNull(1) ? (int?)null : Convert.ToInt32(reader.GetValue(1)),
                    Sets = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                    ResistanceId = Convert.ToInt32(reader.GetValue(3)),
                    Resistance = UmbracoCustom.PropertyValue(UmbracoType.Resistance, reader.GetValue(3)),
                    UnitId = reader.IsDBNull(4) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(4)),
                    Note = reader.GetValue(5).ToString(),
                    WorkoutId = Convert.ToInt32(reader.GetValue(6).ToString()),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(7))
                };
            }
        }
        return superSet;
    }

    #endregion

    #region Notification

    [HttpGet]
    public IEnumerable<UmbracoPreValue> GetPlattform()
    {
        int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Platform));
        return UmbracoCustom.DataTypeValue(id).Select(u => new UmbracoPreValue { Id = u.Id, Value = u.Value });
    }

    [HttpGet]
    public List<Device> GetDeviceByPlatform(int platformid)
    {
        List<Device> devices = new List<Device>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectDeviceByPlatform", new SqlParameter { ParameterName = "@PlatformId", Value = platformid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                devices.Add(new Device
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    PlatformId = Convert.ToInt32(reader.GetValue(1)),
                    Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(1)),
                    DeviceName = reader.GetValue(2).ToString(),
                    IsActive = Convert.ToBoolean(reader.GetValue(3))
                });
            }
        }
        return devices;
    }

    [HttpGet]
    public PushNotification GetNotificacion(string token, int memberid = 0)
    {
        Member member = Member.GetCurrentMember();
        List<PushNotification> notifications = GetNotificacionByMember(memberid);
        return notifications.SingleOrDefault(n => n.Token == token);
    }

    [HttpGet]
    public List<PushNotification> GetNotificacionByMember(int memberid = 0)
    {
        Member member = memberid == 0 ? Member.GetCurrentMember() : new Member(memberid);
        List<PushNotification> notifications = new List<PushNotification>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectNotificationByMember", new SqlParameter { ParameterName = "@MemberId", Value = member.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                notifications.Add(new PushNotification
                {
                    Id = int.Parse(reader.GetValue(0).ToString()),
                    MemberId = int.Parse(reader.GetValue(1).ToString()),
                    Token = reader.GetValue(2).ToString(),
                    DeviceId = int.Parse(reader.GetValue(3).ToString()),
                    IsActive = bool.Parse(reader.GetValue(4).ToString()),
                    Device = reader.GetValue(5).ToString(),
                    //PlatformId = int.Parse(reader.GetValue(6).ToString()),
                    //Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(6))
                });
            }
        }
        return notifications;
    }

    [HttpGet]
    public List<PushNotification> GetNotificacion()
    {
        List<PushNotification> notifications = new List<PushNotification>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectNotification"))
        {
            while (reader.Read())
            {
                notifications.Add(new PushNotification
                {
                    Id = int.Parse(reader.GetValue(0).ToString()),
                    MemberId = int.Parse(reader.GetValue(1).ToString()),
                    Token = reader.GetValue(2).ToString(),
                    DeviceId = int.Parse(reader.GetValue(3).ToString()),
                    IsActive = bool.Parse(reader.GetValue(4).ToString()),
                    Device = reader.GetValue(5).ToString(),
                    PlatformId = int.Parse(reader.GetValue(6).ToString()),
                    Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(6))
                });
            }
        }
        return notifications;
    }

    [HttpPost]
    public HttpResponseMessage UpdateNotification(PushNotification notification)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateNotification",
                      new SqlParameter { ParameterName = "@Id", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                      new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                      new SqlParameter { ParameterName = "@DeviceId", Value = notification.DeviceId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                      new SqlParameter { ParameterName = "@IsActive", Value = notification.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                      );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Notification updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = notification.MemberId,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(notification),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateNotification"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertNotification(PushNotification notification)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            Member member = Member.GetCurrentMember();
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertNotification",
               new SqlParameter { ParameterName = "@Id", Value = notification.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@MemberId", Value = member.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
               new SqlParameter { ParameterName = "@DeviceId", Value = notification.DeviceId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Notification was registered successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(notification),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertNotification"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage InsertEmailMessage(EmailMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SetEmailUser(message);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertEmailMessage",
               new SqlParameter { ParameterName = "@Id", Value = message.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = message.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = message.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = message.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = message.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Email", Value = message.Email, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
               new SqlParameter { ParameterName = "@Message", Value = message.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 }
               );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message was registered successfully");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = message.UserId,
                UserType = message.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(message),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertEmailMessage"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    private void SetEmailUser(EmailMessage message)
    {
        //message.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;
        User user = message.UserId != 0 ? new User(message.UserId) : umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            message.UserId = user.Id;
            message.UserType = user.LoginName.ToLower() == "system" ? UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id : UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            message.UserId = member.Id;
            message.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpPost]
    public HttpResponseMessage SendTemplateMessage(TemplateMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = new Member(message.MemberId);
            //Document trainer = new Document(Convert.ToInt32(member.getProperty("trainer").Value));
            Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Notification)));
            Document document = documents.Single(d => d.Id == message.Id);
            //switch (message.MessageType)
            //{
            //case TemplateMessageType.Email:
            if (member.getProperty("emailAlert").Value.ToString() == "1" || message.UserType != 0)
            {
                string content = message.MessageParams != null ? string.Format(document.getProperty("emailContent").Value.ToString(), message.MessageParams) : document.getProperty("emailContent").Value.ToString();
                EmailMessage emailMessage = new EmailMessage
                    {
                        Email = message.Email,
                        Message = " Member: " + member.Id + content.Replace("/media/", HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/media/").Replace("https", "http"),
                        Subject = document.getProperty("subject").Value.ToString(),
                        UserId = message.UserId,
                        ObjectId = message.ObjectId,
                        ObjectType = message.ObjectType,
                        //TrainerId = !string.IsNullOrEmpty(trainer.getProperty("").Value.ToString()) ? Convert.ToInt32(trainer.getProperty("").Value) : (int?)null
                    };
                SendEmail(emailMessage);
                //InsertEmailMessage(emailMessage);
            }
            //break;
            //case TemplateMessageType.Push:
            if (message.UserType == 0)
            {
                PushMessage pushMessage = new PushMessage
                {
                    MemberId = message.MemberId,
                    Message = document.getProperty("pushContent").Value.ToString() + " Member: " + member.Id,
                    Token = message.Token,
                    UserId = message.UserId,
                    ObjectId = message.ObjectId,
                    ObjectType = message.ObjectType,
                    Type = message.MessageType,
                    Title = document.getProperty("pushTitle").Value.ToString(),
                    Badge = message.Badge, //Convert.ToInt32(document.getProperty("badge").Value),
                    Alert = document.getProperty("alert").Value.ToString(),
                    Sound = document.getProperty("sound").Value.ToString(),
                };
                SendPush(pushMessage);
            }
            //break;
            //}

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message was registered successfully");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
               Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(message),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
               Operation = "SendTemplateMessage"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    private void PostMessage(Post post)
    {
        Topic topic = SelectTopicById(post.TopicId);
        int trainerId = UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer");
        int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
        if (topic.UserId != post.UserId && topic.UserType != trainerId)
        {
            SendTemplateMessage(new TemplateMessage
            {
                Id = 1264,//1174, 
                Email = UmbracoCustom.GetEmail(topic.UserId.ToString(), topic.UserType.ToString()),
                MemberId = topic.UserId,
                MessageType = "topicid",
                ObjectId = topic.Id,
                ObjectType = 21
            });
        }
        //List<Post> posts = SelectPost(topic.Id);
        //var x = posts.Where(p => p.UserType != UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer") && (p.UserId != post.UserId)).Select(p => new TemplateMessage
        //   {
        //       Id = 1265, //1175, 
        //       Email = UmbracoCustom.GetEmail(p.UserId.ToString(), p.UserType.ToString()),
        //       MemberId = p.UserId,
        //       MessageType = "topicid",
        //       ObjectId = p.TopicId,
        //       ObjectType = 22
        //   }).GroupBy(tm => tm.Email).Select(tm => tm.First());
        //List<TemplateMessage> list = new List<TemplateMessage>();
        //foreach (TemplateMessage templateMessage in posts.Where(p => p.UserType != UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer") && p.UserId != post.UserId).Select(p => new TemplateMessage
        //   {
        //       Id = 1265, //1175, 
        //       Email = UmbracoCustom.GetEmail(p.UserId.ToString(), p.UserType.ToString()),
        //       MemberId = p.UserId,
        //       MessageType = "topicid",
        //       ObjectId = p.TopicId,
        //       ObjectType = 22
        //   }).GroupBy(tm => tm.Email).Select(tm => tm.First()))
        //{
        foreach (TemplateMessage templateMessage in SelectPostNotification(post.TopicId, post.UserId, trainerId).Select(p => new TemplateMessage
           {
               Id = 1265, //1175, 
               Email = UmbracoCustom.GetEmail(p.ToString(), clientId.ToString()),
               MemberId = p,
               MessageType = "topicid",
               ObjectId = post.TopicId,
               ObjectType = 22
           }))
        {
            //TemplateMessage tmp = list.Find(t => t.Email == templateMessage.Email && t.MemberId == templateMessage.MemberId);
            //if (tmp == null)
            //{
            //    list.Add(templateMessage);
            //}
            SendTemplateMessage(templateMessage);
        }

        //foreach (TemplateMessage templateMessage in list)
        //{
        //    SendTemplateMessage(templateMessage);
        //}
    }

    [HttpPost]
    private void TopicMessage(Post post)
    {
        Member member = Member.GetCurrentMember();
        Topic topic = SelectTopicById(post.TopicId);
        if (post.UserType != UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer") || (member != null && topic.UserId != member.Id))
        {
            SendTemplateMessage(new TemplateMessage
            {
                Id = 1264,//1174, 
                Email = UmbracoCustom.GetEmail(topic.UserId.ToString(), topic.UserType.ToString()),
                MemberId = post.UserId,
                MessageType = "topicid",
                ObjectId = topic.Id,
                ObjectType = 21
            });
        }
    }

    [HttpPost]
    public HttpResponseMessage InsertPushMessage(PushMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            Member member = Member.GetCurrentMember();
            SetPushUser(message);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
                  new SqlParameter { ParameterName = "@Id", Value = message.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@Token", Value = message.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
                  new SqlParameter { ParameterName = "@NotificationId", Value = message.NotificationId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@UserId", Value = message.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@UserType", Value = message.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@ObjectId", Value = message.ObjectId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@ObjectType", Value = message.ObjectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@Message", Value = message.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
                  );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message was registered successfully");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = message.UserId,
                UserType = message.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertPushMessage"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    private void SetPushUser(PushMessage message)
    {
        //message.ObjectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;
        User user = message.UserId != 0 ? new User(message.UserId) : umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            message.UserId = user.Id;
            message.UserType = user.LoginName.ToLower() == "system" ? UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id : UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            message.UserId = member.Id;
            message.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpGet]
    public void SendAllNotification(int templateid, int objectId)
    {
        DateTime current = DateTime.UtcNow;
        if (current.DayOfWeek == DayOfWeek.Monday && current.Hour == 8)
        {
            foreach (
                Member member in Member.GetAllAsList().Where(m => m.getProperty("isActive").Value.ToString() == "1"))
            {
                SendTemplateMessage(new TemplateMessage
                {
                    Id = templateid,
                    Email = member.Email,
                    MemberId = member.Id,
                    UserId = 7,
                    ObjectId = 0,
                    ObjectType = objectId
                });
            }
        }
    }


    [HttpPost]
    public HttpResponseMessage SendPush(PushMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            //Member member = Member.GetCurrentMember() ?? new Member(message.MemberId);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            //PushNotification notification = GetNotificacion(message.Token, message.MemberId);
            //int userType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "client").Id;
            //int objectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;

            //string type = UmbracoCustom.PropertyValue(UmbracoType.ObjectType, message.ObjectType);
            string root = HttpContext.Current.Server.MapPath("~/certificate");
            string certName = "certificate.p12";
            byte[] cert = File.ReadAllBytes(Path.Combine(root, certName));
            PushBroker push = new PushBroker();
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;
            push.RegisterAppleService(new ApplePushChannelSettings(cert, "MetaFitness12!@"));
            AppleNotification appleNotification = new AppleNotification();
            var notifications = GetNotificacionByMember(message.MemberId);
            if (notifications.Count > 0)
            {
                foreach (PushNotification pushNotification in notifications)
                {
                    appleNotification
                        .ForDeviceToken(pushNotification.Token)
                        .WithAlert(message.Title, "", message.Alert, new object[] { })
                        .WithSound(message.Sound)
                        .WithCustomItem("message", message.Message)
                        .WithCustomItem("objectid", message.ObjectType);

                    if (message.Badge != 0)
                    {
                        appleNotification.WithBadge(message.Badge);
                    }

                    if (!string.IsNullOrEmpty(message.Type))
                    {
                        appleNotification.WithCustomItem(message.Type, message.ObjectId);
                    }

                    push.QueueNotification(appleNotification);

                    //pushService.OnDeviceSubscriptionExpired += new DeviceSubscriptionExpiredDelegate(Events_OnDeviceSubscriptionExpired);
                    //pushService.OnDeviceSubscriptionIdChanged += new  DeviceSubscriptionIdChanged(Events_OnDeviceSubscriptionIdChanged);
                    //pushService.OnChannelException += new ChannelExceptionDelegate(Events_OnChannelException);
                    //pushService.OnNotificationSendFailure += new  NotificationSendFailureDelegate(Events_OnNotificationSendFailure);
                    //pushService.OnNotificationSent += new NotificationSentDelegate(Events_OnNotificationSent);

                    //ApplePushChannelSettings settings = new ApplePushChannelSettings(false, cert, "ilovebbq");
                    //pushService.RegisterAppleService(settings);
                    //pushService.QueueNotification(new AppleNotification().ForDeviceToken(message.Token).WithAlert(message.Message).WithBadge(7));
                    //pushService.StopAllServices();
                    message.MemberId = pushNotification.MemberId;
                    message.Token = pushNotification.Token;
                    InsertPushMessage(message);
                }

            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message sent successfully");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(message),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "SendPush"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);

        }
        return response;
    }

    private void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
    {
        //Currently this event will only ever happen for Android GCM
        Console.WriteLine("Device Registration Changed: Old-> " + oldSubscriptionId + " New-> " + newSubscriptionId + " -> " + notification);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Device Registration Changed: Old-> " + oldSubscriptionId + " New-> " + newSubscriptionId + " -> " + notification);
    }

    private void NotificationSent(object sender, INotification notification)
    {
        Console.WriteLine("Sent: " + sender + " -> " + notification);
    }

    private void NotificationFailed(object sender, INotification notification, Exception notificationFailureException)
    {
        Console.WriteLine("Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification);
    }

    private void ChannelException(object sender, IPushChannel channel, Exception exception)
    {
        Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Channel Exception: " + sender + " -> " + exception);
    }

    private void ServiceException(object sender, Exception exception)
    {
        Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Channel Exception: " + sender + " -> " + exception);
    }

    private void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
    {
        Console.WriteLine("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId);
    }

    private void ChannelDestroyed(object sender)
    {
        Console.WriteLine("Channel Destroyed for: " + sender);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Channel Destroyed for: " + sender);
    }

    private void ChannelCreated(object sender, IPushChannel pushChannel)
    {
        Console.WriteLine("Channel Created for: " + sender);
        umbraco.BusinessLogic.Log.Add(LogTypes.Error, -1, "Channel Created for: " + sender);
    }

    [HttpPost]
    public HttpResponseMessage SendEmail(EmailMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {

            SmtpClient client = new SmtpClient();
            MailMessage mailMessage = new MailMessage();
            mailMessage.IsBodyHtml = true;
            mailMessage.From = new MailAddress("app@metafitnessatx.com");
            mailMessage.To.Add(message.Email);
            mailMessage.Subject = message.Subject;
            mailMessage.Body = message.Message;
            //Document d = new Document(1149);
            ////AlternateView alternateView = AlternateView.CreateAlternateViewFromString(message.Message, null, MediaTypeNames.Text.Html); //System.Text.Encoding.UTF8
            //AlternateView alternateView = AlternateView.CreateAlternateViewFromString(d.getProperty("content").Value.ToString(), null, MediaTypeNames.Text.Html); //System.Text.Encoding.UTF8
            //if (message.Attachments.Count > 0)
            //{
            //    foreach (string file in message.Attachments)
            //    {
            //        LinkedResource linkedResource = new LinkedResource(file, MediaTypeNames.Image.Jpeg)
            //            {
            //                ContentId = "photo",
            //                TransferEncoding = TransferEncoding.Base64,
            //                ContentType = {Name = "photo.jpg"}
            //            };
            //        //alternateView.LinkedResources.Add(linkedResource);
            //        //Attachment attachment = new Attachment(file, MediaTypeNames.Image.Jpeg);
            //        //ContentDisposition disposition = attachment.ContentDisposition;
            //        //disposition.CreationDate = System.IO.File.GetCreationTime(file);
            //        //disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            //        //disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            //        //disposition.DispositionType = DispositionTypeNames.Inline;
            //        //mailMessage.Attachments.Add(attachment);
            //    }
            //}
            //mailMessage.AlternateViews.Add(alternateView);
            client.Send(mailMessage);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message sent successfully");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(message),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "SendEmail"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    #endregion

    #region MessageBoard

    [HttpPost]
    public Topic InsertTopic(Topic topic)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            SetTopicUser(topic);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = topic.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTopic",
            parameter,
            new SqlParameter { ParameterName = "@Name", Value = topic.Name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 },
                //new SqlParameter { ParameterName = "@Description", Value = topic.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@UserId", Value = topic.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserType", Value = topic.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
            //topic.Id = Convert.ToInt32(parameter.Value);
            topic = SelectTopicById(Convert.ToInt32(parameter.Value));
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Topic successfully created");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = topic.UserId,
                UserType = topic.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(topic),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertTopic"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return topic;
    }

    private void SetTopicUser(Topic topic)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            topic.UserId = user.Id;
            topic.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            topic.UserId = member.Id;
            topic.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpPost]
    public Post InsertPost(Post post)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            SetPostUser(post);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = post.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPost",
                parameter,
                new SqlParameter { ParameterName = "@Message", Value = post.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
                new SqlParameter { ParameterName = "@TopicId", Value = post.TopicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@UserId", Value = post.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@UserType", Value = post.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
            post = SelectPostById(Convert.ToInt32(parameter.Value));
            PostMessage(post);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Post successfully created");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = post.UserId,
                UserType = post.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(post),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertPost"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return post;
    }


    //[HttpPost]
    //public Task<Post> InsertPost(Post post)
    //{
    //    HttpResponseMessage response = new HttpResponseMessage();
    //    try
    //    {

    //        SetPostUser(post);
    //        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
    //        SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = post.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
    //        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPost",
    //            parameter,
    //            new SqlParameter { ParameterName = "@Message", Value = post.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
    //            new SqlParameter { ParameterName = "@TopicId", Value = post.TopicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //            new SqlParameter { ParameterName = "@UserId", Value = post.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //            new SqlParameter { ParameterName = "@UserType", Value = post.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
    //        );
    //        post = SelectPostById(Convert.ToInt32(parameter.Value));
    //        response.StatusCode = HttpStatusCode.OK;
    //        response.Content = new StringContent("Post successfully created");
    //    }
    //    catch (Exception ex)
    //    {
    //        response.StatusCode = HttpStatusCode.InternalServerError;
    //        response.Content = new StringContent(ex.Message);
    //        throw new HttpResponseException(response);
    //    }
    //    return Task<Post>.Factory.StartNew(() =>
    //    {
    //        try
    //        {
    //            PostMessage(post);
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //        return post;
    //    });
    //}

    private void SetPostUser(Post post)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            post.UserId = user.Id;
            post.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            post.UserId = member.Id;
            post.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpGet]
    public TopicViewModel SelectTopic([FromUri]Paging paging)
    {
        List<Topic> topics = new List<Topic>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTopic"))
        {
            while (reader.Read())
            {
                Topic topic = new Topic
                    {
                        Id = Convert.ToInt32(reader.GetValue(0)),
                        Name = reader.GetValue(1).ToString(),
                        //Description = reader.GetValue(2).ToString(),
                        UserId = Convert.ToInt32(reader.GetValue(2)),
                        UserType = Convert.ToInt32(reader.GetValue(3)),
                        CreatedDate = Convert.ToDateTime(reader.GetValue(4).ToString()),
                        UpdatedDate = reader.IsDBNull(5) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(5).ToString()),
                        LastPost = SelectLastPost(Convert.ToInt32(reader.GetValue(0)))
                    };
                GetTopicUser(topic);
                topics.Add(topic);
            }
            paging.Records = topics.Count();
            paging.TotalPages = (int)Math.Ceiling((float)topics.Count() / paging.Pagesize);
        }
        return new TopicViewModel
            {
                Topics = topics.Skip((paging.CurrentPage - 1) * paging.Pagesize).Take(paging.Pagesize).ToList(),
                Paging = paging
            };
    }

    private void GetTopicUser(Topic topic)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, topic.UserType).ToLower();
        if (type == "trainer")
        {
            User u = new User(topic.UserId);
            topic.User = u.Name;
            topic.IsOwner = currentUser == topic.UserId;
            string[] username = u.Name.Split(' ');
            topic.FirstName = username[0];
            topic.LastName = string.Join(" ", username.Skip(1));
        }
        else
        {
            Member m = new Member(topic.UserId);
            topic.User = m.Text;
            topic.IsOwner = currentUser == topic.UserId;
            topic.FirstName = m.getProperty("firstName").Value.ToString();
            topic.LastName = m.getProperty("lastName").Value.ToString();
        }
    }

    [HttpGet]
    public List<Post> SelectPost(int id)
    {
        List<Post> posts = new List<Post>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPost", new SqlParameter { ParameterName = "@TopicId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Post post = new Post
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Message = reader.GetValue(1).ToString(),
                    TopicId = Convert.ToInt32(reader.GetValue(2)),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6)),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(7))
                };
                GetPostUser(post);
                posts.Add(post);
            }
            //paging.Records = posts.Count();
            //paging.TotalPages = (int)Math.Ceiling((float)posts.Count() / paging.Pagesize);
            //return posts.Skip((paging.CurrentPage - 1) * paging.Pagesize).Take(paging.Pagesize).ToList();
        }
        return posts;
    }

    [HttpGet]
    public List<int> SelectPostNotification(int topicId, int userId, int userType)
    {
        List<int> users = new List<int>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPostNotification",
            new SqlParameter { ParameterName = "@TopicId", Value = topicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserId", Value = userId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserType", Value = userType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            ))
        {
            while (reader.Read())
            {
                users.Add(Convert.ToInt32(reader.GetValue(0)));
            }
        }
        return users;
    }

    [HttpGet]
    public Post SelectLastPost(int topicId)
    {
        Post post = new Post();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectLastPost", new SqlParameter { ParameterName = "@TopicId", Value = topicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                post = new Post
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Message = reader.GetValue(1).ToString(),
                    TopicId = Convert.ToInt32(reader.GetValue(2)),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6)),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(7))
                };
                GetPostUser(post);
            }
        }
        return post;
    }

    [HttpGet]
    public Talk SelectLastTalk(int chatId)
    {
        Talk talk = new Talk();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectLastTalk", new SqlParameter { ParameterName = "@ChatId", Value = chatId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                talk = new Talk
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ChatId = Convert.ToInt32(reader.GetValue(1)),
                    Message = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(6))
                };
                GetTalkUser(talk);
            }
        }
        return talk;
    }

    private void GetPostUser(Post post)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, post.UserType).ToLower();

        if (type == "trainer")
        {
            User u = new User(post.UserId);
            post.User = u.Name;
            post.IsOwner = currentUser == post.UserId;
            string[] username = u.Name.Split(' ');
            post.FirstName = username[0];
            post.LastName = username[1];
        }
        else
        {
            Member m = new Member(post.UserId);
            post.User = m.Text;
            post.IsOwner = currentUser == post.UserId;
            post.FirstName = m.getProperty("firstName").Value.ToString();
            post.LastName = m.getProperty("lastName").Value.ToString();
        }
    }

    [HttpPost]
    public HttpResponseMessage DeleteTopic(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteTopic", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Topic successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteTopic"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeletePost(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeletePost", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Post successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeletePost"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateTopic(Topic topic)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateTopic",
            new SqlParameter { ParameterName = "@Id", Value = topic.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Name", Value = topic.Name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
                //new SqlParameter { ParameterName = "@Description", Value = topic.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Topic successfully updated");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(topic),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateTopic"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdatePost(Post post)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdatePost",
            new SqlParameter { ParameterName = "@Id", Value = post.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Message", Value = post.Message ?? string.Empty, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@ReportAbuse", Value = post.ReportAbuse, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Post successfully updated");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(post),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdatePost"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public Chat InsertChat(Chat chat)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            SetChatUser(chat);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = chat.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertChat",
               parameter,
               new SqlParameter { ParameterName = "@GymnastId", Value = chat.GymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Subject", Value = chat.Subject, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 2147483647 },
               new SqlParameter { ParameterName = "@UserId", Value = chat.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = chat.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );
            //chat.Id = Convert.ToInt32(parameter.Value);
            chat = SelectChatById(Convert.ToInt32(parameter.Value));
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Chat successfully created");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = chat.UserId,
                UserType = chat.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(chat),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertChat"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return chat;
    }

    private void SetChatUser(Chat chat)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            chat.UserId = user.Id;
            chat.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            chat.UserId = member.Id;
            chat.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpPost]
    public Talk InsertTalk(Talk talk)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            bool isTrainer = SetTalkUser(talk);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = talk.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTalk",
               parameter,
               new SqlParameter { ParameterName = "@ChatId", Value = talk.ChatId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Message", Value = talk.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 },
               new SqlParameter { ParameterName = "@UserId", Value = talk.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = talk.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );
            talk = SelectTalkById(Convert.ToInt32(parameter.Value));
            if (isTrainer)
            {
                UpdateChat(new Chat { Id = talk.ChatId, IsRead = false });
            }
            else
            {
                int trainerId = UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer");
                Account account = MemberIsLoggedOn();
                    SendTemplateMessage(new TemplateMessage
                    {
                        Id = 3881, //1174, 
                        MessageParams = new object[]{account.FirstName, account.LastName, account.Email},
                        Email = UmbracoCustom.GetEmail(account.TrainerId.ToString(), trainerId.ToString()),
                        MemberId = account.Id,
                        UserType = trainerId,
                        MessageType = "chatid",
                        ObjectId = talk.ChatId,
                        ObjectType = 23
                    });
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Talk successfully created");
        }
        catch (Exception ex)
        {
            InsertLog(new global::Log
            {
                UserId = talk.UserId,
                UserType = talk.UserType,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(talk),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "InsertTalk"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return talk;
    }

    private bool SetTalkUser(Talk talk)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        bool isTrainer = false;
        if (user != null)
        {
            talk.UserId = user.Id;
            talk.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
            isTrainer = true;
        }
        else
        {
            talk.UserId = member.Id;
            talk.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
        return isTrainer;
    }

    [HttpGet]
    public ChatViewModel SelectChat([FromUri]Paging paging)
    {
        List<Chat> chats = new List<Chat>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectChat", new SqlParameter { ParameterName = "@GymnastId", Value = paging.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Chat chat = new Chat
                    {
                        Id = Convert.ToInt32(reader.GetValue(0)),
                        GymnastId = Convert.ToInt32(reader.GetValue(1)),
                        Subject = reader.GetValue(2).ToString(),
                        UserId = Convert.ToInt32(reader.GetValue(3)),
                        UserType = Convert.ToInt32(reader.GetValue(4)),
                        CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                        IsRead = Convert.ToBoolean(reader.GetValue(6)),
                        LastTalk = SelectLastTalk(Convert.ToInt32(reader.GetValue(0)))
                    };
                GetChatUser(chat);
                chats.Add(chat);
            }
            paging.Records = chats.Count();
            paging.TotalPages = (int)Math.Ceiling((float)chats.Count() / paging.Pagesize);
        }
        return new ChatViewModel
            {
                Chats = chats.Skip((paging.CurrentPage - 1) * paging.Pagesize).Take(paging.Pagesize).ToList(),
                Paging = paging
            };
    }

    private void GetChatUser(Chat chat)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, chat.UserType).ToLower();

        if (type == "trainer")
        {
            User u = new User(chat.UserId);
            chat.User = u.Name;
            chat.IsOwner = currentUser == chat.UserId;
            string[] username = u.Name.Split(' ');
            chat.FirstName = username[0];
            chat.LastName = username[1];
        }
        else
        {
            Member m = new Member(chat.UserId);
            chat.User = m.Text;
            chat.IsOwner = currentUser == chat.UserId;
            chat.FirstName = m.getProperty("firstName").Value.ToString();
            chat.LastName = m.getProperty("lastName").Value.ToString();
        }
    }

    [HttpGet]
    public List<Talk> SelectTalk(int id)
    {
        List<Talk> talks = new List<Talk>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTalk", new SqlParameter { ParameterName = "@ChatId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Talk talk = new Talk
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ChatId = Convert.ToInt32(reader.GetValue(1)),
                    Message = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(6))
                };
                GetTalkUser(talk);
                talks.Add(talk);
            }
            //paging.Records = talks.Count();
            //paging.TotalPages = (int)Math.Ceiling((float)talks.Count() / paging.Pagesize);
            //return talks.Skip((paging.CurrentPage - 1) * paging.Pagesize).Take(paging.Pagesize).ToList();
        }
        return talks;
    }

    [HttpGet]
    public int SelectNumberUnreadChat(int gymnastId)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        return Convert.ToInt32(SqlHelper.ExecuteScalar(cn, CommandType.StoredProcedure, "SelectNumberUnreadChat", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }));
    }

    [HttpGet]
    public int SelectNumberChat(int gymnastId)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        return Convert.ToInt32(SqlHelper.ExecuteScalar(cn, CommandType.StoredProcedure, "SelectNumberChat", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }));
    }

    [HttpGet]
    public int SelectNumberTopic(int gymnastId)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        return Convert.ToInt32(SqlHelper.ExecuteScalar(cn, CommandType.StoredProcedure, "SelectNumberTopic", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }));
    }

    [HttpGet]
    public List<Chat> SelectRecentChat(int gymnastId)
    {
        List<Chat> chats = new List<Chat>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRecentChat", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Chat chat = new Chat
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Subject = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    IsRead = Convert.ToBoolean(reader.GetValue(6)),
                    LastTalk = SelectLastTalk(Convert.ToInt32(reader.GetValue(0)))
                };
                GetChatUser(chat);
                chats.Add(chat);
            }
        }
        return chats;
    }

    [HttpGet]
    public List<Topic> SelectRecentTopic()
    {
        List<Topic> topics = new List<Topic>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRecentTopic"))
        {
            while (reader.Read())
            {
                Topic topic = new Topic
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Name = reader.GetValue(1).ToString(),
                    //Description = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(2)),
                    UserType = Convert.ToInt32(reader.GetValue(3)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(4).ToString()),
                    UpdatedDate = reader.IsDBNull(5) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(5).ToString())
                    //LastPost = SelectLastPost(Convert.ToInt32(reader.GetValue(0)))
                };
                GetTopicUser(topic);
                topics.Add(topic);
            }
        }
        return topics;
    }

    [HttpGet]
    public List<Chat> SelectUnreadRecentChat(int gymnastId)
    {
        List<Chat> chats = new List<Chat>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectUnreadRecentChat", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Chat chat = new Chat
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Subject = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    IsRead = Convert.ToBoolean(reader.GetValue(6)),
                    LastTalk = SelectLastTalk(Convert.ToInt32(reader.GetValue(0)))
                };
                GetChatUser(chat);
                chats.Add(chat);
            }
        }
        return chats;
    }

    [HttpGet]
    public List<Chat> SelectReadRecentChat(int gymnastId)
    {
        List<Chat> chats = new List<Chat>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectReadRecentChat", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                Chat chat = new Chat
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Subject = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    IsRead = Convert.ToBoolean(reader.GetValue(6)),
                    LastTalk = SelectLastTalk(Convert.ToInt32(reader.GetValue(0)))
                };
                GetChatUser(chat);
                chats.Add(chat);
            }
        }
        return chats;
    }

    private void GetTalkUser(Talk talk)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, talk.UserType).ToLower();

        if (type == "trainer")
        {
            User u = new User(talk.UserId);
            talk.User = u.Name;
            talk.IsOwner = currentUser == talk.UserId;
            string[] username = u.Name.Split(' ');
            talk.FirstName = username[0];
            talk.LastName = username[1];
        }
        else
        {
            Member m = new Member(talk.UserId);
            talk.User = m.Text;
            talk.IsOwner = currentUser == talk.UserId;
            talk.FirstName = m.getProperty("firstName").Value.ToString();
            talk.LastName = m.getProperty("lastName").Value.ToString();
        }
    }

    [HttpPost]
    public HttpResponseMessage UpdateChat(Chat chat)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateChat",
            new SqlParameter { ParameterName = "@Id", Value = chat.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Subject", Value = chat.Subject ?? string.Empty, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@IsRead", Value = chat.IsRead, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Chat successfully updated");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(chat),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateChat"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage UpdateTalk(Talk talk)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateTalk",
            new SqlParameter { ParameterName = "@Id", Value = talk.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Message", Value = talk.Message ?? string.Empty, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@ReportAbuse", Value = talk.ReportAbuse, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Talk successfully updated");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(talk),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateTalk"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeleteChat(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteChat", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Chat successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteChat"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage DeleteTalk(int id)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteTalk", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Talk successfully deleted");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = id.ToString(),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "DeleteTalk"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public Talk SelectTalkById(int id)
    {
        Talk talk = new Talk();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTalkById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                talk = new Talk
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ChatId = Convert.ToInt32(reader.GetValue(1)),
                    Message = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString()),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(7))
                };
                GetTalkUser(talk);
            }
        }
        return talk;
    }

    [HttpGet]
    public Chat SelectChatById(int id)
    {
        Chat chat = new Chat();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectChatById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                chat = new Chat
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Subject = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString()),
                    IsRead = Convert.ToBoolean(reader.GetValue(7))
                };
                GetChatUser(chat);
            }
        }
        return chat;
    }

    [HttpGet]
    public Post SelectPostById(int id)
    {
        Post post = new Post();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPostById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                post = new Post
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Message = reader.GetValue(1).ToString(),
                    TopicId = Convert.ToInt32(reader.GetValue(2)),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5)),
                    UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString()),
                    ReportAbuse = Convert.ToBoolean(reader.GetValue(7))
                };
                GetPostUser(post);
            }
        }
        return post;
    }

    [HttpGet]
    public Topic SelectTopicById(int id)
    {
        Topic topic = new Topic();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTopicById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                topic = new Topic
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    Name = reader.GetValue(1).ToString(),
                    //Description = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(2)),
                    UserType = Convert.ToInt32(reader.GetValue(3)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(4)),
                    UpdatedDate = reader.IsDBNull(5) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(5).ToString())
                };
                GetTopicUser(topic);
            }
        }
        return topic;
    }

    #endregion

    #region Measurement

    [HttpPost]
    public Task<HttpResponseMessage> InsertMeasurement()
    {
        if (!Request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        int trainerId = UmbracoCustom.PropertyId(UmbracoType.UserType, "trainer");
        Account account = MemberIsLoggedOn(); //SelectMemberByGymnast(gymnastId);//;SelectMemberByGymnast(Convert.ToInt32(provider.FormData.GetValues("GymnastId")[0]));
        if (account.TrainerId != 0)
        {
            SendTemplateMessage(new TemplateMessage
            {
                Id = 3883, //1174, 
                MessageParams = new object[] {account.FirstName, account.LastName, account.Email},
                Email = UmbracoCustom.GetEmail(account.TrainerId.ToString(), trainerId.ToString()),
                MemberId = account.Id,
                UserId = account.Id,
                UserType = trainerId
            });
        }
        //string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //string root = HttpContext.Current.Server.MapPath(UmbracoCustom.GetParameterValue(UmbracoType.Temp));
        //MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);
        string root = UmbracoCustom.GetParameterValue(UmbracoType.Temp);
        CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(root);

        Request.Content.LoadIntoBufferAsync().Wait();
        var task = Request.Content.ReadAsMultipartAsync(provider).
            ContinueWith<HttpResponseMessage>(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception + " IsFaulted");
                }

                try
                {
                    // Show all the key-value pairs.
                    //Measurement measurement = new Measurement
                    //    {
                    //        GymnastId = Convert.ToInt32(provider.FormData.GetValues("GymnastId")[0]),
                    //        Weight = Convert.ToDecimal(provider.FormData.GetValues("Weight")[0]),
                    //        Neck = Convert.ToDecimal(provider.FormData.GetValues("Neck")[0]),
                    //        Shoulders = Convert.ToDecimal(provider.FormData.GetValues("Shoulders")[0]),
                    //        RightArm = Convert.ToDecimal(provider.FormData.GetValues("RightArm")[0]),
                    //        LeftArm = Convert.ToDecimal(provider.FormData.GetValues("LeftArm")[0]),
                    //        Chest = Convert.ToDecimal(provider.FormData.GetValues("Chest")[0]),
                    //        BellyButton = Convert.ToDecimal(provider.FormData.GetValues("BellyButton")[0]),
                    //        Hips = Convert.ToDecimal(provider.FormData.GetValues("Hips")[0]),
                    //        RightThigh = Convert.ToDecimal(provider.FormData.GetValues("RightThigh")[0]),
                    //        LeftThigh = Convert.ToDecimal(provider.FormData.GetValues("LeftThigh")[0]),
                    //        RightCalf = Convert.ToDecimal(provider.FormData.GetValues("RightCalf")[0]),
                    //        LeftCalf = Convert.ToDecimal(provider.FormData.GetValues("LeftCalf")[0]),
                    //        Arm = Convert.ToDecimal(provider.FormData.GetValues("Arm")[0]),
                    //        Waist = Convert.ToDecimal(provider.FormData.GetValues("Waist")[0]),
                    //        Thigh = Convert.ToDecimal(provider.FormData.GetValues("Thigh")[0]),
                    //        Back = Convert.ToDecimal(provider.FormData.GetValues("Back")[0])
                    //    };
                    
                    string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
                    SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
                    int gymnastId = Convert.ToInt32(provider.FormData.GetValues("GymnastId")[0]);
                    SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertMeasurement",
                      parameter,
                      new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                      new SqlParameter { ParameterName = "@Weight", Value = Convert.ToDecimal(provider.FormData.GetValues("Weight")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Neck", Value = Convert.ToDecimal(provider.FormData.GetValues("Neck")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Shoulders", Value = Convert.ToDecimal(provider.FormData.GetValues("Shoulders")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@RightArm", Value = Convert.ToDecimal(provider.FormData.GetValues("RightArm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@LeftArm", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftArm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Chest", Value = Convert.ToDecimal(provider.FormData.GetValues("Chest")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@BellyButton", Value = Convert.ToDecimal(provider.FormData.GetValues("BellyButton")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Hips", Value = Convert.ToDecimal(provider.FormData.GetValues("Hips")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@RightThigh", Value = Convert.ToDecimal(provider.FormData.GetValues("RightThigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@LeftThigh", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftThigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@RightCalf", Value = Convert.ToDecimal(provider.FormData.GetValues("RightCalf")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@LeftCalf", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftCalf")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Arm", Value = Convert.ToDecimal(provider.FormData.GetValues("Arm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Waist", Value = Convert.ToDecimal(provider.FormData.GetValues("Waist")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Thigh", Value = Convert.ToDecimal(provider.FormData.GetValues("Thigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@Back", Value = Convert.ToDecimal(provider.FormData.GetValues("Back")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal, Precision = 8, Scale = 3 },
                      new SqlParameter { ParameterName = "@HasPhotos", Value = provider.FileData.Count > 0, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                      );

                    string imagePath = UmbracoCustom.GetParameterValue(UmbracoType.Photo) + gymnastId;
                    //string datePath = Path.Combine(imagePath, DateTime.Now.ToString("MMddyyyy"));
                    string datePath = Path.Combine(imagePath, Convert.ToInt32(parameter.Value).ToString());
                    DirectoryInfo directoryInfo = !Directory.Exists(imagePath) ? Directory.CreateDirectory(imagePath) : new DirectoryInfo(imagePath);
                    DirectoryInfo directoryDate = !Directory.Exists(datePath) ? Directory.CreateDirectory(datePath) : new DirectoryInfo(datePath);

                    // This illustrates how to get the file names.
                    foreach (MultipartFileData file in provider.FileData)
                    {
                        FileInfo info = new FileInfo(file.LocalFileName);
                        Stream stream = new FileStream(file.LocalFileName, FileMode.Open);
                        stream.Position = 0;
                        UmbracoFile imageUpload = UmbracoFile.Save(stream, string.Format(@"{0}\{1}", directoryDate.FullName, info.Name));
                        imageUpload.Resize(100, "thumb");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Measurement succesfully created");
            });

        return task;
    }

    [HttpPost]
    public HttpResponseMessage UpdateMeasurement(Measurement measurement)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateMeasurement",
              new SqlParameter { ParameterName = "@Id", Value = measurement.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
              new SqlParameter { ParameterName = "@Weight", Value = measurement.Weight, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Neck", Value = measurement.Neck, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Shoulders", Value = measurement.Shoulders, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@RightArm", Value = measurement.RightArm, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@LeftArm", Value = measurement.LeftArm, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Chest", Value = measurement.Chest, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@BellyButton", Value = measurement.BellyButton, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Hips", Value = measurement.Hips, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@RightThigh", Value = measurement.RightThigh, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@LeftThigh", Value = measurement.LeftThigh, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@RightCalf", Value = measurement.RightCalf, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@LeftCalf", Value = measurement.LeftCalf, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Arm", Value = measurement.Arm, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Waist", Value = measurement.Waist, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Thigh", Value = measurement.Thigh, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@Back", Value = measurement.Back, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
              new SqlParameter { ParameterName = "@HasPhotos", Value = measurement.HasPhotos, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
              );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Measurement was updated successfully");
        }
        catch (Exception ex)
        {
            int clientId = UmbracoCustom.PropertyId(UmbracoType.UserType, "client");
            InsertLog(new global::Log
            {
                UserId = Member.GetCurrentMember().Id,
                UserType = clientId,
                Endpoint = HttpContext.Current.Request.Url.PathAndQuery,
                RequestObject = JsonConvert.SerializeObject(measurement),
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Operation = "UpdateMeasurement"
            });
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpGet]
    public IEnumerable<Photo> GetMeasurementPhoto(int id, bool base64 = false)
    {
        List<Photo> photos = new List<Photo>();
        Measurement measurement = SelectMeasurementById(id);
        IEnumerable<string> files = Directory.GetFiles(Path.Combine(new[] { UmbracoCustom.GetParameterValue(UmbracoType.Photo), measurement.GymnastId.ToString(), measurement.Id.ToString() })).Where(f => !f.Contains("_thumb"));
        foreach (string file in files)
        {
            FileInfo info = new FileInfo(file);
            Image image = Image.FromFile(file);
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            photos.Add(new Photo
                {
                    FileName = info.Name,
                    PhotoArray = base64 ? new byte[] { } : memoryStream.ToArray(),
                    Base64 = base64 ? string.Format("data:image/jpeg;base64,{0}", UmbracoCustom.ImageToBase64(image, ImageFormat.Png)) : string.Empty
                });
        }
        return photos;
    }

    [HttpGet]
    public IEnumerable<PhotoViewModel> GetAllMeasurementPhoto(int gymnastId)
    {
        List<PhotoViewModel> photoViewModels = new List<PhotoViewModel>();
        List<Measurement> measurements = SelectMeasurementByGymnastId(gymnastId);

        foreach (Measurement measurement in measurements)
        {
            List<Photo> photos = new List<Photo>();
            IEnumerable<string> files =
                Directory.GetFiles(
                    Path.Combine(new[]
                        {
                            UmbracoCustom.GetParameterValue(UmbracoType.Photo), 
                            measurement.GymnastId.ToString(),
                            measurement.Id.ToString()
                        })).Where(f => !f.Contains("_thumb"));

            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                Image image = Image.FromFile(file);
                MemoryStream memoryStream = new MemoryStream();
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                photos.Add(new Photo
                {
                    FileName = info.Name,
                    PhotoArray = memoryStream.ToArray()
                });
            }

            photoViewModels.Add(new PhotoViewModel
                {
                    Id = measurement.Id,
                    Photos = photos
                });
        }
        return photoViewModels;
    }

    [HttpGet]
    public Measurement SelectMeasurementById(int id)
    {
        Measurement measurement = new Measurement();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                measurement = new Measurement
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Weight = Convert.ToDecimal(reader.GetValue(2)),
                    Neck = Convert.ToDecimal(reader.GetValue(3)),
                    Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                    RightArm = Convert.ToDecimal(reader.GetValue(5)),
                    LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                    Chest = Convert.ToDecimal(reader.GetValue(7)),
                    BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                    Hips = Convert.ToDecimal(reader.GetValue(9)),
                    RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                    LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                    RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                    LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                    Arm = Convert.ToDecimal(reader.GetValue(14)),
                    Waist = Convert.ToDecimal(reader.GetValue(15)),
                    Thigh = Convert.ToDecimal(reader.GetValue(16)),
                    Back = Convert.ToDecimal(reader.GetValue(17)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString()),
                    HasPhotos = Convert.ToBoolean(reader.GetValue(19))
                };
            }
        }
        return measurement;
    }

    [HttpGet]
    public List<Measurement> SelectMeasurementByGymnastId(int gymnastId)
    {
        List<Measurement> measurements = new List<Measurement>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastId", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                measurements.Add(new Measurement
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Weight = Convert.ToDecimal(reader.GetValue(2)),
                    Neck = Convert.ToDecimal(reader.GetValue(3)),
                    Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                    RightArm = Convert.ToDecimal(reader.GetValue(5)),
                    LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                    Chest = Convert.ToDecimal(reader.GetValue(7)),
                    BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                    Hips = Convert.ToDecimal(reader.GetValue(9)),
                    RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                    LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                    RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                    LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                    Arm = Convert.ToDecimal(reader.GetValue(14)),
                    Waist = Convert.ToDecimal(reader.GetValue(15)),
                    Thigh = Convert.ToDecimal(reader.GetValue(16)),
                    Back = Convert.ToDecimal(reader.GetValue(17)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString()),
                    HasPhotos = Convert.ToBoolean(reader.GetValue(19))
                });
            }
        }
        return measurements;
    }

    [HttpGet]
    public Measurement SelectLastMeasurement(int gymnastId)
    {
        Measurement measurement = new Measurement();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectLastMeasurement", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                measurement = new Measurement
                    {
                        Id = Convert.ToInt32(reader.GetValue(0)),
                        GymnastId = Convert.ToInt32(reader.GetValue(1)),
                        Weight = Convert.ToDecimal(reader.GetValue(2)),
                        Neck = Convert.ToDecimal(reader.GetValue(3)),
                        Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                        RightArm = Convert.ToDecimal(reader.GetValue(5)),
                        LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                        Chest = Convert.ToDecimal(reader.GetValue(7)),
                        BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                        Hips = Convert.ToDecimal(reader.GetValue(9)),
                        RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                        LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                        RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                        LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                        Arm = Convert.ToDecimal(reader.GetValue(14)),
                        Waist = Convert.ToDecimal(reader.GetValue(15)),
                        Thigh = Convert.ToDecimal(reader.GetValue(16)),
                        Back = Convert.ToDecimal(reader.GetValue(17)),
                        CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString()),
                        HasPhotos = Convert.ToBoolean(reader.GetValue(19))
                    };
            }
        }
        return measurement;
    }

    [HttpGet]
    public List<Measurement> SelectMeasurementByGymnastByDate(int gymnastId, DateTime fromDate, DateTime toDate)
    {
        List<Measurement> measurements = new List<Measurement>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastIdByDate",
            new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@FromDate", Value = fromDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
            new SqlParameter { ParameterName = "@ToDate", Value = toDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime }
            ))
        {
            while (reader.Read())
            {
                measurements.Add(new Measurement
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Weight = Convert.ToDecimal(reader.GetValue(2)),
                    Neck = Convert.ToDecimal(reader.GetValue(3)),
                    Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                    RightArm = Convert.ToDecimal(reader.GetValue(5)),
                    LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                    Chest = Convert.ToDecimal(reader.GetValue(7)),
                    BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                    Hips = Convert.ToDecimal(reader.GetValue(9)),
                    RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                    LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                    RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                    LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                    Arm = Convert.ToDecimal(reader.GetValue(14)),
                    Waist = Convert.ToDecimal(reader.GetValue(15)),
                    Thigh = Convert.ToDecimal(reader.GetValue(16)),
                    Back = Convert.ToDecimal(reader.GetValue(17)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString()),
                    HasPhotos = Convert.ToBoolean(reader.GetValue(19))
                });
            }
        }
        return measurements;
    }

    [HttpGet]
    public List<Measurement> SelectFirstLastMeasurement(int gymnastId)
    {
        List<Measurement> measurements = new List<Measurement>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        using (SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectFirstLastMeasurement", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }))
        {
            while (reader.Read())
            {
                measurements.Add(new Measurement
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Weight = Convert.ToDecimal(reader.GetValue(2)),
                    Neck = Convert.ToDecimal(reader.GetValue(3)),
                    Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                    RightArm = Convert.ToDecimal(reader.GetValue(5)),
                    LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                    Chest = Convert.ToDecimal(reader.GetValue(7)),
                    BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                    Hips = Convert.ToDecimal(reader.GetValue(9)),
                    RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                    LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                    RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                    LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                    Arm = Convert.ToDecimal(reader.GetValue(14)),
                    Waist = Convert.ToDecimal(reader.GetValue(15)),
                    Thigh = Convert.ToDecimal(reader.GetValue(16)),
                    Back = Convert.ToDecimal(reader.GetValue(17)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString()),
                    HasPhotos = Convert.ToBoolean(reader.GetValue(19))
                });
            }
        }
        return measurements;
    }

    #endregion

    #region Others

    [HttpGet]
    public string GetDocument(string name)
    {
        Document document = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Site)));
        Document result = document.Children.SingleOrDefault(d => d.Text.ToLower() == name.ToLower());
        return result.getProperty("body").Value.ToString();
    }

    [HttpGet]
    public IEnumerable<UmbracoPreValue> GetUmbracoPreValue(int id)
    {
        return UmbracoCustom.DataTypeValue(id).Select(u => new UmbracoPreValue { Id = u.Id, Value = u.Value });
    }

    [HttpGet]
    public InfomationViewModel GetInfomation(int gymnastId)
    {
        Member member = Member.GetCurrentMember();
        return new InfomationViewModel
            {
                Measurements = SelectFirstLastMeasurement(gymnastId).Select(m => new MeasurementViewModel
                    {
                        Measurement = m
                        //Photos = GetMeasurementPhoto(m.Id).ToList()
                    }).ToList(),
                RecentChats = SelectRecentChat(gymnastId),
                ReadChats = SelectReadRecentChat(gymnastId),
                UnreadChats = SelectUnreadRecentChat(gymnastId),
                NumberUnreadChat = SelectNumberUnreadChat(gymnastId),
                NumberNotViewedWorkout = SelectNumberNotViewedWorkout()
            };
    }

    [HttpGet]
    public DashboardViewModel GetDashboardInfomation(int gymnastId, int memberId)
    {
        Member member = Member.GetCurrentMember();
        User user = umbraco.BusinessLogic.User.GetCurrent();
        return new DashboardViewModel
        {
            Account = SelectMemberById(memberId),
            Avatar = GetMemberAvatar(memberId),
            Measurements = SelectFirstLastMeasurement(gymnastId).Select(m => new MeasurementViewModel
            {
                Measurement = m,
                Photos = GetMeasurementPhoto(m.Id, true).ToList()
            }).ToList(),
            Chats = SelectRecentChat(gymnastId),
            UnreadChats = SelectUnreadRecentChat(gymnastId),
            NumberChat = SelectNumberChat(gymnastId),
            NumberUnreadChat = SelectNumberUnreadChat(gymnastId),
            Topics = SelectRecentTopic(),
            NumberTopic = SelectNumberTopic(gymnastId),
            NumberWorkout = SelectNumberWorkout(memberId),
            NumberViewedWorkout = SelectNumberViewedWorkout(memberId),
            NumberNotViewedWorkout = SelectNumberNotViewedWorkout(memberId),
            NumberCompleteWorkout = SelectNumberCompleteWorkout(memberId),
            NumberNotCompleteWorkout = SelectNumberNotCompleteWorkout(memberId)
        };
    }

    [HttpPost]
    public HttpResponseMessage InsertLog(global::Log log)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            SetLogUser(log);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertLog",
            new SqlParameter { ParameterName = "@Id", Value = log.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserId", Value = (object)log.UserId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserType", Value = (object)log.UserType ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Endpoint", Value = log.Endpoint, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@RequestObject", Value = log.RequestObject, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@ExceptionMessage", Value = log.ExceptionMessage, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@ExceptionType", Value = log.ExceptionType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@StackTrace", Value = log.StackTrace, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@Source", Value = log.Source, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@Operation", Value = log.Operation, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 2147483647 },
            new SqlParameter { ParameterName = "@ObjectId", Value = (object)log.ObjectId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@ObjectType", Value = (object)log.ObjectType ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Log successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }


    private void SetLogUser(global::Log log)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            log.UserId = user.Id;
            log.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            log.UserId = member.Id;
            log.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    #endregion

    #region Test

    [HttpPost]
    public HttpResponseMessage TestInsert(RoutineViewModel routineViewModel)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("It Works");
        return response;
    }

    [HttpPost]
    public HttpResponseMessage TestInsert2(RoutineViewModel routineViewModel)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("It Works");
        return response;
    }

    [HttpGet]
    public DateTime GetDateTime(string datetime)
    {
        DateTime result;
        bool parse = DateTime.TryParse(datetime, out result);
        return result;
    }

    [HttpGet]
    public IEnumerable<Workout> SelectWorkouts()
    {
        List<Workout> workouts = new List<Workout>();
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        foreach (Document child in documents[1].Children)
        {

            Workout w = new Workout();

            w.Id = child.Id;
            w.ParentId = child.ParentId;
            w.Name = child.Text;
            w.DateScheduled = (child.getProperty("dateScheduled").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateScheduled").Value) : (DateTime?)null);
            w.DateCompleted = (child.getProperty("dateCompleted").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateCompleted").Value) : (DateTime?)null);
            w.Description = child.getProperty("description").Value.ToString();
            w.StateId = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? Convert.ToInt32(child.getProperty("state").Value) : (int?)null);
            w.State = (!string.IsNullOrEmpty(child.getProperty("state").Value.ToString()) ? UmbracoCustom.PropertyValue(UmbracoType.WorkoutState, Convert.ToInt32(child.getProperty("state").Value)) : string.Empty);
            //w.RateId = (child.getProperty("rate").Value.ToString() != "" ? Convert.ToInt32(child.getProperty("rate").Value) : (int?)null),
            //w.Rate = UmbracoCustom.PropertyValue(UmbracoType.Rate, child.getProperty("rate")),
            w.Note = child.getProperty("note").Value.ToString();
            w.CreatedDate = child.CreateDateTime;
            w.UpdatedDate = child.UpdateDate;
            workouts.Add(w);
        }


        return workouts;
    }

    #endregion
}
