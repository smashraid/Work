using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.ApplicationBlocks.Data;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using umbraco.BusinessLogic;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.presentation.nodeFactory;
using Property = umbraco.cms.businesslogic.property.Property;


public class HelperSurfaceController : Umbraco.Web.Mvc.SurfaceController
{
    private PushBroker pushService;

    public HelperSurfaceController()
    {
        string root = System.Web.HttpContext.Current.Server.MapPath("~/certificate");
        string certName = "metafitness.p12";
        byte[] cert = System.IO.File.ReadAllBytes(Path.Combine(root, certName));

        //pushService = new PushBroker();

        //ApplePushChannelSettings settings = new ApplePushChannelSettings(false, cert, "ilovebbq");
        //pushService.RegisterAppleService(settings);
    }

    [HttpGet]
    public void GetNodes()
    {
        XmlTree xTree = new XmlTree();
        ITreeService treeParams = new TreeService(1099, "content", false, false, TreeDialogModes.none, null);
        TreeDefinition tree = TreeDefinitionCollection.Instance.FindTree("content");
        BaseTree instance = tree.CreateInstance();
        instance.SetTreeParameters((ITreeService)treeParams);
        instance.Render(ref xTree);
        Response.ContentType = "application/json";
        Response.Write(((object)xTree).ToString());
    }

