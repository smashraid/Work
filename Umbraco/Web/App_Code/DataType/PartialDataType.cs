using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.uicontrols;
using uComponents.DataTypes.DataTypeGrid;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;
using Microsoft.ApplicationBlocks.Data;
using ClientDependency.Core;



public class PartialControl : Panel
{
    public string Id { get; set; }
    public string View { get; set; }
    private readonly Panel panel = new Panel();
    private readonly Literal form = new Literal();

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public PartialControl()
    {
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        var v = GetView();
        form.ID = Id + "content";
        form.Text = v;
        panel.ID = Id;
        panel.Attributes.Add("data-view", View);
        panel.Controls.Add(form);
        Controls.Add(panel);
    }

    private string GetView()
    {
        Uri baseUri = new Uri("http://localhost:21773/");
        HttpClient httpclient = new HttpClient();
        httpclient.BaseAddress = baseUri;
        string response = httpclient.GetStringAsync("umbraco/surface/DataTypeSurface/GetPartial?name=" + View).Result;
        return new MvcHtmlString(response).ToHtmlString();
    }
}

/// <summary>
/// Summary description for DeviceNestedDataType
/// </summary>
public class Partial : AbstractDataEditor
{
    public PartialControl control = new PartialControl();
    private PartialPrevalueEditor prevalueEditor;

    public Partial()
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
        SortedList s = ((PartialPrevalueEditor)PrevalueEditor).StoredConfiguration;
        if (s.ContainsKey("id"))
        {
            control.Id = s.GetByIndex(s.IndexOfKey("id")).ToString();
        }

        if (s.ContainsKey("view"))
        {
            control.View = s.GetByIndex(s.IndexOfKey("view")).ToString();
        }

    }

    public override IDataPrevalue PrevalueEditor
    {
        get
        {
            return prevalueEditor ?? (prevalueEditor = new PartialPrevalueEditor(this));
        }
    }

    public override string DataTypeName
    {
        get { return "Partial"; }
    }

    public override Guid Id
    {
        get { return new Guid("CD7698FB-595B-4FE3-B1D7-35057FB3F47E"); }
    }
}



public class PartialPrevalueEditor : PlaceHolder, IDataPrevalue
{
    private BaseDataType _datatype;
    private TextBox _id;
    private TextBox _view;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.PlaceHolder"/> class. 
    /// </summary>
    public PartialPrevalueEditor(BaseDataType dataType)
    {
        _datatype = dataType;
        SetupChildControls();
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Sets the configuration properties from the Configuration function
        if (!Page.IsPostBack)
        {
            SortedList s = StoredConfiguration;

            if (s.ContainsKey("id"))
            {
                _id.Text = s.GetByIndex(s.IndexOfKey("id")).ToString();
            }

            if (s.ContainsKey("view"))
            {
                _view.Text = s.GetByIndex(s.IndexOfKey("view")).ToString();
            }
        }
    }

    private void SetupChildControls()
    {
        _id = new TextBox { ID = "txtId", Text = "", Width = 250 };
        _view = new TextBox { ID = "txtView", Text = "", Width = 250 };
        Controls.Add(_id);
        Controls.Add(_view);
    }

