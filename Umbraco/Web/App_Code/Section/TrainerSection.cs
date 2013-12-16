using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using umbraco.BusinessLogic;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace Custom
{
    public class TrainerSection : BaseTree
    {
        public TrainerSection(string application)
            : base(application)
        {
            this.ShowContextMenu = false;
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Icon = FolderIcon;
            rootNode.OpenIcon = FolderIconOpen;
            rootNode.NodeType = TreeAlias;
            rootNode.NodeID = "-1";
            //rootNode.Action = string.Format("javascript:openDashboard();");
            rootNode.Menu.Clear();
            rootNode.Menu.Add(ActionRefresh.Instance);
        }

        /// 
        /// Override the render method to create the newsletter tree
        /// 
        /// 
        public override void Render(ref XmlTree Tree)
        {
//            XmlTree xTree = new XmlTree();
//            ITreeService treeParams = new TreeService(1099, "content", false, false, TreeDialogModes.none, null);
//            TreeDefinition tree = TreeDefinitionCollection.Instance.FindTree("content");
//            BaseTree instance = tree.CreateInstance();
//            instance.SetTreeParameters((ITreeService)treeParams);
//            instance.Render(ref xTree);



//            string json = @"{
//                              ""data"": {
//                                  ""title"": ""Workout 2"",
//                                  ""icon"": ""/umbraco/images/umbraco/bin.png"",
//                                  ""attributes"": {
//                                    ""class"": ""sprTree noSpr"",
//                                    ""href"": ""javascript:openContent(\u00271102\u0027);"",
//                                    ""umb:nodedata"": ""{\u0027menu\u0027:\u0027K,,,Z,T,L\u0027,\u0027nodeType\u0027:\u0027content\u0027,\u0027source\u0027:\u0027/umbraco/tree.aspx?rnd=45fec03ca8454bf394a4828a40513c60\\u0026id=1102\\u0026treeType=content\\u0026contextMenu=true\\u0026isDialog=false\u0027}"",
//                                  }                                  
//                                },
//                              ""attributes"": {
//                                  ""id"": ""1102"",  
//                                  ""class"": ""dim overlay-new"",  
//                                  ""rel"": ""dataNode"",  
//                                  ""umb:type"": ""content""                                    
//                                }                              
//                            }";

//            string json1 = @"{
//                              ""data"": {
//                                  ""title"": ""Workout 2"",
//                                  ""icon"": ""/umbraco/images/umbraco/bin.png"",
//                                  ""attributes"": {
//                                    ""class"": ""sprTree noSpr"",
//                                    ""href"": ""javascript:openContent(\u00271102\u0027);"",
//                                    ""umb:nodedata"": ""{\u0027menu\u0027:\u0027K,,,Z,T,L\u0027,\u0027nodeType\u0027:\u0027content\u0027,\u0027source\u0027:\u0027/umbraco/tree.aspx?rnd=45fec03ca8454bf394a4828a40513c60\\u0026id=1102\\u0026treeType=content\\u0026contextMenu=true\\u0026isDialog=false\u0027}"",
//                                  }                                  
//                                },
//                              ""attributes"": {
//                                  ""id"": ""1102"",  
//                                  ""class"": ""dim overlay-new"",  
//                                  ""rel"": ""dataNode"",  
//                                  ""umb:type"": ""content""                                    
//                                }                              
//                            }";

//            JObject o = JObject.Parse(json);
//            JObject o1 = JObject.Parse(json1);



//            var z = this.NodeKey;

//            Dictionary.DictionaryItem[] tmp = this.id == this.StartNodeID
//                                                  ? Dictionary.getTopMostItems
//                                                  : new Dictionary.DictionaryItem(this.id).Children;



            Tree.treeCollection.Clear();
            User user = User.GetCurrent();
            List<Member> members = Member.GetAllAsList().Where(m => Roles.GetRolesForUser(m.LoginName).Contains("Users") && m.getProperty("isActive").Value.ToString() == "1").ToList();
            List<IContent> documents = ApplicationContext.Current.Services.ContentService.GetChildren(int.Parse(UmbracoCustom.GetParameterValue(UmbracoType.GymnastNode))).ToList();
            List<IContent> gymnasts = documents.Where(d => d.GetValue("trainer").ToString() == user.Id.ToString() && members.Select(m => m.Id).Contains(d.GetValue<int>("member"))).ToList();
            foreach (IContent gymnast in gymnasts)
            {
                Member member = members.Single(m => m.getProperty("gymnast").Value.ToString() == gymnast.Id.ToString());
                var node = XmlTreeNode.Create(this);
                node.NodeID = gymnast.Id.ToString();
                node.Text = member.Text;
                node.Icon = "user.gif";
                node.Action = string.Format("javascript:openDashboard({0}, {1});", gymnast.Id, member.Id);
                //node.Source = gymnast.HasChildren ? this.GetTreeServiceUrl(gymnast.Id).Replace("trainer", "content") : "";
                //node.Source = "/umbraco/surface/HelperSurface/GetNodes";
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
                function openTrainer(id) {                   
                    parent.right.document.location.href = '/umbraco/editContent.aspx?id=' + id;
                }
			");

            Javascript.Append(@"
                function openPreviousNewsletters() {
                    parent.right.document.location.href = 'custom/client.aspx';                    
                }
			");

            Javascript.Append(@"
                function openTopic(id) {                   
                    parent.right.document.location.href = '/topic.aspx?id=' + id;
                }
			");

            Javascript.Append(@"
                function openDashboard(gymnastid, memberid) {                   
                    parent.right.document.location.href = '/dashboard.aspx?gymnastid=' + gymnastid + '&memberid=' + memberid;
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

    public class Node
    {
        public Data Data { get; set; }
        public Attributes Attributes { get; set; }
    }

    public class Attributes
    {
    }

    public class Data
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        //public string  { get; set; }
    }

    /// <summary>
    /// An ITreeService class that returns the values found in the Query String or any dictionary
    /// 
    /// </summary>
    internal class TreeRequestParams : ITreeService
    {
        private Dictionary<string, string> m_params;

        public string NodeKey
        {
            get
            {
                if (!this.m_params.ContainsKey("nodeKey"))
                    return "";
                else
                    return this.m_params["nodeKey"];
            }
        }

        public string Application
        {
            get
            {
                if (this.m_params.ContainsKey("app"))
                    return this.m_params["app"];
                if (!this.m_params.ContainsKey("appAlias"))
                    return "";
                else
                    return this.m_params["appAlias"];
            }
        }

        public int StartNodeID
        {
            get
            {
                if (this.m_params.ContainsKey("id"))
                {
                    string str = this.m_params["id"];
                }
                int result;
                if (int.TryParse(HttpContext.Current.Request.QueryString["id"], out result))
                    return result;
                else
                    return -1;
            }
        }

        public string FunctionToCall
        {
            get
            {
                if (!this.m_params.ContainsKey("functionToCall"))
                    return "";
                else
                    return this.m_params["functionToCall"];
            }
        }

        public bool IsDialog
        {
            get
            {
                bool result;
                if (this.m_params.ContainsKey("isDialog") && bool.TryParse(this.m_params["isDialog"], out result))
                    return result;
                else
                    return false;
            }
        }

        public TreeDialogModes DialogMode
        {
            get
            {
                if (this.m_params.ContainsKey("dialogMode"))
                {
                    if (this.IsDialog)
                    {
                        try
                        {
                            return (TreeDialogModes)Enum.Parse(typeof(TreeDialogModes), this.m_params["dialogMode"]);
                        }
                        catch
                        {
                            return TreeDialogModes.none;
                        }
                    }
                }
                return TreeDialogModes.none;
            }
        }

        public bool ShowContextMenu
        {
            get
            {
                bool result;
                if (this.m_params.ContainsKey("contextMenu") && bool.TryParse(this.m_params["contextMenu"], out result))
                    return result;
                else
                    return true;
            }
        }

        public string TreeType
        {
            get
            {
                if (!this.m_params.ContainsKey("treeType"))
                    return "";
                else
                    return this.m_params["treeType"];
            }
        }

        private TreeRequestParams()
        {

        }

        public static TreeRequestParams FromQueryStrings()
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (string key in HttpContext.Current.Request.QueryString.Keys)
                items.Add(key, HttpUtility.HtmlEncode(HttpContext.Current.Request.QueryString[key]));
            return TreeRequestParams.FromDictionary(items);
        }

        public static TreeRequestParams FromDictionary(Dictionary<string, string> items)
        {
            TreeRequestParams treeRequestParams = new TreeRequestParams();
            treeRequestParams.m_params = items;
            return treeRequestParams;
        }

        /// <summary>
        /// Converts the tree parameters to a tree service object
        /// 
        /// </summary>
        /// 
        /// <returns/>
        public TreeService CreateTreeService()
        {
            TreeService treeService = new TreeService();
            treeService.ShowContextMenu = this.ShowContextMenu;
            treeService.IsDialog = this.IsDialog;
            treeService.DialogMode = this.DialogMode;
            treeService.App = this.Application;
            treeService.TreeType = this.TreeType;
            treeService.NodeKey = this.NodeKey;
            treeService.StartNodeID = this.StartNodeID;
            treeService.FunctionToCall = this.FunctionToCall;
            return treeService;
        }
    }
}