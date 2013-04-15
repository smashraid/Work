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

namespace Custom{

    public class UmbracoRepository : BaseTree
    {
        public UmbracoRepository(string application)
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

            var nodeWorkout = XmlTreeNode.Create(this);
            nodeWorkout.NodeID = "Workout" + Guid.NewGuid();
            nodeWorkout.Text = "Workout";
            nodeWorkout.Icon = "b_user.png";
            nodeWorkout.Action = string.Format("javascript:openWorkoutRepository({0});", 1355);
            Tree.Add(nodeWorkout);

            var nodeExercise = XmlTreeNode.Create(this);
            nodeExercise.NodeID = "Exercise" + Guid.NewGuid();
            nodeExercise.Text = "Exercise";
            nodeExercise.Icon = "b_user.png";
            nodeExercise.Action = string.Format("javascript:openExerciseRepository({0});", 1355);
            Tree.Add(nodeExercise);
        }
        
        public override void Render(ref System.Xml.XmlDocument Tree)
        {
            base.Render(ref Tree);
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(@"
                function openWorkoutRepository(id) {                   
                    parent.right.document.location.href = '/workout.aspx?id=' + id;
                }
			");

            Javascript.Append(@"
                function openExerciseRepository(id) {
                    parent.right.document.location.href = '/exercise.aspx?id=' + id;                    
                }
			");
        }

        protected override void OnNodeActionsCreated(NodeActionsEventArgs e)
        {
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