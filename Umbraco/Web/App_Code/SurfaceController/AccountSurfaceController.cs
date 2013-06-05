using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PushSharp;
using Umbraco.Web.Models;
using umbraco.BusinessLogic;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;
using Property = umbraco.cms.businesslogic.property.Property;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

/// <summary>
/// Summary description for AccountSurfaceController
/// </summary>
public class AccountSurfaceController : Umbraco.Web.Mvc.SurfaceController
{
    public AccountSurfaceController()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    [HttpPost]
    public ActionResult Login(Account account)
    {
        //Member member = Member.GetMemberFromLoginAndEncodedPassword(account.LoginName, UmbracoCustom.EncodePassword(account.Password));
        //Property image = member.getProperty("photo");
        //Property isActive = member.getProperty("isActive");
        //if (Membership.ValidateUser(account.LoginName, account.Password) && isActive.Value.ToString() == "1")
        //{
        //    FormsAuthentication.SetAuthCookie(account.LoginName, true);
        //    return Redirect("/home");
        //}
        return Redirect("/login");
    }

    [HttpGet]
    public ActionResult LogOff()
    {
        //if (Request.Cookies["yourAuthCookie"] != null)
        //{
        //    HttpCookie cookie = new HttpCookie("yourAuthCookie");
        //    cookie.Expires = DateTime.Now.AddYears(-20);
        //    Response.Cookies.Add(cookie);
        //}
        return Redirect(Request.UrlReferrer.ToString());
    }

    [HttpPost]
    public ActionResult Register(Account account, HttpPostedFileBase photo)
    {

        //MembershipUser user = Membership.CreateUser(account.LoginName, account.Password, account.Email);
        ////MemberGroup memberGroup = MemberGroup.MakeNew("Users", new User(account.LoginName));
        //Roles.AddUserToRole(account.LoginName, "Users");
        //Member member = Member.GetMemberFromLoginAndEncodedPassword(account.LoginName, UmbracoCustom.EncodePassword(account.Password));
        //member.Text = account.Name;
        //member.getProperty("isActive").Value = "1";
        //member.getProperty("city").Value = account.City;
        //member.getProperty("state").Value = account.State;
        //member.getProperty("country").Value = account.Country;
        //member.getProperty("zipCode").Value = account.ZipCode;
        //member.getProperty("address").Value = account.Address;
        //member.getProperty("phone").Value = account.Phone;
        //Property image = member.getProperty("photo");
        //string folderPath = string.Format(@"{0}\{1}", Server.MapPath("~/media"), image.Id);
        //string imagePath = string.Format(@"{0}\{1}", folderPath, photo.FileName);
        //Directory.CreateDirectory(folderPath); 
        //photo.SaveAs(imagePath);
        //image.Value = imagePath;
        //member.Save();
        //FormsAuthentication.SetAuthCookie(account.LoginName, true);
        
        return Content("thank you");
    }

    //[HttpGet]
    //public ActionResult Information()
    //{
    //    Member member = Member.GetCurrentMember();
    //    Account account = new Account
    //        {
    //            Address = member.getProperty("address").Value.ToString(),
    //            City = member.getProperty("city").Value.ToString(),
    //            Country = member.getProperty("country").Value.ToString(),
    //            Email = member.Email,
    //            LoginName = member.LoginName,
    //            Name = member.Text,
    //            Phone = member.getProperty("phone").Value.ToString(),
    //            State = member.getProperty("state").Value.ToString(),
    //            ZipCode = member.getProperty("zipCode").Value.ToString(),
    //            Image = member.getProperty("photo").Value.ToString()
    //        };
    //    //RenderModel model = new RenderModel(account);

    //    return View(account);
    //}

