using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;

/// <summary>
/// Summary description for DataTypeTrainer
/// </summary>
public class ChatContent : AbstractDataEditor
{
    public ChatControl control = new ChatControl();

    public ChatContent()
    {
        //
        // TODO: Add constructor logic here
        //
        //set rendercontrol
        base.RenderControl = control;
        //init event
        control.Init += new EventHandler(Control_Init);
        //save event
        base.DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
    }

    void Control_Init(object sender, EventArgs e)
    {

    }

    void DataEditorControl_OnSave(EventArgs e)
    {

    }

    public override string DataTypeName
    {
        get { return "Chat Content"; }
    }

    public override Guid Id
    {
        get { return new Guid("FE31C8D2-8931-4B4C-900C-2059C8BA63DB"); }
    }
}

public class ChatControl : Panel
{
    public HtmlTextArea HtmlTextArea;
    public HtmlButton HtmlButton;
    //public HtmlInputButton HtmlInputButton;
    //public GridView GridView;
    public Panel Panel;
    public Literal Literal;

    private PushBroker pushService;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public ChatControl()
    {
        string root = System.Web.HttpContext.Current.Server.MapPath("~/certificate");
        string certName = "metafitness.p12";
        byte[] cert = System.IO.File.ReadAllBytes(Path.Combine(root, certName));

        //pushService = new PushBroker();
        //ApplePushChannelSettings settings = new ApplePushChannelSettings(false, cert, "ilovebbq");
        //pushService.RegisterAppleService(settings);


        HtmlTextArea = new HtmlTextArea { ID = "txtChatMessage", Name = "txtChatMessage", Cols = 50, Rows = 5, ClientIDMode = ClientIDMode.Static, Value = "" };

        HtmlButton = new HtmlButton { ID = "btnChatSubmit", ClientIDMode = ClientIDMode.Static, InnerText = "Submit" };
        HtmlButton.ServerClick += new EventHandler(HtmlButtonOnServerClick);
        //HtmlInputButton = new HtmlInputButton { Value = "Submit", ID = "btnChatSubmit", ClientIDMode = ClientIDMode.Static, Name = "btnChatSubmit" };
        //HtmlInputButton.ServerClick += HtmlInputButtonOnServerClick;

        Literal = new Literal();

        Panel = new Panel { ID = "pnlChatContent", ClientIDMode = ClientIDMode.Static };

        Panel pnlChatInsert = new Panel();
        pnlChatInsert.Controls.Add(HtmlTextArea);
        pnlChatInsert.Controls.Add(HtmlButton);

        Panel.Controls.Add(Literal);
        Panel.Controls.Add(pnlChatInsert);
    }

    private void HtmlButtonOnServerClick(object sender, EventArgs eventArgs)
    {
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Gymnast = new Document(documentId);

        int userType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;

        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertChat",
           new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@GymnastId", Value = Gymnast.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@Message", Value = HtmlTextArea.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 },
           new SqlParameter { ParameterName = "@UserId", Value = User.GetCurrent().Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@UserType", Value = userType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
           );
        HtmlTextArea.Value = string.Empty;
        LoadData();
        NotificationMessage();
    }


    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        this.Controls.Add(Panel);

        //this.Page.ClientScript.RegisterClientScriptInclude("DataEditorSettings.limitChars.js", this.Page.ClientScript.GetWebResourceUrl(typeof(CharlimitControl), "DataEditorSettings.Control.Charlimit.js"));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadData();

    }

    private void LoadData()
    {
        Literal.Text = string.Empty;
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Gymnast = new Document(documentId);

        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectChat",
          new SqlParameter { ParameterName = "@GymnastId", Value = Gymnast.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        );

        while (reader.Read())
        {
            Literal.Text += string.Format("<div>{0}<br/>{1}</div>", Convert.ToDateTime(reader.GetValue(5)).ToShortDateString(), reader.GetValue(2));
        }
    }

    private void NotificationMessage()
    {
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Gymnast = new Document(documentId);
        int memberId = Convert.ToInt32(Gymnast.getProperty("member").Value);

        SmtpClient client = new SmtpClient();
        MailMessage message = new MailMessage();
        message.IsBodyHtml = true;
        message.From = new MailAddress("app@metafitnessatx.com");
        message.To.Add(new Member(memberId).Email);
        message.Subject = "MetaFitness";
        message.Body = "Your Trainer has sent you a new message. Check your MetaFitness App now.";
        client.Send(message);

        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertEmailMessage",
           new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@ObjectId", Value = Gymnast.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == "chat").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@Email", Value = new Member(memberId).Email, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
           new SqlParameter { ParameterName = "@Message", Value = "Your Trainer has sent you a new message. Check your MetaFitness App now.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
           );


        foreach (PushNotification notification in SelectNotificationByMember(memberId))
        {
            pushService.QueueNotification(new AppleNotification().ForDeviceToken(notification.Token).WithAlert("Your Trainer has sent you a new message.").WithBadge(7));
            pushService.StopAllServices();

            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
               new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
               new SqlParameter { ParameterName = "@NotificationId", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = new User("system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "system").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = Gymnast.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(u => u.Value.ToLower() == "chat").Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Message", Value = "Your Trainer has sent you a new message.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
               );
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
}