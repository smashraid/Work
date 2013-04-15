using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using umbraco.BusinessLogic;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;

namespace Custom
{
    public class UmbracoClient : BaseTree
    {
        public UmbracoClient(string application)
            : base(application)
        {
            this.ShowContextMenu = false;
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Icon = FolderIcon;
            rootNode.OpenIcon = FolderIconOpen;
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        /// 
        /// Override the render method to create the newsletter tree
        /// 
        /// 
        public override void Render(ref XmlTree Tree)
        {
            Tree.treeCollection.Clear();
            User user = User.GetCurrent();
            IEnumerable<Member> members = Member.GetAllAsList().Where(m => Roles.GetRolesForUser(m.LoginName).Contains("Users"));
            Document[] documents = Document.GetChildrenForTree(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode)));
            IEnumerable<Document> gymnasts = documents.Where(d => d.getProperty("trainer").Value.ToString() == user.Id.ToString());
            foreach (Document gymnast in gymnasts)
            {
                Member member = members.Single(m => m.getProperty("gymnast").Value.ToString() == gymnast.Id.ToString());
                var node = XmlTreeNode.Create(this);
                node.NodeID = gymnast.Id.ToString();
                node.Text = member.Text;
                node.Icon = "b_user.png";
                node.Action = string.Format("javascript:openSendNewsletter({0});", gymnast.Id);
                node.Source = gymnast.HasChildren ? this.GetTreeServiceUrl(gymnast.Id).Replace("trainer", "content") : "";
                Tree.Add(node);
            }
            //Document document = documents.Single(d => d.Text == user.Name);
            
            //.Where(m=> m.getProperty("trainer").Value.ToString() == document.Id.ToString());

            //foreach (Member member in members)
            //{
            //    if (member.getProperty("trainer").Value.ToString() == document.Id.ToString())
            //    {
            //        var node = XmlTreeNode.Create(this);
            //        node.NodeID = member.Id.ToString();
            //        node.Text = member.Text;
            //        node.Icon = "b_user.png";
            //        node.Action = string.Format("javascript:openSendNewsletter({0});", member.Id);
            //        Tree.Add(node);
            //    }
            //}
        }



        public override void Render(ref System.Xml.XmlDocument Tree)
        {
            base.Render(ref Tree);
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(@"
                function openSendNewsletter(id) {                   
                    parent.right.document.location.href = '/umbraco/editContent.aspx?id=' + id;
                }
			");

            Javascript.Append(@"
                function openPreviousNewsletters() {
                    parent.right.document.location.href = 'custom/client.aspx';                    
                }
			");
        }

        protected override void OnNodeActionsCreated(NodeActionsEventArgs e)
        {
            if (e.IsRoot)
            {
                e.AllowedActions.Clear();
                e.AllowedActions.Add(ActionRefresh.Instance);
            }
            base.OnNodeActionsCreated(e);
        }

        protected override void CreateAllowedActions(ref List<umbraco.interfaces.IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ActionRefresh.Instance);
            base.CreateAllowedActions(ref actions);
        }

        protected override void CreateRootNodeActions(ref List<umbraco.interfaces.IAction> actions)
        {
            base.CreateRootNodeActions(ref actions);
        }

        protected override void OnAfterNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {
            base.OnAfterNodeRender(ref sender, ref node, e);
        }

        protected override void OnBeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {
            base.OnBeforeNodeRender(ref sender, ref node, e);
        }

        protected override void OnAfterTreeRender(object sender, TreeEventArgs e)
        {
            base.OnAfterTreeRender(sender, e);
        }

        protected override void OnBeforeTreeRender(object sender, TreeEventArgs e)
        {
            base.OnBeforeTreeRender(sender, e);
        }
    }

}