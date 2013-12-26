using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Microsoft.ApplicationBlocks.Data;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using umbraco;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using DataTypeDefinition = umbraco.cms.businesslogic.datatype.DataTypeDefinition;
using Property = umbraco.cms.businesslogic.property.Property;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;


/// <summary>
/// Summary description for UmbracoCustom
/// </summary>
public class UmbracoCustom : System.Web.UI.Page
{
    public UmbracoCustom()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    /// <summary>
    /// Encode string in Hash format
    /// </summary>
    /// <param name="password">password</param>
    /// <returns>Encoded password</returns>
    public static string EncodePassword(string password)
    {
        HMACSHA1 hash = new HMACSHA1();
        hash.Key = Encoding.Unicode.GetBytes(password);

        string encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
        return encodedPassword;
    }

    /// <summary>
    /// Get a List of PreValues by Id
    /// </summary>
    /// <param name="id">Id of the PreValue</param>
    /// <returns>List of PreValues</returns>
    public static IEnumerable<PreValue> DataTypeValue(int id)
    {
        DataTypeDefinition dataTypeDefinition = DataTypeDefinition.GetDataTypeDefinition(id);
        SortedList data = PreValues.GetPreValues(dataTypeDefinition.Id);
        return data.Values.Cast<PreValue>();
    }

