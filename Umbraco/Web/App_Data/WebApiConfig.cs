using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.XPath;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

public class WebApiConfig
{
    public static void Configure(HttpConfiguration config)
    {
        //config.Filters.Add(new ValidationActionFilter());

        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{action}/{id}",
            defaults: new { id = RouteParameter.Optional });

        config.Services.Replace(typeof(IDocumentationProvider), new XmlDocumentationProvider((HttpContext.Current.Server.MapPath("~/bin/Web.XML"))));
    }
}

/// <summary>
/// A custom <see cref="IDocumentationProvider"/> that reads the API documentation from an XML documentation file.
/// </summary>
public class XmlDocumentationProvider : IDocumentationProvider
{
    private XPathNavigator _documentNavigator;
    private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
    private const string ParameterExpression = "param[@name='{0}']";

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
    /// </summary>
    /// <param name="documentPath">The physical path to XML document.</param>
    public XmlDocumentationProvider(string documentPath)
    {
        if (documentPath == null)
        {
            throw new ArgumentNullException("documentPath");
        }
        XPathDocument xpath = new XPathDocument(documentPath);
        _documentNavigator = xpath.CreateNavigator();
    }

    public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
    {
        XPathNavigator methodNode = GetMethodNode(actionDescriptor);
        if (methodNode != null)
        {
            XPathNavigator summaryNode = methodNode.SelectSingleNode("summary");
            if (summaryNode != null)
            {
                return summaryNode.Value.Trim();
            }
        }

        return null;
    }

    public virtual List<Error> GetError(HttpActionDescriptor actionDescriptor)
    {
        List<Error> errors = new List<Error>();
        XPathNavigator methodNode = GetMethodNode(actionDescriptor);
        if (methodNode != null)
        {
            XPathNodeIterator summaryNode = methodNode.Select("error");
            if (summaryNode != null)
            {
                while (summaryNode.MoveNext())
                {
                    errors.Add(new Error
                    {
                        Message = summaryNode.Current.InnerXml,
                        Code = int.Parse(summaryNode.Current.GetAttribute("name", ""))
                    });
                }
            }
        }

        return errors;
    }

    public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
    {
        ReflectedHttpParameterDescriptor reflectedParameterDescriptor =
            parameterDescriptor as ReflectedHttpParameterDescriptor;
        if (reflectedParameterDescriptor != null)
        {
            XPathNavigator methodNode = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor);
            if (methodNode != null)
            {
                string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                XPathNavigator parameterNode =
                    methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, ParameterExpression,
                                                              parameterName));
                if (parameterNode != null)
                {
                    return parameterNode.Value.Trim();
                }
            }
        }

        return null;
    }

    private XPathNavigator GetMethodNode(HttpActionDescriptor actionDescriptor)
    {
        ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
        if (reflectedActionDescriptor != null)
        {
            string selectExpression = String.Format(CultureInfo.InvariantCulture, MethodExpression,
                                                    GetMemberName(reflectedActionDescriptor.MethodInfo));
            return _documentNavigator.SelectSingleNode(selectExpression);
        }

        return null;
    }

    private static string GetMemberName(MethodInfo method)
    {
        string name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", method.DeclaringType.FullName,
                                    method.Name);
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 0)
        {
            string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
            name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames));
        }

        return name;
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            // Format the generic type name to something like: Generic{System.Int32,System.String}
            Type genericType = type.GetGenericTypeDefinition();
            Type[] genericArguments = type.GetGenericArguments();
            string typeName = genericType.FullName;

            // Trim the generic parameter counts from the name
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
            return String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", typeName,
                                 String.Join(",", argumentTypeNames));
        }

        return type.FullName;
    }
}

public class Operation
{
    public string HttpMethod { get; set; }
    public string Path { get; set; }
    public string Controller { get; set; }
    public string Documentation { get; set; }
    public string Response { get; set; }
    public List<Parameter> Parameters { get; set; }
    public List<Error> Errors { get; set; }
}

public class Parameter
{
    public string Name { get; set; }
    public string Documentation { get; set; }
    public List<Property1> Properties { get; set; }
    public bool IsClass { get; set; }
}

public class Property1
{
    public string Type { get; set; }
    public string Name { get; set; }
}

public class Error
{
    public int Code { get; set; }
    public string Message { get; set; }
}