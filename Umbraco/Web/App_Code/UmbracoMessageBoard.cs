using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
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
    public class UmbracoMessageBoard : BaseTree
    {
        public UmbracoMessageBoard(string application)
            : base(application)
        {
            string functionToCall = this.FunctionToCall;
            this.ShowContextMenu = false;
            this.id = 1159;
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

            var node = XmlTreeNode.Create(this);
            node.NodeID = "Topic" + Guid.NewGuid();
            node.Text = "Topic";
            node.Icon = "b_user.png";
            node.Action = string.Format("javascript:openMessageBoard({0});", 1355);
            var data = GetTreeServiceUrl(node.NodeID);
            Tree.Add(node);
        }

        public override void Render(ref System.Xml.XmlDocument Tree)
        {
            var x = Tree;
            var z = x.BaseURI;
            base.Render(ref Tree);
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(@"
                function openMessageBoard(id) {                   
                    parent.right.document.location.href = '/messageboard.aspx?id=' + id;
                }
			");

            Javascript.Append(@"
                function openPreviousNewsletters() {
                    parent.right.document.location.href = '/messageboard.aspx';                    
                }
			");
        }

        protected override void OnNodeActionsCreated(NodeActionsEventArgs e)
        {
            //e.AllowedActions.Clear();
            base.OnNodeActionsCreated(e);
        }

        protected override void CreateAllowedActions(ref List<umbraco.interfaces.IAction> actions)
        {
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
            var x = node;
            var z = sender;
            base.OnBeforeNodeRender(ref sender, ref node, e);
        }

        protected override void OnAfterTreeRender(object sender, TreeEventArgs e)
        {
            var x = sender;
            var z = x.GetType();
            base.OnAfterTreeRender(sender, e);
        }

        protected override void OnBeforeTreeRender(object sender, TreeEventArgs e)
        {
            var x = sender;
            var z = x.GetType();
            base.OnBeforeTreeRender(sender, e);


        }
    }
}