    /// <summary>
    /// Get a List of PreValues by Id
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <returns>List of PreValues</returns>
    public static IEnumerable<PreValue> DataTypeValue(UmbracoType parameter)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        DataTypeDefinition dataTypeDefinition = DataTypeDefinition.GetDataTypeDefinition(id);
        SortedList data = PreValues.GetPreValues(dataTypeDefinition.Id);
        return data.Values.Cast<PreValue>();
    }

    /// <summary>
    /// Add new value to a existing PreValue
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a string</param>
    public static void SetPreValue(UmbracoType parameter, string value)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        IEnumerable<PreValue> preValues = DataTypeValue(id);
        PreValue preValue = PreValue.MakeNew(id, value);
        preValue.DataTypeId = id;
        preValue.SortOrder = preValues.Max(pv => pv.SortOrder) + 1;
        preValue.Save();
    }

    /// <summary>
    /// Return a value of a PreValue by value
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a string</param>
    /// <returns>Returns the value of the PreValue if exists, if not returns an empty string</returns>
    public static string PropertyValue(UmbracoType parameter, string value)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        IEnumerable<PreValue> data = DataTypeValue(id);
        PreValue preValue = data.SingleOrDefault(dt => dt.Value.ToLower() == value.ToLower());
        string result = (preValue != null) ? preValue.Value : string.Empty;

        return result;
    }

    /// <summary>
    /// Return the id of a PreValue by value
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a string</param>
    /// <returns>Returns the value of the PreValue if exists, if not returns an empty string</returns>
    public static int PropertyValueId(UmbracoType parameter, string value)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        IEnumerable<PreValue> data = DataTypeValue(id);
        PreValue preValue = data.SingleOrDefault(dt => dt.Value.ToLower() == value.ToLower());
        int result = (preValue != null) ? preValue.Id : 0;

        return result;
    }

    /// <summary>
    /// Return a value of a PreValue by value
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="preValueId">id of the PreValue</param>
    /// <returns>Returns the value of the PreValue if exists, if not returns an empty string</returns>
    public static string PropertyValue(UmbracoType parameter, int preValueId)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        IEnumerable<PreValue> data = DataTypeValue(id);
        PreValue preValue = data.SingleOrDefault(dt => dt.Id == preValueId);
        string result = (preValue != null) ? preValue.Value : string.Empty;

        return result;
    }

    /// <summary>
    /// Return a value of a PreValue by Property
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a Property</param>
    /// <returns>Returns the value of the Property if exists, if not returns an empty string</returns>
    public static string PropertyValue(UmbracoType parameter, Property value)
    {
        string result = string.Empty;
        if (value.Value.ToString() != "")
        {
            int id = Convert.ToInt32(GetParameterValue(parameter));
            IEnumerable<PreValue> data = DataTypeValue(id);
            PreValue preValue = data.SingleOrDefault(dt => dt.Id == Convert.ToInt32(value.Value));
            result = (preValue != null) ? preValue.Value : string.Empty;
        }
        return result;
    }

    /// <summary>
    /// Return the Value of a PreValue by object
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a object</param>
    /// <returns>Returns the value of the Property if exists, if not returns an empty string</returns>
    public static string PropertyValue(UmbracoType parameter, object value)
    {
        string result = string.Empty;
        if (DBNull.Value != value)
        {
            int pvalue = Convert.ToInt32(value);
            int id = Convert.ToInt32(GetParameterValue(parameter));
            IEnumerable<PreValue> data = DataTypeValue(id);
            PreValue preValue = data.SingleOrDefault(dt => dt.Id == pvalue);
            result = (preValue != null) ? preValue.Value : string.Empty;
        }
        return result;
    }

    /// <summary>
    /// Return Id of a PreValue
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <param name="value">value as a string</param>
    /// <returns>Returns the id of the Property if exists, if not returns 0</returns>
    public static int PropertyId(UmbracoType parameter, string value)
    {
        int id = Convert.ToInt32(GetParameterValue(parameter));
        IEnumerable<PreValue> data = DataTypeValue(id);
        PreValue preValue = data.SingleOrDefault(dt => dt.Value.ToLower() == value.ToLower());
        return (preValue != null) ? preValue.Id : 0;
    }

    /// <summary>
    /// Get the value of a Parameter
    /// </summary>
    /// <param name="parameter">UmbracoType</param>
    /// <returns>Returns the parameter if exists, if not returns an empty string</returns>
    public static string GetParameterValue(UmbracoType parameter)
    {
        string result = string.Empty;
        switch (parameter)
        {
            case UmbracoType.Connection:
                result = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;
                break;
            case UmbracoType.TimeOut:
                result = ConfigurationManager.AppSettings["umbracoTimeOutInMinutes"];
                break;
            default:
                //string path = HttpContext.Current != null ? HttpContext.Current.Server.MapPath("~/App_Data") + @"\ResourceData.xml" : ConfigurationManager.AppSettings["ResourceData"];
                string path = ConfigurationManager.AppSettings["ResourceData"];
                XDocument umbracoData = XDocument.Load(path);
                result = (from c in umbracoData.Descendants("Data")
                          where c.Value.Equals(parameter.ToString())
                          select c.Attribute("key").Value).FirstOrDefault();
                break;
        }
        return result;
    }

    /// <summary>
    /// Get the Content of a UComponenet Grid
    /// </summary>
    /// <param name="property">IPublishedContentProperty</param>
    /// <returns>List of dymamic objects</returns>
    public static IEnumerable<dynamic> GetDataTypeGrid(IPublishedContentProperty property)
    {
        List<dynamic> result = new List<dynamic>();
        if (property != null)
        {
            foreach (XElement element in XDocument.Parse(property.Value.ToString()).Descendants("item"))
            {
                dynamic data = new ExpandoObject();
                ((IDictionary<string, object>)data).Add("id", element.Attribute("id").Value);
                ((IDictionary<string, object>)data).Add("sortorder", element.Attribute("sortOrder").Value);
                foreach (XElement item in element.Descendants())
                {
                    ((IDictionary<string, object>)data).Add(item.Name.LocalName, item.Value);
                }
                result.Add(data);
            }
        }
        return result;
    }

    /// <summary>
    /// Get the Content of a UComponenet Grid
    /// </summary>
    /// <param name="property">Property</param>
    /// <returns>List of dymamic objects</returns>
    public static IEnumerable<dynamic> GetDataTypeGrid(Property property)
    {
        List<dynamic> result = new List<dynamic>();
        if (property != null)
        {
            foreach (XElement element in XDocument.Parse(property.Value.ToString()).Descendants("item"))
            {
                dynamic data = new ExpandoObject();
                ((IDictionary<string, object>)data).Add("id", element.Attribute("id").Value);
                ((IDictionary<string, object>)data).Add("sortorder", element.Attribute("sortOrder").Value);
                foreach (XElement item in element.Descendants())
                {
                    ((IDictionary<string, object>)data).Add(item.Name.LocalName, item.Value);
                }
                result.Add(data);
            }
        }
        return result;
    }

    public static IEnumerable<dynamic> GetRss(IPublishedContentProperty property)
    {
        List<dynamic> result = new List<dynamic>();
        if (property != null)
        {
            foreach (XElement element in XDocument.Load(property.Value.ToString()).Descendants("item"))
            {
                dynamic data = new ExpandoObject();
                foreach (XElement item in element.Descendants())
                {
                    ((IDictionary<string, object>)data).Add(item.Name.LocalName, item.Value);
                }
                result.Add(data);
            }
        }
        return result;
    }

    public static string GetFormatDate(string date)
    {
        DateTime formatDate = DateTime.Parse(date);
        return formatDate.ToString("MMMM dd, yyyy");
    }

    public static string GetTextAreaFormat(string text)
    {
        return Regex.Replace(text, @"[\r\n]+", "<br />");
    }

    public static string GetSliderUrl(DynamicPublishedContent image)
    {
        string result = string.Empty;
        IPublishedContentProperty externalUrlProperty = image.GetProperty("externalUrl");
        if (externalUrlProperty != null)
        {
            result = externalUrlProperty.Value.ToString();
        }
        IPublishedContentProperty internalUrlProperty = image.GetProperty("internalUrl");
        if (internalUrlProperty != null)
        {
            if (internalUrlProperty.Value.ToString() != string.Empty)
            {
                result = new Document(Convert.ToInt32(internalUrlProperty.Value)).ToNode().NiceUrl;
            }
        }
        return result;
    }

    public static List<Nav> GetNav(IPublishedContent model, int level)
    {
        List<Nav> navs = new List<Nav>();
        while (model.Level > level)
        {
            navs.Add(new Nav
                {
                    Level = model.Level,
                    Name = model.GetProperty("linkName").Value.ToString() != string.Empty ? model.GetProperty("linkName").Value.ToString() : model.Name, //model.GetProperty("linkName").Value.ToString(),
                    Url = model.NiceUrl()
                });
            model = model.Up();
        }
        return navs.OrderBy(n => n.Level).ToList();
    }

    public static Guid InsertUserLogin(int userId)
    {
        Guid guid = Guid.NewGuid();
        string cn = GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertUserLogin",
                  new SqlParameter { ParameterName = "@ContextID", Value = guid, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.UniqueIdentifier },
                  new SqlParameter { ParameterName = "@UserID", Value = userId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                  new SqlParameter { ParameterName = "@Timeout", Value = DateTime.Now.Ticks + 600000000L * long.Parse(GetParameterValue(UmbracoType.TimeOut)), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                  );
        return guid;
    }

    public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Convert Image to byte[]
            image.Save(ms, format);
            byte[] imageBytes = ms.ToArray();

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(imageBytes);
            //return string.Format("data:image/jpeg;base64,{0}", base64String);
            return base64String;
        }
    }

    public static Image Base64ToImage(string base64String)
    {
        // Convert Base64 String to byte[]
        byte[] imageBytes = Convert.FromBase64String(base64String);
        MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

        // Convert byte[] to Image
        ms.Write(imageBytes, 0, imageBytes.Length);
        Image image = Image.FromStream(ms, true);
        return image;
    }

    public static string GetEmail(string userId, string userType)
    {
        string email = string.Empty;
        string type = PropertyValue(UmbracoType.UserType, Convert.ToInt32(userType)).ToLower();
        switch (type)
        {
            case "trainer":
            case "admin":
            case "system":
                email = new User(Convert.ToInt32(userId)).Email;
                break;
            case "client":
                email = new Member(Convert.ToInt32(userId)).Email;
                break;
        }
        return email;
    }
}

public enum UmbracoType
{
    GymnastNode,
    Connection,
    TimeOut,
    Category,
    State,
    Type,
    Unit,
    Gender,
    Platform,
    Rate,
    RegionalOffice,
    Paramount,
    Device,
    Exercise,
    Media,
    Photo,
    Site,
    UserType,
    ObjectType,
    Action,
    Training,
    Resistance,
    WorkoutState,
    Temp,
    Purchase,
    Notification,
    Template,
    Male,
    Female,
    Store
}

