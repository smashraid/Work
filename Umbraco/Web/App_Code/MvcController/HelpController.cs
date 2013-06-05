using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Http.Controllers;
using System.Xml.Linq;

namespace Services.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /Help/

        public ActionResult Index()
        {
            XmlDocumentationProvider docProvider = (XmlDocumentationProvider)GlobalConfiguration.Configuration.Services.GetDocumentationProvider();
            //XDocument document = XDocument.Load(Server.MapPath("~/bin/Services.XML"));
            //IEnumerable<XElement> elememt = document.Descendants("member");
            List<Operation> operations = new List<Operation>();

            foreach (var api in GlobalConfiguration.Configuration.Services.GetApiExplorer().ApiDescriptions)
            {
                //string action = string.Format("{0}.{1}", api.ActionDescriptor.ControllerDescriptor.ControllerType.FullName, api.ActionDescriptor.ActionName);
                //XElement x = elememt.SingleOrDefault(e=>e.Attribute("name").Value.Contains(action));

                Operation operation = new Operation
                    {
                        Controller = api.ActionDescriptor.ControllerDescriptor.ControllerName,
                        HttpMethod = api.HttpMethod.Method,
                        Path = api.RelativePath,
                        Documentation = api.Documentation,
                        Parameters = new List<Parameter>(),
                        Errors = docProvider.GetError(api.ActionDescriptor)
                    };
                
                if (api.ActionDescriptor.ReturnType != null)
                {
                    ReflectedHttpActionDescriptor reflectedHttpActionDescriptor = (ReflectedHttpActionDescriptor)api.ActionDescriptor;
                    Type[] types = reflectedHttpActionDescriptor.MethodInfo.ReturnParameter.ParameterType.GetGenericArguments();
                    string result = OperationReturnType(types);
                    //if (types.Length > 0)
                    //{
                    //    operation.Response = api.ActionDescriptor.ReturnType.Name.Replace(string.Format("`{0}", types.Length), "<" + string.Join(",", types.Select(t => t.Name)) + ">");
                    //}
                    //else
                    //{
                    //    operation.Response = api.ActionDescriptor.ReturnType.Name;
                    //}
                    if (result == string.Empty)
                    {
                        operation.Response = reflectedHttpActionDescriptor.MethodInfo.ReturnParameter.ParameterType.Name.Replace(string.Format("`{0}", types.Length), "<" + string.Join(",", types.Select(t => t.Name)) + ">");
                    }
                    else
                    {
                        operation.Response = reflectedHttpActionDescriptor.MethodInfo.ReturnParameter.ParameterType.Name.Replace(string.Format("`{0}", types.Length), "<" + result + ">");
                    }

                }
                else
                {
                    operation.Response = "Void";
                }


                foreach (ApiParameterDescription propertyInfo in api.ParameterDescriptions)
                {
                    if (propertyInfo.ParameterDescriptor.ParameterType.IsClass && propertyInfo.ParameterDescriptor.ParameterType.Namespace != "System")
                    {
                        PropertyInfo[] p = propertyInfo.ParameterDescriptor.ParameterType.GetProperties();
                        operation.Parameters.Add(new Parameter
                            {
                                Documentation = propertyInfo.Documentation,
                                IsClass = true,
                                Name = propertyInfo.Name,
                                Properties =
                                   p.Select(pi => new Property1
                                   {
                                       Name = pi.Name,
                                       Type = (pi.PropertyType.Name.Contains("Nullable") ? pi.PropertyType.Name.Replace("`1", "<" + pi.PropertyType.GetGenericArguments()[0].Name + ">") : pi.PropertyType.Name)
                                   }).ToList()
                            });
                    }
                    else
                    {
                        operation.Parameters.Add(new Parameter
                            {
                                Documentation = propertyInfo.Documentation,
                                IsClass = false,
                                Name = propertyInfo.Name,
                                Properties = new List<Property1>
                                    {
                                        new Property1
                                            {
                                                Name = propertyInfo.Name,
                                                Type = propertyInfo.ParameterDescriptor.ParameterType.Name
                                            }
                                    }
                            });
                    }
                }
                operations.Add(operation);
            }
            return View(operations);
        }

        public string OperationReturnType(Type[] types)
        {
            string result = string.Empty;
            if (types.Length == 1)
            {
                var type = types[0];
                var arguments = type.GetGenericArguments();
                result += type.Name.Replace(string.Format("`{0}", types.Length), "<" + string.Join(",", arguments.Select(t => t.Name)) + ">");
                if (arguments.Length == 1)
                {
                    result = result.Replace(string.Join(",", arguments.Select(t => t.Name)), "{0}");
                    string result2 = OperationReturnType(arguments);
                    result = string.Format(result, result2);
                }
                else if (arguments.Length > 1)
                {
                    result = result.Replace(result, type.Name.Replace(string.Format("`{0}", arguments.Length), "<" + string.Join(",", arguments.Select(t => t.Name)) + ">"));
                }
            }
            return result;
        }
    }

}