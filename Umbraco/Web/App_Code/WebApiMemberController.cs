using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using System.Xml.Linq;
using Microsoft.ApplicationBlocks.Data;
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
using File = System.IO.File;
using Log = umbraco.BusinessLogic.Log;
using Property = umbraco.cms.businesslogic.property.Property;

public class MemberController : ApiController
{
    private PushBroker pushService;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.Http.ApiController"/> class.
    /// </summary>
    public MemberController()
    {
        string root = HttpContext.Current.Server.MapPath("~/certificate");
        string certName = "metafitness.p12";
        byte[] cert = File.ReadAllBytes(Path.Combine(root, certName));

        pushService = new PushBroker();
        //pushService.Events.OnDeviceSubscriptionExpired += new ChannelEvents.DeviceSubscriptionExpired(Events_OnDeviceSubscriptionExpired);
        //pushService.Events.OnDeviceSubscriptionIdChanged += new ChannelEvents.DeviceSubscriptionIdChanged(Events_OnDeviceSubscriptionIdChanged);
        //pushService.Events.OnChannelException += new ChannelEvents.ChannelExceptionDelegate(Events_OnChannelException);
        //pushService.Events.OnNotificationSendFailure += new ChannelEvents.NotificationSendFailureDelegate(Events_OnNotificationSendFailure);
        //pushService.Events.OnNotificationSent += new ChannelEvents.NotificationSentDelegate(Events_OnNotificationSent);

        ApplePushChannelSettings settings = new ApplePushChannelSettings(false, cert, "ilovebbq");
        pushService.RegisterAppleService(settings);
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
            UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
            TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : 0,
            EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
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
            MembershipUser user = Membership.GetUser(changePassword.LoginName);
            user.ChangePassword(changePassword.Password, changePassword.NewPassword);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Password changed successfully");
        }
        catch (Exception ex)
        {
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
            SmtpClient client = new SmtpClient();
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress("app@metafitnessatx.com");
            message.To.Add(user.Email);
            message.Subject = "MetaFitness Password Reset";
            message.Body = string.Format("<h2>Password Reset</h2><div>Your password has been reset through the MetaFitness App. You new password is:</div><div></div><div style='background-color: grey; color: white; height: 40px; width: 150px; text-align: center; padding-top: 16px;; font-size: 14pt;'>{0}</div><div></div><div>Your login is your email address.</div><div></div><div>Thank you for using MetaFitness.</div>", newPassword);
            client.Send(message);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Reset Password successfully");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            //throw new HttpResponseException(response);
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
            UseMetric = member.getProperty("useMetric").Value.ToString() == "1",
            TrainerId = document.getProperty("trainer").Value.ToString() != string.Empty ? Convert.ToInt32(document.getProperty("trainer").Value) : 0,
            EmailAlert = member.getProperty("emailAlert").Value.ToString() == "1",
        };
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
            member.getProperty("birthday").Value = account.Birthday.HasValue ? account.Birthday.Value.ToShortDateString() : string.Empty;
            member.getProperty("city").Value = account.City ?? string.Empty;
            member.getProperty("state").Value = account.State ?? string.Empty;
            member.getProperty("country").Value = account.Country ?? string.Empty;
            member.getProperty("zipCode").Value = account.ZipCode ?? string.Empty;
            member.getProperty("address").Value = account.Address ?? string.Empty;
            member.getProperty("phone").Value = account.Phone ?? string.Empty;
            member.getProperty("height").Value = account.Height.HasValue ? account.Height.ToString() : string.Empty;
            member.getProperty("startWeight").Value = account.StartWeight.HasValue ? account.Height.ToString() : string.Empty;
            member.getProperty("goalWeight").Value = account.GoalWeight.HasValue ? account.Height.ToString() : string.Empty;
            //member.getProperty("dob").Value = account.Birthday.HasValue ? account.Birthday.Value.ToShortDateString() : string.Empty;
            member.getProperty("useMetric").Value = account.UseMetric ? "1" : "0";
            member.getProperty("emailAlert").Value = account.EmailAlert ? "1" : "0";
            member.Save();
            FormsAuthentication.SetAuthCookie(account.LoginName, true);
            //CreateGymnast(new Gymnast { MemberId = member.Id, Name = member.Text });

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Member was registered successfully");
        }
        catch (Exception ex)
        {
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

        string root = HttpContext.Current.Server.MapPath("~/App_Data");
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
                        Log.Add(LogTypes.New, member.Id, file.LocalFileName);
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(UmbracoCustom.GetParameterValue(UmbracoType.Media) + image.Id);
                        Log.Add(LogTypes.New, image.Id, directoryInfo.FullName);
                        UmbracoFile imageUpload = UmbracoFile.Save(stream, string.Format(@"{0}\{1}", directoryInfo.FullName, info.Name));
                        imageUpload.Resize(100, "thumb");
                        image.Value = string.Format("/media/{0}/{1}", image.Id, info.Name);
                        member.Save();
                    }
                    catch (Exception ex)
                    {
                        Log.Add(LogTypes.New, -1, ex.Message);
                    }


                }
                return Request.CreateResponse(HttpStatusCode.OK);
            });

        return task;

    }

    #endregion

    #region Workout

    [HttpPost]
    public Workout InsertWorkout(Workout workout)
    {
        HttpResponseMessage response = new HttpResponseMessage();

        try
        {
            Member member = Member.GetCurrentMember();
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
            document.getProperty("state").Value = workout.State;
            document.Save();
            workout.Id = document.Id;

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return workout;
    }

    [HttpGet]
    public IEnumerable<Workout> SelectWorkoutByMember()
    {
        Member member = Member.GetCurrentMember();
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Document document = documents.Single(d => d.Text == member.Text);
        return document.Children.Select(child => new Workout
        {
            Id = child.Id,
            ParentId = child.ParentId,
            Name = child.Text,
            DateScheduled = (child.getProperty("dateScheduled").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateScheduled").Value) : (DateTime?)null),
            DateCompleted = (child.getProperty("dateCompleted").Value.ToString() != "" ? Convert.ToDateTime(child.getProperty("dateCompleted").Value) : (DateTime?)null),
            Description = child.getProperty("description").Value.ToString(),
            //RateId = (child.getProperty("rate").Value.ToString() != "" ? Convert.ToInt32(child.getProperty("rate").Value) : (int?)null),
            //Rate = UmbracoCustom.PropertyValue(UmbracoType.Rate, child.getProperty("rate")),
            Note = child.getProperty("note").Value.ToString()
        }).ToList();
    }

    [HttpPost]
    public HttpResponseMessage UpdateWorkout(Workout workout)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();

            Document document = new Document(workout.Id);
            document.getProperty("dateScheduled").Value = workout.DateScheduled;
            document.getProperty("dateCompleted").Value = workout.DateCompleted;
            document.getProperty("description").Value = workout.Description;
            document.getProperty("state").Value = workout.State;
            //document.getProperty("rate").Value = workout.RateId;
            document.getProperty("note").Value = workout.Note;
            document.Save();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully edited");
        }
        catch (Exception ex)
        {
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
            Member member = Member.GetCurrentMember();

            Document document = new Document(id);
            document.delete();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Workout successfully deleted");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
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
    public List<Exercise> GetExerciseByCategory(int categoryid, int trainerid)
    {
        List<Exercise> exercises = new List<Exercise>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectExerciseByCategory",
            new SqlParameter { ParameterName = "@CategoryId", Value = categoryid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@TrainerId", Value = trainerid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
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
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
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
            }
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Routine successfully created");
            transaction.Commit();
        }
        catch (SqlException ex)
        {
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
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRoutineByWorkout", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
                Reps = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(6)),
                Sets = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(7)),
                Resistance = reader.IsDBNull(8) ? (decimal?)null : Convert.ToDecimal(reader.GetValue(8)),
                UnitId = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader.GetValue(9)),
                Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(9)),
                Note = reader.GetValue(10).ToString(),
                StateId = reader.IsDBNull(11) ? (int?)null : Convert.ToInt32(reader.GetValue(11)),
                State = UmbracoCustom.PropertyValue(UmbracoType.State, reader.GetValue(11)),
                SortOrder = Convert.ToInt32(reader.GetValue(12))
            });
        }
        return routines;
    }

    [HttpGet]
    public IEnumerable<RoutineViewModel> GetRoutineStories(int id)
    {
        Member member = Member.GetCurrentMember();

        List<RoutineViewModel> routineViewModels = new List<RoutineViewModel>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectRoutineByWorkout", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
                    SortOrder = Convert.ToInt32(reader.GetValue(12))
                },
                Stories = GetStory(Convert.ToInt32(reader.GetValue(5)))
            });
        }
        return routineViewModels;
    }

    [HttpGet]
    public IEnumerable<Story> GetStory(int id)
    {
        Member member = Member.GetCurrentMember();

        List<Story> exerciseStories = new List<Story>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectStory", new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
                Note = reader.GetValue(6).ToString()
            });
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
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectSuperSet", new SqlParameter { ParameterName = "@WorkoutId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            superSets.Add(new SuperSet
            {
                Id = Convert.ToInt32(reader.GetValue(0).ToString()),
                Reps = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(1)),
                Sets = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                ResistanceId = Convert.ToInt32(reader.GetValue(3)),
                Resistance = UmbracoCustom.PropertyValue(UmbracoType.Resistance, reader.GetValue(3)),
                UnitId = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(4)),
                Note = reader.GetValue(10).ToString()
            });
        }
        return superSets;
    }

    public SuperSet GetSuperSetById(int id)
    {
        SuperSet superSet= new SuperSet();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectSuperSetById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            superSet= new SuperSet
            {
                Id = Convert.ToInt32(reader.GetValue(0).ToString()),
                Reps = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(1)),
                Sets = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(2)),
                ResistanceId = Convert.ToInt32(reader.GetValue(3)),
                Resistance = UmbracoCustom.PropertyValue(UmbracoType.Resistance, reader.GetValue(3)),
                UnitId = reader.IsDBNull(9) ? (int?)null : Convert.ToInt32(reader.GetValue(4)),
                Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(4)),
                Note = reader.GetValue(10).ToString()
            };
        }
        return superSet;
    }

    [HttpPost]
    public HttpResponseMessage TestInsert(RoutineViewModel routineViewModel)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("It Works");
        return response;
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
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectDeviceByPlatform",
            new SqlParameter { ParameterName = "@PlatformId", Value = platformid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }

            );
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
        return devices;
    }

    [HttpGet]
    public PushNotification GetNotificacion(string token)
    {
        Member member = Member.GetCurrentMember();
        List<PushNotification> notifications = new List<PushNotification>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectNotificationByMember", new SqlParameter { ParameterName = "@MemberId", Value = member.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
        return notifications.SingleOrDefault(n => n.Token == token);
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
                      new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                      new SqlParameter { ParameterName = "@DeviceId", Value = notification.DeviceId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                      new SqlParameter { ParameterName = "@IsActive", Value = notification.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                      );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Notification updated successfully");
        }
        catch (Exception ex)
        {
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
               new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
               new SqlParameter { ParameterName = "@DeviceId", Value = notification.DeviceId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Notification was registered successfully");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public HttpResponseMessage SendPush(PushMessage message)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            Member member = Member.GetCurrentMember();
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            PushNotification notification = GetNotificacion(message.Token);
            int userType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "client").Id;
            int objectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;

            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
               new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
               new SqlParameter { ParameterName = "@NotificationId", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = member.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = userType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = message.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = objectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Message", Value = message.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
               );

            pushService.QueueNotification(new AppleNotification().ForDeviceToken(message.Token).WithAlert(message.Message).WithBadge(7));
            pushService.StopAllServices();

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Message sent successfully");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    #endregion

    #region MessageBoard

    [HttpPost]
    public int InsertTopic(Topic topic)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        int id = 0;
        try
        {
            SetTopicUser(topic);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = topic.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTopic",
            parameter,
            new SqlParameter { ParameterName = "@Name", Value = topic.Name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@Description", Value = topic.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
            new SqlParameter { ParameterName = "@UserId", Value = topic.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@UserType", Value = topic.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
            id = Convert.ToInt32(parameter.Value);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Topic successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return id;
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
    public HttpResponseMessage InsertPost(Post post)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            SetPostUser(post);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPost",
                new SqlParameter { ParameterName = "@Id", Value = post.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@Message", Value = post.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
                new SqlParameter { ParameterName = "@TopicId", Value = post.TopicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@UserId", Value = post.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "@UserType", Value = post.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Post successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

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
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTopic");
        while (reader.Read())
        {
            Topic topic = new Topic
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetValue(1).ToString(),
                Description = reader.GetValue(2).ToString(),
                UserId = Convert.ToInt32(reader.GetValue(3)),
                UserType = Convert.ToInt32(reader.GetValue(4)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(5).ToString()),
                UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString())

            };
            GetTopicUser(topic);
            topics.Add(topic);
        }
        paging.Records = topics.Count();
        paging.TotalPages = (int)Math.Ceiling((float)topics.Count() / paging.Pagesize);
        return new TopicViewModel
            {
                Topics = topics.Skip((paging.CurrentPage - 1)*paging.Pagesize).Take(paging.Pagesize).ToList(),
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
            topic.User = new User(topic.UserId).Name;
            topic.IsOwner = currentUser == topic.UserId;
        }
        else
        {
            topic.User = new Member(topic.UserId).Text;
            topic.IsOwner = currentUser == topic.UserId;
        }
    }

    [HttpGet]
    public List<Post> SelectPost(int id)
    {
        List<Post> posts = new List<Post>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPost", new SqlParameter { ParameterName = "@TopicId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
        return posts;
    }

    private void GetPostUser(Post post)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, post.UserType).ToLower();

        if (type == "trainer")
        {
            post.User = new User(post.UserId).Name;
            post.IsOwner = currentUser == post.UserId;
        }
        else
        {
            post.User = new Member(post.UserId).Text;
            post.IsOwner = currentUser == post.UserId;
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
            new SqlParameter { ParameterName = "@Name", Value = topic.Name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@Description", Value = topic.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Topic successfully updated");
        }
        catch (Exception ex)
        {
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
            new SqlParameter { ParameterName = "@Message", Value = post.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@ReportAbuse", Value = post.ReportAbuse, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Post successfully updated");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
    }

    [HttpPost]
    public int InsertChat(Chat chat)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        int id = 0;
        try
        {
            SetChatUser(chat);
            SqlParameter parameter = new SqlParameter { ParameterName = "@Id", Value = chat.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertChat",
               parameter,
               new SqlParameter { ParameterName = "@GymnastId", Value = chat.GymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Subject", Value = chat.Subject, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
               new SqlParameter { ParameterName = "@UserId", Value = chat.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = chat.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );
            id = Convert.ToInt32(parameter.Value);
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Chat successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return id;
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
    public int InsertTalk(Talk talk)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        int id = 0;
        try
        {
            SetTalkUser(talk);
            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTalk",
               new SqlParameter { ParameterName = "@Id", Value = talk.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ChatId", Value = talk.ChatId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Message", Value = talk.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 },
               new SqlParameter { ParameterName = "@UserId", Value = talk.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = talk.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
               );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Talk successfully created");
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return id;
    }

    private void SetTalkUser(Talk talk)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            talk.UserId = user.Id;
            talk.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            talk.UserId = member.Id;
            talk.UserType =
                UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(
                    u => u.Value.ToLower() == "client").Id;
        }
    }

    [HttpGet]
    public ChatViewModel SelectChat([FromUri]Paging paging)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectChat",
          new SqlParameter { ParameterName = "@GymnastId", Value = paging.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        );
        List<Chat> chats = new List<Chat>();
        while (reader.Read())
        {
            Chat chat = new Chat
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    GymnastId = Convert.ToInt32(reader.GetValue(1)),
                    Subject = reader.GetValue(2).ToString(),
                    UserId = Convert.ToInt32(reader.GetValue(3)),
                    UserType = Convert.ToInt32(reader.GetValue(4)),
                    CreatedDate = Convert.ToDateTime(reader.GetValue(5))
                };
            GetChatUser(chat);
            chats.Add(chat);
        }
        paging.Records = chats.Count();
        paging.TotalPages = (int)Math.Ceiling((float)chats.Count() / paging.Pagesize);
        return new ChatViewModel
            {
                Chats = chats.Skip((paging.CurrentPage - 1)*paging.Pagesize).Take(paging.Pagesize).ToList(),
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
            chat.User = new User(chat.UserId).Name;
            chat.IsOwner = currentUser == chat.UserId;
        }
        else
        {
            chat.User = new Member(chat.UserId).Text;
            chat.IsOwner = currentUser == chat.UserId;
        }
    }

    [HttpGet]
    public List<Talk> SelectTalk(int id)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTalk",
          new SqlParameter { ParameterName = "@ChatId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        );
        List<Talk> talks = new List<Talk>();
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
        return talks;
    }

    private void GetTalkUser(Talk talk)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();

        int currentUser = user != null ? user.Id : member.Id;
        string type = UmbracoCustom.PropertyValue(UmbracoType.UserType, talk.UserType).ToLower();

        if (type == "trainer")
        {
            talk.User = new User(talk.UserId).Name;
            talk.IsOwner = currentUser == talk.UserId;
        }
        else
        {
            talk.User = new Member(talk.UserId).Text;
            talk.IsOwner = currentUser == talk.UserId;
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
            new SqlParameter { ParameterName = "@Subject", Value = chat.Subject, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Chat successfully updated");
        }
        catch (Exception ex)
        {
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
            new SqlParameter { ParameterName = "@Message", Value = talk.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
            new SqlParameter { ParameterName = "@ReportAbuse", Value = talk.ReportAbuse, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
            );
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("Talk successfully updated");
        }
        catch (Exception ex)
        {
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
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = new StringContent(ex.Message);
            throw new HttpResponseException(response);
        }
        return response;
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

        string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);
        CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(root);

        Request.Content.LoadIntoBufferAsync().Wait();
        var task = Request.Content.ReadAsMultipartAsync(provider).
            ContinueWith<HttpResponseMessage>(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertMeasurement",
                  new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@GymnastId", Value = Convert.ToInt32(provider.FormData.GetValues("GymnastId")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                  new SqlParameter { ParameterName = "@Weight", Value = Convert.ToDecimal(provider.FormData.GetValues("Weight")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Neck", Value = Convert.ToDecimal(provider.FormData.GetValues("Neck")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Shoulders", Value = Convert.ToDecimal(provider.FormData.GetValues("Shoulders")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@RightArm", Value = Convert.ToDecimal(provider.FormData.GetValues("RightArm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@LeftArm", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftArm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Chest", Value = Convert.ToDecimal(provider.FormData.GetValues("Chest")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@BellyButton", Value = Convert.ToDecimal(provider.FormData.GetValues("BellyButton")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Hips", Value = Convert.ToDecimal(provider.FormData.GetValues("Hips")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@RightThigh", Value = Convert.ToDecimal(provider.FormData.GetValues("RightThigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@LeftThigh", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftThigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@RightCalf", Value = Convert.ToDecimal(provider.FormData.GetValues("RightCalf")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@LeftCalf", Value = Convert.ToDecimal(provider.FormData.GetValues("LeftCalf")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Arm", Value = Convert.ToDecimal(provider.FormData.GetValues("Arm")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Waist", Value = Convert.ToDecimal(provider.FormData.GetValues("Waist")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Thigh", Value = Convert.ToDecimal(provider.FormData.GetValues("Thigh")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal },
                  new SqlParameter { ParameterName = "@Back", Value = Convert.ToDecimal(provider.FormData.GetValues("Back")[0]), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal }
                  );

                // Show all the key-value pairs.
                //Measurement measurement = new Measurement
                //    {
                //        GymnastId = Convert.ToInt32(provider.FormData.GetValues(0)[0]),
                //        Weight = Convert.ToDecimal(provider.FormData.GetValues(1)[0]),
                //        Neck = Convert.ToDecimal(provider.FormData.GetValues(2)[0]),
                //        Shoulders = Convert.ToDecimal(provider.FormData.GetValues(3)[0]),
                //        RightArm = Convert.ToDecimal(provider.FormData.GetValues(4)[0]),
                //        LeftArm = Convert.ToDecimal(provider.FormData.GetValues(5)[0]),
                //        Chest = Convert.ToDecimal(provider.FormData.GetValues(6)[0]),
                //        BellyButton = Convert.ToDecimal(provider.FormData.GetValues(7)[0]),
                //        Hips = Convert.ToDecimal(provider.FormData.GetValues(8)[0]),
                //        RightThigh = Convert.ToDecimal(provider.FormData.GetValues(9)[0]),
                //        LeftThigh = Convert.ToDecimal(provider.FormData.GetValues(10)[0]),
                //        RightCalf = Convert.ToDecimal(provider.FormData.GetValues(11)[0]),
                //        LeftCalf = Convert.ToDecimal(provider.FormData.GetValues(12)[0]),
                //        Arm = Convert.ToDecimal(provider.FormData.GetValues(13)[0]),
                //        Waist = Convert.ToDecimal(provider.FormData.GetValues(14)[0]),
                //        Thigh = Convert.ToDecimal(provider.FormData.GetValues(15)[0]),
                //        Back = Convert.ToDecimal(provider.FormData.GetValues(16)[0])
                //    };

                string imagePath = UmbracoCustom.GetParameterValue(UmbracoType.Photo) + provider.FormData.GetValues("GymnastId")[0];
                string datePath = Path.Combine(imagePath, DateTime.Now.ToString("MMddyyyy"));
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

                return Request.CreateResponse(HttpStatusCode.OK);
            });

        return task;
    }

    [HttpGet]
    public Measurement SelectMeasurementByGymnast(int gymnastId)
    {
        Measurement measurement = new Measurement();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastId", new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
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
                CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString())
            };
        }
        return measurement;
    }

    [HttpGet]
    public List<Measurement> SelectMeasurementByGymnastByDate(int gymnastId, DateTime fromDate, DateTime toDate)
    {
        List<Measurement> measurements = new List<Measurement>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastIdByDate",
            new SqlParameter { ParameterName = "@GymnastId", Value = gymnastId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@FromDate", Value = fromDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
            new SqlParameter { ParameterName = "@ToDate", Value = toDate, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime }
            );
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
                CreatedDate = Convert.ToDateTime(reader.GetValue(18).ToString())
            });
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

    #endregion
}
