using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;

/// <summary>
/// Summary description for DataTypeTrainer
/// </summary>
public class TrainerPicker : AbstractDataEditor
{
    public TrainerControl control = new TrainerControl();

   public TrainerPicker()
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
        TrainerControl dropDownList = (TrainerControl)sender;
        dropDownList.DropDownList.SelectedValue = !string.IsNullOrEmpty(base.Data.Value.ToString())  ? base.Data.Value.ToString() : string.Empty;
    }

    void DataEditorControl_OnSave(EventArgs e)
    {
        base.Data.Value = control.DropDownList.SelectedValue;
    }

    public override string DataTypeName
    {
        get { return "Trainer Picker"; }
    }

    public override Guid Id
    {
        get { return new Guid("1BA9853C-8772-43A8-937B-E865B21DFDDA"); }
    }
}

public class TrainerControl : Panel
{
    public DropDownList DropDownList { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public TrainerControl()
    {
        DropDownList = new DropDownList();
        DropDownList.Items.Add(new ListItem("", ""));
        var users = User.getAll().Where(u => u.UserType.Name == "trainer" && u.Disabled == false);
        foreach (User user in users)
        {
            DropDownList.Items.Add(new ListItem(user.Name, user.Id.ToString()));
        }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        this.Controls.Add(DropDownList);
        //this.Page.ClientScript.RegisterClientScriptInclude("DataEditorSettings.limitChars.js", this.Page.ClientScript.GetWebResourceUrl(typeof(CharlimitControl), "DataEditorSettings.Control.Charlimit.js"));
    }
}