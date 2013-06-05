using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;

/// <summary>
/// Summary description for DataTypeTrainer
/// </summary>
public class SendEmailNotification : AbstractDataEditor
{
    public SendEmailNotificationControl control = new SendEmailNotificationControl();

    public SendEmailNotification()
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
        get { return "Send Email Notification"; }
    }

    public override Guid Id
    {
        get { return new Guid("7724D87B-34F3-4874-9CAE-566A02716E09"); }
    }
}

public class SendEmailNotificationControl : Panel
{
    public Button Button { get; set; }
    //public Label Label;
    public GridView GridView;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public SendEmailNotificationControl()
    {
        //Label = new Label { ID = "lblEmailMessage", ClientIDMode = ClientIDMode.Static, Text = "Test" };
        Button = new Button { ID = "btnEmailSend", ClientIDMode = ClientIDMode.Static, Text = "Send" };
        Button.Click += new EventHandler(ButtonOnClick);

        GridView = new GridView { ID = "grdvEmailResult", ClientIDMode = ClientIDMode.Static, AutoGenerateColumns = false, Width = 300, GridLines = GridLines.Both, ShowHeaderWhenEmpty = true};
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

    private void ButtonOnClick(object sender, EventArgs eventArgs)
    {
        int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
        Document Workout = new Document(documentId);
        Document Gymnast = new Document(Workout.ParentId);
        int trainerId = Convert.ToInt32(Gymnast.getProperty("trainer").Value);
        int memberId = Convert.ToInt32(Gymnast.getProperty("member").Value);
        Member member = new Member(memberId);
        //Label.Text = string.Format("WorkoutId: {0} / TrainerId: {1} ", documentId, Gymnast.getProperty("trainer").Value);

        SmtpClient client = new SmtpClient();
        MailMessage message = new MailMessage();
        message.IsBodyHtml = true;
        message.From = new MailAddress("app@metafitnessatx.com");
        message.To.Add(member.Email);
        message.Subject = "MetaFitness";
        message.Body = "A new workout is available on your account.";
        client.Send(message);

        int userType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.UserType))).Single(u => u.Value.ToLower() == "trainer").Id;
        int objectType = UmbracoCustom.DataTypeValue(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.ObjectType))).Single(o => o.Value.ToLower() == "workout").Id;
        
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertEmailMessage",
           new SqlParameter { ParameterName = "@Id", Value = new int(), Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@UserId", Value = trainerId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@UserType", Value = userType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@ObjectId", Value = Workout.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@ObjectType", Value = objectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
           new SqlParameter { ParameterName = "@Email", Value = member.Email, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
           new SqlParameter { ParameterName = "@Message", Value = "A new workout is available on your account. Check your MetaFitness App now.", Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 500 }
           );

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
        List<EmailMessage> emailMessages = new List<EmailMessage>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectEmailMessage",
          new SqlParameter { ParameterName = "@ObjectId", Value = Workout.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
          new SqlParameter { ParameterName = "@ObjectType", Value = objectType, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        );
        while (reader.Read())
        {
            emailMessages.Add(new EmailMessage
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    UserId = Convert.ToInt32(reader.GetValue(1)),
                    UserType = Convert.ToInt32(reader.GetValue(2)),
                    ObjectId = Convert.ToInt32(reader.GetValue(3)),
                    ObjectType = Convert.ToInt32(reader.GetValue(4)),
                    Email = reader.GetValue(5).ToString(),
                    Message = reader.GetValue(6).ToString(),
                    SendDate = Convert.ToDateTime(reader.GetValue(7))
                });

        }
        GridView.DataSource = emailMessages;
        GridView.DataBind();
    }
}