    [HttpGet]
    public JsonResult Rss(string url)
    {
        //localhost:1909/umbraco/surface/helpersurface/rss?url=http://feeds.feedburner.com/general-interest-ogj
        List<dynamic> result = new List<dynamic>();

        foreach (XElement element in XDocument.Load(url).Descendants("item"))
        {
            dynamic data = new ExpandoObject();
            foreach (XElement item in element.Descendants())
            {
                ((IDictionary<string, object>)data).Add(item.Name.LocalName, item.Value);
            }
            result.Add(data);
        }
        return Json(result, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult GetContent(int Id)
    {
        Document document = new Document(Id);
        return Json(document.getProperty("body").Value.ToString(), JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public PartialViewResult GetPartial(string name)
    {
        return PartialView(name);
    }

    [HttpPost]
    public JsonResult Research(string firstname, string lastname, string email)
    {
        SmtpClient client = new SmtpClient();
        MailMessage mailMessage = new MailMessage();
        StringBuilder formHtml = new StringBuilder();
        formHtml.Append(string.Format("<div><label>First Name: </label>{0}</div>", firstname));
        formHtml.Append(string.Format("<div><label>Last Name: </label>{0}</div>", lastname));
        formHtml.Append(string.Format("<div><label>Email: </label>{0}</div>", email));
        mailMessage.IsBodyHtml = true;
        mailMessage.From = new MailAddress("onlinesubmittals@paramount-fs.com");
        mailMessage.To.Add("onlinesubmittals@paramount-fs.com");
        mailMessage.Subject = "Research";
        mailMessage.Body = formHtml.ToString();
        client.Send(mailMessage);
        return Json("Thank you for your submission.");
    }

    [HttpPost]
    public JsonResult Contact(string firstname, string lastname, string email, string message)
    {
        SmtpClient client = new SmtpClient();
        MailMessage mailMessage = new MailMessage();
        StringBuilder formHtml = new StringBuilder();
        formHtml.Append(string.Format("<div><label>First Name: </label>{0}</div>", firstname));
        formHtml.Append(string.Format("<div><label>Last Name: </label>{0}</div>", lastname));
        formHtml.Append(string.Format("<div><label>Email: </label>{0}</div>", email));
        formHtml.Append(string.Format("<div><label>Message: </label>{0}</div>", message));
        mailMessage.IsBodyHtml = true;
        mailMessage.From = new MailAddress("onlinesubmittals@paramount-fs.com");
        mailMessage.To.Add("onlinesubmittals@paramount-fs.com");
        mailMessage.Subject = "Contact Us";
        mailMessage.Body = formHtml.ToString();
        client.Send(mailMessage);
        return Json("Thank you for your submission.");
    }

    [HttpPost]
    public JsonResult ProfessionalLogin(string usernameprofessional, string passwordprofessional)
    {

        return Json("Thank you for your submission.");
    }

    [HttpPost]
    public JsonResult ClientLogin(string usernameclient, string passwordclient)
    {

        return Json("Thank you for your submission.");
    }

    [HttpGet]
    public JsonResult GetRegions()
    {
        ArrayList list = new ArrayList();
        Document regionalOffice = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.RegionalOffice)));
        foreach (Document region in regionalOffice.Children)
        {
            list.Add(region.getProperty("code").Value.ToString());
        }

        //regionalOffice.Children.Select(r => listRegions.Add(r.getProperty("code").Value.ToString()));
        return Json(list, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult GetMarkers()
    {
        ArrayList list = new ArrayList();
        Document site = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Paramount)));
        DocumentType office = DocumentType.GetByAlias("Office");
        list.Add(new Marker
        {
            name = "Corporate Office",
            latLng = new ArrayList
                            {
                               decimal.Parse(site.getProperty("latitude").Value.ToString()),
                               decimal.Parse(site.getProperty("longitude").Value.ToString())
                            }
        });
        Document regionalOffice = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.RegionalOffice)));
        foreach (Document region in regionalOffice.Children)
        {
            foreach (Document marker in region.Children.Where(r => r.ContentType.Id == office.Id))
            {
                list.Add(new Marker
                    {
                        name = marker.Text,
                        latLng = new ArrayList
                            {
                               decimal.Parse(marker.getProperty("latitude").Value.ToString()),
                               decimal.Parse(marker.getProperty("longitude").Value.ToString())
                            }
                    });
            }
        }
        return Json(list, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public PartialViewResult GetInfo(string state)
    {
        Info info = new Info
            {
                Offices = new List<Office>(),
                Basins = new List<string>(),
                State = state
            };
        DocumentType basin = DocumentType.GetByAlias("Basin");
        DocumentType office = DocumentType.GetByAlias("Office");
        Document site = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Paramount)));
        if (site.getProperty("state").Value.ToString() == state)
        {
            info.Offices.Add(new Office(site, "Corporate Office"));
        }
        Document regionalOffice = new Document(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.RegionalOffice)));
        Document region = regionalOffice.Children.SingleOrDefault(r => r.Text == state);
        if (region != null)
        {
            foreach (Document marker in region.Children)
            {
                if (marker.ContentType.Id == basin.Id)
                {
                    info.Basins.Add(marker.Text);
                }
                else if (marker.ContentType.Id == office.Id)
                {
                    info.Offices.Add(new Office(marker));
                }

            }
        }
        return PartialView("_RegionInfo", info);
    }

    [HttpGet]
    public PartialViewResult GetStock()
    {
        //http://vikku.info/codetrash/Yahoo_Finance_Stock_Quote_API
        List<string[]> parsedData = new List<string[]>();
        string url = "http://download.finance.yahoo.com/d/quotes.csv?s=^GSPC+^IXIC+CLG13.NYM+NGG13.NYM&f=snl1c1p2&e=.csv";
        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
        webRequest.Method = WebRequestMethods.Http.Get;
        webRequest.ContentType = "application/octet-stream";
        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
        {
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(webResponse.GetResponseStream());

                string line;
                string[] row;

                while ((line = stream.ReadLine()) != null)
                {
                    row = line.Replace("\"", string.Empty).Split(',');
                    parsedData.Add(row);
                }
                //string json = stream.ReadToEnd();
                //var xxx = json.Replace("\"", string.Empty).Split(new[] { '\r', '\n' });
            }
        }
        return PartialView("_StockInfo", parsedData);
    }

    //[HttpGet]
    //public ViewResult GetMessageBoard(int id)
    //{
    //    return View("MessageBoard");
    //}

    [HttpPost]
    public JsonResult SetTopic(Topic topic)
    {
        SetTopicUser(topic);
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlParameter Id = new SqlParameter("@Id", SqlDbType.Int);
        Id.Direction = ParameterDirection.Output;
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTopic", Id,
            new SqlParameter { ParameterName = "@Name", Value = topic.Name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 },
                   //new SqlParameter { ParameterName = "@Description", Value = topic.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
                   new SqlParameter { ParameterName = "@UserId", Value = topic.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserType", Value = topic.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                   );
        topic = SelectTopicById(Convert.ToInt32(Id.Value));

        return Json(topic);
    }

    [HttpGet]
    public JsonResult GetTopic()
    {
        List<Topic> topics = SelectTopic();
        return Json(topics, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult SetPost(Post post)
    {
        SetPostUser(post);
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlParameter Id = new SqlParameter("@Id", SqlDbType.Int);
        Id.Direction = ParameterDirection.Output;
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPost", Id,
            new SqlParameter { ParameterName = "@Message", Value = post.Message, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 500 },
                   new SqlParameter { ParameterName = "@TopicId", Value = post.TopicId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserId", Value = post.UserId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserType", Value = post.UserType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                   );
        post = SelectPostById(Convert.ToInt32(Id.Value));
        
        return Json(post);
    }

    [HttpGet]
    public JsonResult GetPost(int id)
    {
        List<Post> posts = SelectPost(id);
        Topic topic = SelectTopic().Single(t => t.Id == id);

        return Json(new { posts = posts, topic = topic }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult DeleteTopic(int id)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeleteTopic", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        return Json("Success");
    }

    [HttpPost]
    public JsonResult DeletePost(int id)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "DeletePost", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        return Json("Success");
    }

    [HttpPost]
    public JsonResult SetCategory(string name)
    {
        UmbracoCustom.SetPreValue(UmbracoType.Category, name);
        return Json("Success");
    }

    [HttpPost]
    public void NotificationMessasge(int topicId, int postId)
    {
        foreach (Post post in SelectTopicNotification(topicId))
        {
            SmtpClient client = new SmtpClient();
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress("app@metafitnessatx.com");
            message.To.Add(new Member(post.UserId).Email);
            message.Subject = "MetaFitness";
            message.Body = (post.IsOwner ? "A message you created has a new response. Check your MetaFitness App now." : "A message you commented on has a new response. Check your MetaFitness App now.");
            client.Send(message);

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertEmailMessage",
               new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = (post.IsOwner ? topicId : postId), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == (post.IsOwner ? "topic" : "post")).Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Email", Value = new Member(post.UserId).Email, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
               new SqlParameter { ParameterName = "@Message", Value = (post.IsOwner ? "A message you created has a new response. Check your MetaFitness App now." : "A message you commented on has a new response. Check your MetaFitness App now."), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
               );


            foreach (PushNotification notification in SelectNotificationByMember(post.UserId))
            {
                pushService.QueueNotification(new AppleNotification().ForDeviceToken(notification.Token).WithAlert((post.IsOwner ? "A message you created has a new response." : "A message you commented on has a new response.")).WithBadge(7));
                pushService.StopAllServices();

                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
                   new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                   new SqlParameter { ParameterName = "@NotificationId", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@ObjectId", Value = (post.IsOwner ? topicId : postId), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == (post.IsOwner ? "topic" : "post")).Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@Message", Value = (post.IsOwner ? "A message you created has a new response." : "A message you commented on has a new response."), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
                   );
            }
        }
    }

    [HttpGet]
    public void NotificationTask()
    {
        foreach (Member member in Member.GetAllAsList().Where(m => m.getProperty("isActive").Value.ToString() == "1"))
        {
            SmtpClient client = new SmtpClient();
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress("app@metafitnessatx.com");
            message.To.Add(member.Email);
            message.Subject = "MetaFitness";
            message.Body = "It's time. Let's check in on those results. Submit yours now through your MetaFitness App."; 
            client.Send(message);

            string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertEmailMessage",
               new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = Convert.ToInt32(member.getProperty("gymnast").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == "gymnast").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Email", Value = member.Email, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
               new SqlParameter { ParameterName = "@Message", Value = "It's time. Let's check in on those results. Submit yours now through your MetaFitness App.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
               );

            foreach (PushNotification notification in SelectNotificationByMember(member.Id))
            {
                pushService.QueueNotification(new AppleNotification().ForDeviceToken(notification.Token).WithAlert("It's time. Let's check in on those results. Submit yours now.").WithBadge(7));
                pushService.StopAllServices();

                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
                   new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                   new SqlParameter { ParameterName = "@NotificationId", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@ObjectId", Value = Convert.ToInt32(member.getProperty("gymnast").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == "gymnast").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@Message", Value = "It's time. Let's check in on those results. Submit yours now.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
                   );
            }
        }
    }

    #region Data Access

    public List<Topic> SelectTopic()
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
                //Description = reader.GetValue(2).ToString(),
                UserId = Convert.ToInt32(reader.GetValue(3)),
                UserType = Convert.ToInt32(reader.GetValue(4)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(5).ToString()),
                UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString())
            };
            GetTopicUser(topic);
            topics.Add(topic);
        }
        return topics;
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
                CreatedDate = Convert.ToDateTime(reader.GetValue(5).ToString()),
                UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString())
            };
            GetPostUser(post);
            posts.Add(post);
        }
        return posts;
    }

    public List<Post> SelectTopicNotification(int id)
    {
        List<Post> posts = new List<Post>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTopicNotification", new SqlParameter { ParameterName = "@TopicId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            Post post = new Post
            {
                UserId = Convert.ToInt32(reader.GetValue(0)),
                IsOwner = Convert.ToBoolean(reader.GetValue(1))
            };
            posts.Add(post);
        }
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

    public Topic SelectTopicById(int id)
    {
        Topic topic = new Topic();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTopicById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            topic = new Topic
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetValue(1).ToString(),
                //Description = reader.GetValue(2).ToString(),
                UserId = Convert.ToInt32(reader.GetValue(3)),
                UserType = Convert.ToInt32(reader.GetValue(4)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(5).ToString()),
                UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString())
            };
            GetTopicUser(topic);
        }
        return topic;
    }

    public Post SelectPostById(int id)
    {
        Post post = new Post();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPostById", new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            post = new Post
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Message = reader.GetValue(1).ToString(),
                TopicId = Convert.ToInt32(reader.GetValue(2)),
                UserId = Convert.ToInt32(reader.GetValue(3)),
                UserType = Convert.ToInt32(reader.GetValue(4)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(5).ToString()),
                UpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(6).ToString())
            };
            GetPostUser(post);
        }
        return post;
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

    private void SetPostUser(Post post)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        Member member = Member.GetCurrentMember();
        if (user != null)
        {
            post.UserId = user.Id;
            post.UserType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;
        }
        else
        {
            post.UserId = member.Id;
            post.UserType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "client").Id;
        }
    }



    private List<PushNotification> SelectNotificationByMember(int id)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        List<PushNotification> notifications = new List<PushNotification>();
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectNotificationByMember",
                                                       new SqlParameter
                                                           {
                                                               ParameterName = "@MemberId",
                                                               Value = id,
                                                               Direction = ParameterDirection.Input,
                                                               SqlDbType = SqlDbType.Int
                                                           });
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
        return notifications;
    }

    #endregion

}

public class Marker
{
    public ArrayList latLng { get; set; }
    public string name { get; set; }
}

public class Info
{
    public string State { get; set; }
    public List<Office> Offices { get; set; }
    public List<string> Basins { get; set; }
}

public class Office
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Office(Document document)
    {
        Name = document.Text;
        Address = UmbracoCustom.GetTextAreaFormat(document.getProperty("address").Value.ToString());
        City = document.getProperty("city").Value.ToString();
        Fax = document.getProperty("fax").Value.ToString();
        Phone = document.getProperty("phone").Value.ToString();
        State = document.getProperty("state").Value.ToString();
        ZipCode = document.getProperty("zipCode").Value.ToString();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Office(Document document, string name)
    {
        Name = name;
        Address = UmbracoCustom.GetTextAreaFormat(document.getProperty("address").Value.ToString());
        City = document.getProperty("city").Value.ToString();
        Fax = document.getProperty("fax").Value.ToString();
        Phone = document.getProperty("phone").Value.ToString();
        State = document.getProperty("state").Value.ToString();
        ZipCode = document.getProperty("zipCode").Value.ToString();
    }
}

public class Nav
{
    public int Level { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}