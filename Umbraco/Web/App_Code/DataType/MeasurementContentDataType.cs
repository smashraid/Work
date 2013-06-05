using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;

/// <summary>
/// Summary description for MeasurementContentDataType
/// </summary>
public class MeasurementContent : AbstractDataEditor
{
    MeasurementControl control = new MeasurementControl();


	public MeasurementContent()
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

    public override string DataTypeName
    {
        get { return "Measurement Content"; }
    }

    public override Guid Id
    {
        get { return new Guid("ED9F4A70-3AC8-4EFE-9915-0AF39C758DC2"); }
    }
}

public class MeasurementControl : Panel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Panel"/> class.
    /// </summary>
    public MeasurementControl()
    {
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        Panel pnlForm = new Panel { ID = "pnlForm2", CssClass = "form-horizontal", ClientIDMode = ClientIDMode.Static };
        Controls.Add(pnlForm);
    }
}
