using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.editorControls.textfield;

/// <summary>
/// Summary description for DataTypeTrainer
/// </summary>
public class ExercisePicker : AbstractDataEditor
{
    public ExerciseControl control;

    public ExercisePicker()
    {
        //
        // TODO: Add constructor logic here
        //
        control = new ExerciseControl();
        base.RenderControl = control;
        control.Init += new EventHandler(Control_Init);

        base.DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        base.DataEditorControl.Load += new EventHandler(DataEditorControl_Load);
        base.DataEditorControl.DataBinding += new EventHandler(DataEditorControl_DataBinding);

        base.DataEditorControl.Editor.DataBinding += new EventHandler(Editor_DataBinding);
        base.DataEditorControl.Editor.Load += new EventHandler(Editor_Load);

        base.DataEditorControl.Control.DataBinding += new EventHandler(Control_DataBinding);
        base.DataEditorControl.Control.Load += new EventHandler(Control_Load);
    }


    private void Editor_Load(object sender, EventArgs e)
    {
        AbstractDataEditorControl control = (AbstractDataEditorControl)sender;
    }

    void Editor_DataBinding(object sender, EventArgs e)
    {
        AbstractDataEditorControl control = (AbstractDataEditorControl)sender;
    }

    void DataEditorControl_DataBinding(object sender, EventArgs e)
    {
        AbstractDataEditorControl control = (AbstractDataEditorControl)sender;
    }

    void DataEditorControl_Load(object sender, EventArgs e)
    {
        AbstractDataEditorControl control = (AbstractDataEditorControl)sender;
    }

    void Control_Load(object sender, EventArgs e)
    {
        ExerciseControl dropDownList = (ExerciseControl)sender;
    }

    void Control_DataBinding(object sender, EventArgs e)
    {
        ExerciseControl dropDownList = (ExerciseControl)sender;
    }

    void Control_Init(object sender, EventArgs e)
    {
        ExerciseControl dropDownList = (ExerciseControl)sender;
        TextFieldEditor exerciseName = (TextFieldEditor)dropDownList.Parent.Parent.FindControl("InsertexerciseName");
        if (exerciseName != null)
        {
            control = dropDownList;
        }
    }

    void DataEditorControl_OnSave(EventArgs e)
    {
        var exerciseId = HttpContext.Current.Request[control.Exercise.UniqueID];
        control.Exercise.SelectedValue = exerciseId;
        base.Data.Value = exerciseId;

        TextFieldEditor exerciseName = (TextFieldEditor)control.Parent.Parent.FindControl("InsertexerciseName");
        if (exerciseName != null)
        {
            exerciseName.Text = control.Exercise.SelectedItem.Text;
        }
    }

    public override umbraco.interfaces.IData Data
    {
        get
        {
            return base.Data;
        }
    }

    public override umbraco.interfaces.IDataEditor DataEditor
    {
        get
        {
            return base.DataEditor;
        }
    }

    public override umbraco.interfaces.IDataPrevalue PrevalueEditor
    {
        get
        {
            return base.PrevalueEditor;
        }
    }

    public override string DataTypeName
    {
        get { return "Exercise Picker"; }
    }

    public override Guid Id
    {
        get { return new Guid("F3CDA711-3571-40AB-96D2-E9EC07DF4AB0"); }
    }


}

public class ExerciseControl : Panel
{
    public DropDownList Category;
    public DropDownList Exercise;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public ExerciseControl()
    {
        Category = new DropDownList { ID = "category", ClientIDMode = ClientIDMode.Static, AutoPostBack = true, DataTextField = "Value", DataValueField = "Id", EnableViewState = false, ViewStateMode = ViewStateMode.Disabled };
        int id = Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.Category));
        Category.DataSource = UmbracoCustom.DataTypeValue(id);
        Category.DataBind();
        Category.SelectedIndexChanged += new EventHandler(Category_SelectedIndexChanged);
        Exercise = new DropDownList { ID = "exercise", ClientIDMode = ClientIDMode.Static, AutoPostBack = false, DataTextField = "exercise", DataValueField = "id", EnableViewState = false, ViewStateMode = ViewStateMode.Disabled };
        Exercise.TextChanged += new EventHandler(Exercise_TextChanged);
        Exercise.SelectedIndexChanged += new EventHandler(Exercise_SelectedIndexChanged);
    }

    void Category_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            LoadData();
        }
    }

    void Exercise_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    void Exercise_TextChanged(object sender, EventArgs e)
    {

    }

    private void LoadData()
    {
        string id = (Category.SelectedValue != string.Empty ? Category.SelectedValue : "23");
        Document document = new Document(Convert.ToInt32(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Property property = document.getProperty("exercise");
        var exercises = UmbracoCustom.GetDataTypeGrid(property).Where(g => g.category == id);
        
        Exercise.Items.Clear();
        foreach (var exercise in exercises)
        {
            Exercise.Items.Add(new ListItem(exercise.exercise, exercise.id));
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        LoadData();
        base.OnLoad(e);

    }

    protected override void OnInit(EventArgs e)
    {
        this.Controls.Add(Category);
        this.Controls.Add(Exercise);
        base.OnInit(e);

        //this.Page.ClientScript.RegisterClientScriptInclude("DataEditorSettings.limitChars.js", this.Page.ClientScript.GetWebResourceUrl(typeof(CharlimitControl), "DataEditorSettings.Control.Charlimit.js"));
    }
}