    public void Save()
    {
        //int num = 0;
        //int num1 = SqlHelper.ExecuteScalar<int>("select max(sortorder) from cmsDataTypePreValues where datatypenodeid = @dtdefid", new IParameter[1]{ SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId)});
        //SqlHelper.ExecuteNonQuery("update cmsDataTypePreValues set [value] = @value, sortorder = @sortorder where id = @id", SqlHelper.CreateParameter("@value", str.Split(new char[1] {'|'})[1]), SqlHelper.CreateParameter("@sortorder", num), SqlHelper.CreateParameter("@id", result));

        _datatype.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), DBTypes.Nvarchar.ToString(), true);
        SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

        SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,@so,@alias)", SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId), SqlHelper.CreateParameter("@value", _id.Text), SqlHelper.CreateParameter("@so", 0), SqlHelper.CreateParameter("@alias", "id"));
        SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,@so,@alias)", SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId), SqlHelper.CreateParameter("@value", _view.Text), SqlHelper.CreateParameter("@so", 1), SqlHelper.CreateParameter("@alias", "view"));

        //ScriptManager.GetCurrent(Page).SetFocus((Control)_textbox);
    }

    public Control Editor
    {
        get { return this; }
    }

    protected override void Render(HtmlTextWriter writer)
    {
        //writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("dataBaseDatatype") + "</div>");
        //_id.RenderControl(writer);
        //_view.RenderControl(writer);
        //writer.Write("<br style='clear: both'/></div>");
        //List<KeyValuePair<int, string>> keyValuePairList = PrevaluesAsKeyValuePairList;
        //if (keyValuePairList.Count > 0)
        //{
        //    writer.Write("<div class='propertyItem'><table style='width: 100%' id=\"prevalues\">");
        //    writer.Write("<tr class='header'><th style='width: 15%'>Text</th><td colspan='2'>Value</td></tr>");
        //    foreach (KeyValuePair<int, string> keyValuePair in keyValuePairList)
        //        writer.Write("<tr class=\"row\"><td class=\"text\">" + (object)keyValuePair.Value + "</td><td class=\"value\"> " + keyValuePair.Key.ToString() + "</td><td><a onclick='javascript:return ConfirmPrevalueDelete();' href='?id=" + _datatype.DataTypeDefinitionId + "&delete=" + keyValuePair.Key.ToString() + "'>" + ui.Text("delete") + "</a> <span class=\"handle\" style=\"cursor:move\">sort<span></td></tr>");
        //    writer.Write("</table><br style='clear: both'/></div>");
        //}
        //writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("addPrevalue") + "</div>");
        //_textbox.RenderControl(writer);
        //writer.Write("<br style='clear: both'/></div>");
        //_tbhidden.RenderControl(writer);
        writer.WriteLine("<table>");
        writer.Write("<tr><th>Id:</th><td>");
        _id.RenderControl(writer);
        writer.Write("</td></tr>");
        writer.Write("<tr><th>View Name:</th><td>");
        _view.RenderControl(writer);
        writer.Write("</td></tr>");
        writer.Write("</table>"); 
         
    }

    public SortedList Prevalues
    {
        get
        {
            SortedList sortedList = new SortedList();
            IRecordsReader recordsReader = SqlHelper.ExecuteReader("Select id, [value] from cmsDataTypePreValues where DataTypeNodeId = " + _datatype.DataTypeDefinitionId + " order by sortorder", new IParameter[0]);
            while (recordsReader.Read())
                sortedList.Add(recordsReader.GetInt("id"), recordsReader.GetString("value"));
            recordsReader.Close();
            return sortedList;
        }
    }

    public SortedList StoredConfiguration
    {
        get
        {
            SortedList sortedList = new SortedList();
            IRecordsReader recordsReader = SqlHelper.ExecuteReader("Select alias, [value] from cmsDataTypePreValues where DataTypeNodeId = " + _datatype.DataTypeDefinitionId + " order by sortorder", new IParameter[0]);
            while (recordsReader.Read())
                sortedList.Add(recordsReader.GetString("alias"), recordsReader.GetString("value"));
            recordsReader.Close();
            return sortedList;
        }
    }

    public List<KeyValuePair<int, string>> PrevaluesAsKeyValuePairList
    {
        get
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            IRecordsReader recordsReader = SqlHelper.ExecuteReader("Select id, [value] from cmsDataTypePreValues where DataTypeNodeId = " + _datatype.DataTypeDefinitionId + " order by sortorder", new IParameter[0]);
            while (recordsReader.Read())
                list.Add(new KeyValuePair<int, string>(recordsReader.GetInt("id"), recordsReader.GetString("value")));
            recordsReader.Close();
            return list;
        }
    }

    private void DeletePrevalue(int id)
    {
        SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where id = " + id, new IParameter[0]);
    }

    public static ISqlHelper SqlHelper
    {
        get
        {
            return Application.SqlHelper;
        }
    }
}