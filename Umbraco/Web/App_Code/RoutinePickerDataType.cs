using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using uComponents.DataTypes.DataTypeGrid;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;

/// <summary>
/// Summary description for RoutineDataType
/// </summary>
public class RoutinePicker : AbstractDataEditor
{
    public RoutineControl control = new RoutineControl();
    public RoutinePrevalueEditor prevalueEditor;

    public RoutinePicker()
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

    public override IDataPrevalue PrevalueEditor
    {
        get
        {
            return prevalueEditor ?? (prevalueEditor = new RoutinePrevalueEditor(this));
            //return base.PrevalueEditor;
        }
    }

    public override string DataTypeName
    {
        get { return "Routine Picker"; }
    }

    public override Guid Id
    {
        get { return new Guid("4824CF08-8987-47CF-94FE-334C79A0AEE3"); }
    }
}

public class RoutineControl : Panel
{
    //private Panel pnlCategory { get; set; }
    //private Label lblCategory { get; set; }
    //private Panel pnlControlCategory { get; set; }
    //private DropDownList Category { get; set; }

    //private Panel pnlExercise { get; set; }
    //private Label lblExercise { get; set; }
    //private Panel pnlControlExercise { get; set; }
    //private DropDownList Exercise { get; set; }

    //private Panel pnlValue { get; set; }
    //private Label lblValue { get; set; }
    //private Panel pnlControlValue { get; set; }
    //private TextBox Value { get; set; }

    //private Panel pnlState { get; set; }
    //private Label lblState { get; set; }
    //private Panel pnlControlState { get; set; }
    //private DropDownList State { get; set; }

    //private Panel pnlAdd { get; set; }
    //private Panel pnlControlAdd { get; set; }
    //private Button Add { get; set; }

    //private Panel pnlResult { get; set; }
    //private GridView Result { get; set; }

    //public Label Message { get; set; }

    private Table grid;
    private Panel pager;
    private HtmlButton button;
    private HtmlInputText inputText;
    private HiddenField categorySelected;
    private HtmlSelect select;
    private HtmlButton buttonApply;
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public RoutineControl()
    {
        //IEnumerable<PreValue> categories = UmbracoCustom.DataTypeValue(1225);
        //pnlCategory = new Panel { ID = "pnlCategory", CssClass = "control-group" };
        //lblCategory = new Label { ID = "lblCategory", Text = "Category ", CssClass = "control-label" };
        //Category = new DropDownList { ID = "ddlCategory", DataValueField = "Id", DataTextField = "Value", AutoPostBack = true };
        //Category.DataSource = categories;
        //Category.DataBind();
        //Category.SelectedIndexChanged += new EventHandler(Category_SelectedIndexChanged);
        //pnlControlCategory = new Panel { ID = "pnlControlCategory", CssClass = "controls" };
        //pnlControlCategory.Controls.Add(Category);
        //pnlCategory.Controls.Add(lblCategory);
        //pnlCategory.Controls.Add(pnlControlCategory);

        //pnlExercise = new Panel { ID = "pnlExercise", CssClass = "control-group" };
        //lblExercise = new Label { ID = "lblExercise", Text = "Exercise ", CssClass = "control-label" };
        //Exercise = new DropDownList { ID = "ddlExercise", DataValueField = "Id", DataTextField = "ExerciseName" };
        //Exercise.DataSource = GetExerciseByCategory();
        //Exercise.DataBind();
        //pnlControlExercise = new Panel { ID = "pnlControlExercise", CssClass = "controls" };
        //pnlControlExercise.Controls.Add(Exercise);
        //pnlExercise.Controls.Add(lblExercise);
        //pnlExercise.Controls.Add(pnlControlExercise);

        //pnlValue = new Panel { ID = "pnlValue", CssClass = "control-group" };
        //lblValue = new Label { ID = "lblValue", Text = "Value ", CssClass = "control-label" };
        //Value = new TextBox { ID = "txtValue" };
        //pnlControlValue = new Panel { ID = "pnlControlValue", CssClass = "controls" };
        //pnlControlValue.Controls.Add(Value);
        //pnlValue.Controls.Add(lblValue);
        //pnlValue.Controls.Add(pnlControlValue);

        //pnlState = new Panel { ID = "pnlState", CssClass = "control-group" };
        //lblState = new Label { ID = "lblState", Text = "State ", CssClass = "control-label" };
        //State = new DropDownList { ID = "ddlState" };
        //pnlControlState = new Panel { ID = "pnlControlState", CssClass = "controls" };
        //pnlControlState.Controls.Add(State);
        //pnlState.Controls.Add(lblState);
        //pnlState.Controls.Add(pnlControlState);

        //pnlAdd = new Panel { ID = "pnlAdd", CssClass = "control-group" };
        //Add = new Button { ID = "btnAdd", Text = "Add", CssClass = "btn" };
        //Add.Click += new EventHandler(Add_Click);
        //pnlControlAdd = new Panel { ID = "pnlControlAdd", CssClass = "controls" };
        //pnlControlAdd.Controls.Add(Add);
        //pnlAdd.Controls.Add(pnlControlAdd);

        //pnlResult = new Panel { ID = "pnlResult", CssClass = "control-group" };
        //Result = new GridView { ID = "grvResult", AutoGenerateColumns = false };
        //Result.Columns.Add(new BoundField { DataField = "Exercise.Id", ShowHeader = true, HeaderText = "Id" });
        //Result.Columns.Add(new BoundField { DataField = "Exercise.ExerciseName", ShowHeader = true, HeaderText = "Name" });
        //Result.Columns.Add(new BoundField { DataField = "Exercise.Description", ShowHeader = true, HeaderText = "Description" });
        //Result.Columns.Add(new BoundField { DataField = "Exercise.Type", ShowHeader = true, HeaderText = "Type" });
        //Result.Columns.Add(new BoundField { DataField = "Exercise.Unit", ShowHeader = true, HeaderText = "Unit" });
        //Result.Columns.Add(new BoundField { DataField = "Exercise.Category", ShowHeader = true, HeaderText = "Category" });
        //Result.Columns.Add(new BoundField { DataField = "Value", ShowHeader = true, HeaderText = "Value" });
        //Result.Columns.Add(new BoundField { DataField = "State", ShowHeader = true, HeaderText = "State" });
        //pnlResult.Controls.Add(Result);
        //GetRoutineByWorkout();

        //Message = new Label { ID = "lblMessage" };


    }

