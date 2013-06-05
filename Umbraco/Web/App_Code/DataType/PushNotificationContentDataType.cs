using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using uComponents.DataTypes.DataTypeGrid;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;

/// <summary>
/// Summary description for NotificationContentDataType
/// </summary>
public class PushNotificationContent : AbstractDataEditor
{
    public PushNotificationControl control = new PushNotificationControl();
    //public NotificationPrevalueEditor prevalueEditor;

    public PushNotificationContent()
	{
		//
		// TODO: Add constructor logic here
		//
        base.RenderControl = control;
        control.Init += new EventHandler(Control_Init);
        base.DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
	}

    void DataEditorControl_OnSave(EventArgs e)
    {

    }

    void Control_Init(object sender, EventArgs e)
    {

    }

    //public override IDataPrevalue PrevalueEditor
    //{
    //    get
    //    {
    //        return prevalueEditor ?? (prevalueEditor = new NotificationPrevalueEditor(this));
    //    }
    //}

    public override string DataTypeName
    {
        get { return "Push Notification"; }
    }

    public override Guid Id
    {
        get { return new Guid("CC2A5329-6962-4781-88D5-0BB92977368F"); } 
    }
}

public class PushNotificationControl : Panel
{
    private Table grid;
    private Panel pager;
    private HiddenField platformSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public PushNotificationControl()
    {
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        grid = new Table { ID = "grid4", ClientIDMode = ClientIDMode.Static };
        pager = new Panel { ID = "pager4", ClientIDMode = ClientIDMode.Static };

        IEnumerable<PreValue> platforms = UmbracoCustom.DataTypeValue(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Platform)));
        platformSelected = new HiddenField { ID = "platformSelected", Value = platforms.First().Id.ToString(), ClientIDMode = ClientIDMode.Static };

        Panel pnlForm = new Panel { ID = "pnlForm4", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
        pnlForm.Controls.Add(grid);
        pnlForm.Controls.Add(pager);
        pnlForm.Controls.Add(platformSelected);

        Controls.Add(pnlForm);
    }
}

//public class PushNotificationPrevalueEditor : PlaceHolder, IDataPrevalue
//{
//    private static BaseDataType _datatype;
//    private Table grid;
//    private Panel pager;
//    private HiddenField platformSelected;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.PlaceHolder"/> class. 
//    /// </summary>
//    public PushNotificationPrevalueEditor(BaseDataType dataType)
//    {
//        _datatype = dataType;
//        InitializeControls();
//    }

//    private void InitializeControls()
//    {
//        grid = new Table { ID = "grid4", ClientIDMode = ClientIDMode.Static };
//        pager = new Panel { ID = "pager4", ClientIDMode = ClientIDMode.Static };

//        IEnumerable<PreValue> platforms = UmbracoCustom.DataTypeValue(int.Parse(UmbracoCustom.GetParameterValue(Parameter.Platform)));
//        platformSelected = new HiddenField { ID = "platformSelected", Value = platforms.First().Id.ToString(), ClientIDMode = ClientIDMode.Static };

//        Panel pnlForm = new Panel { ID = "pnlForm4", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
//        pnlForm.Controls.Add(grid);
//        pnlForm.Controls.Add(pager);
//        pnlForm.Controls.Add(platformSelected);
//        Controls.Add(pnlForm);
//    }

//    protected override void OnInit(EventArgs e)
//    {
//        base.OnInit(e);
//    }

//    public void Save()
//    {

//    }

//    public Control Editor { get { return this; } }
//}