    [HttpGet]
    public ActionResult Test() //http://localhost:21773/umbraco/surface/accountsurface/test
    {
        //IEnumerable<MediaType> mediaTypes = MediaType.GetAllAsList();
        //MediaType image = mediaTypes.SingleOrDefault(mt => mt.Text == "Image");
        //Media media = Media.MakeNew("female.png", image, new User("admin"), 1076);
        //media.Save();
        //var c = mediaTypes.Count();
        //Member member = Member.GetMemberFromLoginAndEncodedPassword("lquispej", EncodePassword("user"));
        //Property image = member.getProperty("photo");

        //DataTypeDefinition dataTypeDefinition = DataTypeDefinition.GetDataTypeDefinition(1098);
        //SortedList data = PreValues.GetPreValues(dataTypeDefinition.Id);
        //XDocument document = new XDocument();
        //XElement element = new XElement("sports");
        //foreach (DictionaryEntry dictionaryEntry in data)
        //{
        //    element.Add(new XElement("sport", new XElement("Id", ((PreValue)dictionaryEntry.Value).Id), new XElement("Name", ((PreValue)dictionaryEntry.Value).Value)));
        //}
        //document.Add(element);
        //document.Save(Server.MapPath("~/App_Data") + @"\sport.xml");

        //DataTypeDefinition dataTypeDefinition = DataTypeDefinition.GetDataTypeDefinition(1100);
        //SortedList data = PreValues.GetPreValues(dataTypeDefinition.Id);
        //XDocument document = new XDocument();
        //XElement element = new XElement("leagues");
        //foreach (DictionaryEntry dictionaryEntry in data)
        //{
        //    var values = (PreValue) dictionaryEntry.Value;
        //    var id = values.Id;
        //    var v = values.Value.Split(',');
        //    var sportId = v[0];
        //    var name = v[1];
        //    element.Add(new XElement("league", new XElement("Id", id), new XElement("Name", name), new XElement("SportId", sportId)));
        //}
        //document.Add(element);
        //document.Save(Server.MapPath("~/App_Data") + @"\league.xml");

        //Document[] documents = Document.GetChildrenForTree(1161);
        //Document document = documents.SingleOrDefault(d => d.Text == "Saulo Tsuchida Cruz");
        //var member = document.getProperty("member");

        //Document[] d1 = Document.GetChildrenForTree(1163);
        //Document d2 = d1.FirstOrDefault();
        //var date1 = document.getProperty("dateScheduled");
        //var date2 = document.getProperty("dateCompleted");
        //var date3 = document.getProperty("description");

        //MembershipUser user = Membership.GetUser();
        //Member umbracoMember = Member.GetCurrentMember();

        //User user = new User("admin");
        //DocumentType documentType = DocumentType.GetByAlias("Team");
        //Document document = Document.MakeNew("Alianza Lima", documentType, new User("admin"),  1131);
        //document.getProperty("city").Value = "Lima";
        //document.getProperty("division").Value = "Primera Division";

        //Document document = new Document(1195);
        //Property property = document.getProperty("bodyWeight");
        //XDocument xmlDocument = XDocument.Parse((string) property.Value);
        //var list = (from x in xmlDocument.Descendants("item")
        //                select new
        //                    {
        //                        id = int.Parse(x.Attribute("id").Value),
        //                        sortOrder = int.Parse(x.Attribute("sortOrder").Value),
        //                        weight = decimal.Parse(x.Element("weight").Value),
        //                        date = DateTime.Parse(x.Element("date").Value)
        //                    }).ToList();


        //var users = umbraco.BusinessLogic.User.getAll();

        //XDocument umbracoData = XDocument.Load(Server.MapPath("~/App_Data") + @"\ResourceDataType.xml");
        //var id = (from c in umbracoData.Descendants("Data")
        //                          where c.Value.Equals("Category")
        //                          select c.Attribute("key").Value).FirstOrDefault();

        //var members = Member.GetAllAsList();
        //var groups = Roles.GetRolesForUser(members.FirstOrDefault().LoginName).Contains("Users");

        //var path = FormsAuthentication.FormsCookiePath;

        //PropertyInfo[] p = typeof (PushNotification).GetProperties();
        //foreach (PropertyInfo propertyInfo in p)
        //{
        //    var name = propertyInfo.Name;
        //    var type = propertyInfo.PropertyType.Name; // t will be System.String    
        //}



        //PreValue value = PreValue.MakeNew(1085, "Category 9");

        //value.DataTypeId = 1085;
        //value.SortOrder = 9;
        //value.Save();

        Document document = new Document(-1);
        var childrens = document.Children;

        bool r = StateHelper.Cookies.HasCookies;
        StateHelper.Cookies.Cookie cookie = StateHelper.Cookies.UserContext;

        //User user = new User("fsotog", "trainer"); //umbraco.BusinessLogic.User.GetCurrent();
        bool result = umbraco.BusinessLogic.User.validateCredentials("fsotog", UmbracoCustom.EncodePassword("trainer"));

        User[] users = umbraco.BusinessLogic.User.getAllByLoginName("fsotog");
        bool result2 = Membership.ValidateUser("fsotog", UmbracoCustom.EncodePassword("trainer"));

        User user = umbraco.BusinessLogic.User.GetUser(1);

        IContentType contentType = Services.ContentTypeService.GetContentType(1072);
        var contentType2 = contentType.CompositionPropertyTypes;
        IContent content = Services.ContentService.GetById(1159); //new Content("Valeria Agurto Castilla", 1158, new ContentType(1072));
        string contentName = content.Name;
        PropertyCollection properties = content.Properties;
        IEnumerable<IContent> childs = Services.ContentService.GetChildren(1158);


        return Content("Hola mundo xD");
    }

    //[HttpGet]
    //public JsonResult Leagues(int id)
    //{
    //    XDocument document = XDocument.Load(Server.MapPath("~/App_Data") + @"\league.xml");
    //    var leagues = (from s in document.Descendants("league")
    //                            select new League
    //                                {
    //                                    Id = Convert.ToInt32(s.Element("Id").Value),
    //                                    Name = s.Element("Name").Value,
    //                                    SportId = Convert.ToInt32(s.Element("SportId").Value)
    //                                }
    //                           );
    //    return Json(leagues.Where(l=>l.SportId == id).ToList(), JsonRequestBehavior.AllowGet);
    //}

    #region Helper



    #endregion
}