    //void Category_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    Exercise.Items.Clear();
    //    Exercise.DataSource = GetExerciseByCategory();
    //    Exercise.DataBind();
    //}

    //void Add_Click(object sender, EventArgs e)
    //{
    //    int documentId = int.Parse(HttpContext.Current.Request.QueryString["id"]);
    //    Document document = new Document(documentId);
    //    Document parentDocument = new Document(document.ParentId);

    //    int id = 0;
    //    string cn = ConfigurationManager.AppSettings["umbracoDbDSN"];
    //    string cmd = "InsertRoutine";
    //    SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, cmd,
    //        new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@DocumentId", Value = parentDocument.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@ExerciseId", Value = Exercise.SelectedValue, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@MemberId", Value = Convert.ToInt32(parentDocument.getProperty("member").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@TrainerId", Value = Convert.ToInt32(parentDocument.getProperty("trainer").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@WorkoutId", Value = document.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
    //        new SqlParameter { ParameterName = "@Value", Value = Value.Text, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal }
    //        );

    //    GetRoutineByWorkout();
    //}

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        //this.Controls.Add(Category);
        //this.Controls.Add(Exercise);
        //this.Controls.Add(Value);
        //this.Controls.Add(State); 
        //this.Controls.Add(Add);
        //this.Controls.Add(Result);
        grid = new Table { ID = "grid1", ClientIDMode = ClientIDMode.Static };
        pager = new Panel { ID = "pager1", ClientIDMode = ClientIDMode.Static };

        Panel pnlTemplate = new Panel();
        inputText = new HtmlInputText{ID = "templateName", ClientIDMode = ClientIDMode.Static};
        button = new HtmlButton { ID = "addWorkout", InnerText = "Add Workout to your repository ", ClientIDMode = ClientIDMode.Static };
        pnlTemplate.Controls.Add(new Label{Text = "Template Name"});
        pnlTemplate.Controls.Add(inputText);
        pnlTemplate.Controls.Add(button);

        //Panel pnlApplyTemplate = new Panel();
        //select = new HtmlSelect{ID = "ddlTemplate", DataSource = SelectTemplate()};
        //select.DataBind();
        //buttonApply = new HtmlButton { ID = "applyTemplate", InnerText = "Apply Template", ClientIDMode = ClientIDMode.Static };
        
        //pnlApplyTemplate.Controls.Add(new Label { Text = "Templates" });
        //pnlApplyTemplate.Controls.Add(select);
        //pnlApplyTemplate.Controls.Add(buttonApply);


