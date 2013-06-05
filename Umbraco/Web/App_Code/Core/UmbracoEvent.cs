using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;


/// <summary>
/// Summary description for UmbracoEvent
/// </summary>
public class UmbracoEvent : IApplicationEventHandler
{
    public UmbracoEvent()
    {
        //
        // TODO: Add constructor logic here
        //
        Member.BeforeSave += new Member.SaveEventHandler(Member_BeforeSave);
        Member.AfterSave += new Member.SaveEventHandler(Member_AfterSave);
        Member.New += new Member.NewEventHandler(Member_New);
        Member.AfterNew += new EventHandler<umbraco.cms.businesslogic.NewEventArgs>(Member_AfterNew);
        Document.BeforeSave += new Document.SaveEventHandler(Document_BeforeSave);
        Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);

        BaseContentTree.AfterNodeRender += new BaseContentTree.AfterNodeRenderEventHandler(BaseContentTreeOnAfterNodeRender);
    }

    private void BaseContentTreeOnAfterNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs eventArgs)
    {
        if (node.IsProtected.GetValueOrDefault(true) && umbraco.helper.GetCurrentUmbracoUser().UserType.Alias == "trainer")
        {
            //Writers cannot see protected pages
            //sender.
            string nodeType = node.NodeType;
            
        }

    }

    void Document_BeforeSave(Document sender, umbraco.cms.businesslogic.SaveEventArgs e)
    {
        //throw new NotImplementedException();
        //Property SendEmailNotification = sender.getProperty("sendEmailNotification");
        //if (SendEmailNotification.Value.ToString() == "0")
        //{
            
        //}
        //Property SendPushNotification = sender.getProperty("sendPushNotification");
        //if (SendPushNotification.Value.ToString() == "0")
        //{

        //}
    }

    void Document_AfterSave(Document sender, umbraco.cms.businesslogic.SaveEventArgs e)
    {
        //throw new NotImplementedException();
        //Property SendEmailNotification = sender.getProperty("sendEmailNotification");
        //Property SendPushNotification = sender.getProperty("sendPushNotification");
    }

    void Member_AfterNew(object sender, umbraco.cms.businesslogic.NewEventArgs e)
    {
        //DocumentType documentType = DocumentType.GetByAlias("Gymnast");
        //Document document = Document.MakeNew(sender.Text, documentType, new User("admin"), int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)));
        //document.getProperty("member").Value = sender.Id;
        //document.Save();
        //Log.Add(LogTypes.New, int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)), string.Format("Create new user {0} in Member_New event", sender.ToString()));
    }

    void Member_New(Member sender, umbraco.cms.businesslogic.NewEventArgs e)
    {
        //DocumentType documentType = DocumentType.GetByAlias("Gymnast");
        //Document document = Document.MakeNew(sender.Text, documentType, new User("admin"), int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)));
        //document.getProperty("member").Value = sender.Id;
        //document.Save();
        //Log.Add(LogTypes.New, int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)), string.Format("Create new user {0} in Member_New event", sender.Text));
    }

    void Member_AfterSave(Member sender, umbraco.cms.businesslogic.SaveEventArgs e)
    {
        Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
        Document documentMember = documents.SingleOrDefault(d => d.Text == sender.Text);
        Property gymnast = sender.getProperty("gymnast");
        if (documentMember == null && Roles.GetRolesForUser(sender.LoginName).Any())
        {
            DocumentType documentType = DocumentType.GetByAlias("Gymnast");
            Document document = Document.MakeNew(sender.Text, documentType, new User("admin"), int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
            document.getProperty("member").Value = sender.Id;
            document.Save();
            gymnast.Value = document.Id;
            sender.Save();
            //Log.Add(LogTypes.New, int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)), string.Format("Create new user {0} in Member_AfterSave event", sender.Text));
        }
        else if (gymnast.Value == null && Roles.GetRolesForUser(sender.LoginName).Any())
        {
            gymnast.Value = documentMember.Id;
            sender.Save();
        }
        //sender.Save();
        //if (sender.LoginName != sender.Text && documentMember == null)
    }

    void Member_BeforeSave(Member sender, umbraco.cms.businesslogic.SaveEventArgs e)
    {
        //DocumentType documentType = DocumentType.GetByAlias("Gymnast");
        //Document document = Document.MakeNew(sender.Text, documentType, new User("admin"), int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)));
        //document.getProperty("member").Value = sender.Id;
        //document.Save();
        //Log.Add(LogTypes.New, int.Parse(UmbracoCustom.GetParameterValue(Parameter.GymnastDocument)), string.Format("Create new user {0} in Member_BeforeSave event", sender.Text));
    }



    //ApplicationBase
    //ApplicationStartupHandler
    //IApplicationEventHandler


    public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
    {
        //throw new NotImplementedException();
    }

    public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
    {
        //throw new NotImplementedException();
    }

    public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
    {
        //throw new NotImplementedException();
    }
}