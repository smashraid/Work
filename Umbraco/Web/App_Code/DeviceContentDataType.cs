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
/// Summary description for DeviceNestedDataType
/// </summary>
public class DeviceContent : AbstractDataEditor
{
    //public DeviceControl control = new DeviceControl();
    public DevicePrevalueEditor prevalueEditor;

    public DeviceContent()
	{
		//
		// TODO: Add constructor logic here
		//
        //base.RenderControl = control;
        //control.Init += new EventHandler(Control_Init);
        //base.DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
	}



    void DataEditorControl_OnSave(EventArgs e)
    {

    }

    void Control_Init(object sender, EventArgs e)
    {

    }

    public override IDataPrevalue PrevalueEditor
    {
        get
        {
            return prevalueEditor ?? (prevalueEditor = new DevicePrevalueEditor(this));
        }
    }

    public override string DataTypeName
    {
        get { return "Device"; }
    }

    public override Guid Id
    {
        get { return new Guid("B2E98CA9-CFFD-40FA-B245-F3F923F2DBA3"); }
    }
}

//public class DeviceControl : Panel
//{
//    private Table grid;
//    private Panel pager;
//    private HiddenField categorySelected;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
//    /// </summary>
//    public DeviceControl()
//    {
//    }

//    protected override void OnInit(EventArgs e)
//    {
//        base.OnInit(e);
//        grid = new Table { ID = "grid3", ClientIDMode = ClientIDMode.Static };
//        pager = new Panel { ID = "pager3", ClientIDMode = ClientIDMode.Static };
//        categorySelected = new HiddenField { ID = "categorySelected", Value = "", ClientIDMode = ClientIDMode.Static };

//        Panel pnlForm = new Panel { ID = "pnlForm3", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
//        pnlForm.Controls.Add(grid);
//        pnlForm.Controls.Add(pager);
//        pnlForm.Controls.Add(categorySelected);

//        Controls.Add(pnlForm);
//    }
//}

public class DevicePrevalueEditor : PlaceHolder, IDataPrevalue
{
    private static BaseDataType _datatype;
    private Table grid;
    private Panel pager;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.PlaceHolder"/> class. 
    /// </summary>
    public DevicePrevalueEditor(BaseDataType dataType)
    {
        _datatype = dataType;
        InitializeControls();
    }

    private void InitializeControls()
    {
        grid = new Table { ID = "grid3", ClientIDMode = ClientIDMode.Static };
        pager = new Panel { ID = "pager3", ClientIDMode = ClientIDMode.Static };

        Panel pnlForm = new Panel { ID = "pnlForm3", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
        pnlForm.Controls.Add(grid);
        pnlForm.Controls.Add(pager);
        Controls.Add(pnlForm);
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
    }

    public void Save()
    {

    }

    public Control Editor { get { return this; } }
}