        IEnumerable<PreValue> categories = UmbracoCustom.DataTypeValue(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.Category)));
        categorySelected = new HiddenField { ID = "categorySelected", Value = categories.First().Id.ToString(), ClientIDMode = ClientIDMode.Static };



        Panel pnlForm = new Panel { ID = "pnlForm1", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
        //pnlForm.Controls.Add(pnlCategory);
        //pnlForm.Controls.Add(pnlExercise);
        //pnlForm.Controls.Add(pnlValue);
        //pnlForm.Controls.Add(pnlState);
        //pnlForm.Controls.Add(pnlAdd);
        //pnlForm.Controls.Add(pnlResult);
        //pnlForm.Controls.Add(Message);

        pnlForm.Controls.Add(pnlTemplate);
        //pnlForm.Controls.Add(pnlApplyTemplate);
        pnlForm.Controls.Add(grid);
        pnlForm.Controls.Add(pager);
        pnlForm.Controls.Add(categorySelected);

        Controls.Add(pnlForm);
    }

    private List<string> SelectTemplate()
    {
        List<string> templates = new List<string>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        User user = umbraco.BusinessLogic.User.GetCurrent();
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectTemplateByTrainerId", new SqlParameter { ParameterName = "@TrainerId", Value = user.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            templates.Add(reader.GetValue(0).ToString());
        }

        return templates;
    }
}

public class RoutinePrevalueEditor : PlaceHolder, IDataPrevalue
{
    private static BaseDataType _datatype;

    //private Panel pnlCategory;
    //private Label lblCategory;
    //private Panel pnlControlCategory;
    //private DropDownList Category;

    //private Panel pnlName;
    //private Label lblName;
    //private Panel pnlControlName;
    //private TextBox Name;

    //private Panel pnlType;
    //private Label lblType;
    //private Panel pnlControlType;
    //private DropDownList Type;

    //private Panel pnlUnit;
    //private Label lblUnit;
    //private Panel pnlControlUnit;
    //private DropDownList Unit;

    //private Panel pnlDescription;
    //private Label lblDescription;
    //private Panel pnlControlDescription;
    //private TextBox Description;

    //private Panel pnlValue;
    //private Label lblValue;
    //private Panel pnlControlValue;
    //private TextBox Value;

    //private Panel pnlState;
    //private Label lblState;
    //private Panel pnlControlState;
    //private DropDownList State;

    //private Panel pnlAdd;
    //private Panel pnlControlAdd;
    //private Button Add;

    //private Panel pnlResult;
    //private GridView Result;

    private Table grid;
    private Panel pager;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.PlaceHolder"/> class. 
    /// </summary>
    public RoutinePrevalueEditor(BaseDataType dataType)
    {
        _datatype = dataType;
        InitializeControls();

    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Category));
        UmbracoCustom.DataTypeValue(id);
    }

    private void InitializeControls()
    {

        grid = new Table { ID = "grid", ClientIDMode = ClientIDMode.Static };
        pager = new Panel { ID = "pager", ClientIDMode = ClientIDMode.Static };

        Panel pnlForm = new Panel { ID = "pnlForm", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
       pnlForm.Controls.Add(grid);
        pnlForm.Controls.Add(pager);
        Controls.Add(pnlForm);
        //LoadData();
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        //Page.ClientScript.RegisterClientScriptInclude("jquery.js", ResolveUrl("~/scripts/jquery.js"));
        //Page.ClientScript.RegisterClientScriptInclude("jquery.ui.js", ResolveUrl("~/scripts/jjquery.ui.js"));
        //Page.ClientScript.RegisterClientScriptInclude("jquery.jqgrid.js", ResolveUrl("~/scripts/jquery.jqgrid.js"));
        //Page.ClientScript.RegisterClientScriptInclude("jquery.jqgrid.locale-es.js", ResolveUrl("~/scripts/jquery.jqgrid.locale-es.js"));
        //Page.ClientScript.RegisterClientScriptInclude("jquery.js", ResolveUrl("~/scripts/site.js"));
    }

    public void Save()
    {
        //NameValueCollection collection = Page.Request.Form;
        //if (collection["ctl00$body$ddlRenderControl"].ToUpper() == "4824CF08-8987-47CF-94FE-334C79A0AEE3")
        //{
        //    int id = 0;
        //    string cn = ConfigurationManager.AppSettings["umbracoDbDSN"];
        //    string cmd = "InsertExercise";
        //    SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, cmd,
        //        new SqlParameter { ParameterName = "@Id", Value = id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
        //        new SqlParameter { ParameterName = "@Name", Value = collection["ctl00$body$txtExerciseName"], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
        //        new SqlParameter { ParameterName = "@Description", Value = collection["ctl00$body$txtDescription"], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
        //        new SqlParameter { ParameterName = "@TypeId", Value = collection["ctl00$body$ddlType"], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
        //        new SqlParameter { ParameterName = "@UnitId", Value = collection["ctl00$body$ddlUnit"], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
        //        new SqlParameter { ParameterName = "@CategoryId", Value = collection["ctl00$body$ddlCategory"], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
        //        );
        //    LoadData();
        //}
    }

    public Control Editor { get { return this; } }
}