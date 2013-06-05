using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using PushSharp;
using PushSharp.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.web;

/// <summary>
/// Summary description for DataTypeTrainer
/// </summary>
public class SendPushNotification : AbstractDataEditor
{
    public SendPushNotificationControl control = new SendPushNotificationControl();

    public SendPushNotification()
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
        get { return "Send Push Notification"; }
    }

    public override Guid Id
    {
        get { return new Guid("CADE01AB-AA27-4499-926C-BB3A3A449B25"); }
    }
}

public class SendPushNotificationControl : Panel
{
    public Button Button { get; set; }
    //public Label Label;
    public GridView GridView;

    
    private PushBroker pushService;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public SendPushNotificationControl()
    {
        string root = HttpContext.Current.Server.MapPath("~/certificate");
        string certName = "metafitness.p12";
        byte[] cert = File.ReadAllBytes(Path.Combine(root, certName));

        pushService = new PushBroker();
        //ApplePushChannelSettings settings = new ApplePushChannelSettings(false, cert, "ilovebbq");
        //pushService.StartApplePushService(settings);

        //Label = new Label { ID = "lblPushMessage", ClientIDMode = ClientIDMode.Static, Text = "Test" };
        Button = new Button { ID = "btnPushSend", ClientIDMode = ClientIDMode.Static, Text = "Send" };
        Button.Click += new EventHandler(ButtonOnClick);

        GridView = new GridView { ID = "grdvPushResult", ClientIDMode = ClientIDMode.Static, AutoGenerateColumns = false, Width = 300, GridLines = GridLines.Both, ShowHeaderWhenEmpty = true };
        GridView.Columns.Add(new BoundField
            {
                HeaderText = "Message",
                DataField = "Message",
                HtmlEncode = true
            });
        GridView.Columns.Add(new BoundField
        {
            HeaderText = "SendDate",
            DataField = "SendDate",
            HtmlEncode = false,
            DataFormatString = "{0:MM-dd-yyyy}"
        });
    }

    private void ServiceException(object sender, Exception error)
    {
        throw new NotImplementedException();
    }

    private void ButtonOnClick(object sender, EventArgs eventArgs)
    {
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Workout = new Document(documentId);
        Document Gymnast = new Document(Workout.ParentId);
        int trainerId = Convert.ToInt32(Gymnast.getProperty("trainer").Value);
        int memberId = Convert.ToInt32(Gymnast.getProperty("member").Value);
        //Label.Text = string.Format("WorkoutId: {0} / TrainerId: {1} ", documentId, Gymnast.getProperty("trainer").Value);

        int userType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;
        int objectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;

        List<PushNotification> notifications = new List<PushNotification>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectNotificationByMember",
                                                       new SqlParameter
                                                       {
                                                           ParameterName = "@MemberId",
                                                           Value = memberId,
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

        foreach (PushNotification notification in notifications)
        {
            //pushService.QueueNotification(NotificationFactory.Apple().ForDeviceToken(notification.Token).WithAlert("A new workout is available on your account.").WithBadge(7));
            pushService.StopAllServices();

            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertPushMessage",
               new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
               new SqlParameter { ParameterName = "@NotificationId", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserId", Value = trainerId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@UserType", Value = userType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectId", Value = Workout.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@ObjectType", Value = objectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
               new SqlParameter { ParameterName = "@Message", Value = "A new workout is available on your account.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
               );
        }
        LoadData();
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        this.Controls.Add(Button);
        //this.Controls.Add(Label);
        this.Controls.Add(GridView);
        //this.Page.ClientScript.RegisterClientScriptInclude("DataEditorSettings.limitChars.js", this.Page.ClientScript.GetWebResourceUrl(typeof(CharlimitControl), "DataEditorSettings.Control.Charlimit.js"));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadData();
    }

    private void LoadData()
    {
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Workout = new Document(documentId);
        int objectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;
        List<PushMessage> pushMessages = new List<PushMessage>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectPushMessage",
          new SqlParameter { ParameterName = "@ObjectId", Value = Workout.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
          new SqlParameter { ParameterName = "@ObjectType", Value = objectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        );
        while (reader.Read())
        {
            pushMessages.Add(new PushMessage
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Token = reader.GetValue(1).ToString(),
                NotificationId = Convert.ToInt32(reader.GetValue(2)),
                UserId = Convert.ToInt32(reader.GetValue(3)),
                UserType = Convert.ToInt32(reader.GetValue(4)),
                ObjectId = Convert.ToInt32(reader.GetValue(5)),
                ObjectType = Convert.ToInt32(reader.GetValue(6)),
                Message = reader.GetValue(7).ToString(),
                SendDate = Convert.ToDateTime(reader.GetValue(8))
            });

        }
        GridView.DataSource = pushMessages;
        GridView.DataBind();
    }
}