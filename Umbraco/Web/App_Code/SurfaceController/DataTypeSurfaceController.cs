using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using umbraco.BusinessLogic;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;
using Newtonsoft.Json;

/// <summary>
/// Summary description for DataTypeSurfaceController
/// </summary>
public class DataTypeSurfaceController : Umbraco.Web.Mvc.SurfaceController
{
    public DataTypeSurfaceController()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    [HttpGet]
    public string GetPrevalue(int id)
    {
        StringBuilder result = new StringBuilder();
        result.Append("<select>");
        IEnumerable<PreValue> values = UmbracoCustom.DataTypeValue(id);
        foreach (PreValue preValue in values)
        {
            result.Append(string.Format("<option value='{0}'>{1}</option>", preValue.Id, preValue.Value));
        }
        result.Append("</select>");
        return result.ToString();
    }

    [HttpGet]
    public string GetExerciseByCategory(int id)
    {
        StringBuilder result = new StringBuilder();
        //result.Append("<select>");
        List<Exercise> exercises = SelectExerciseByCategory(id);
        foreach (Exercise exercise in exercises)
        {
            result.Append(string.Format("<option value='{0}'>{1}</option>", exercise.Id, exercise.ExerciseName));
        }
        //result.Append("</select>");
        return result.ToString();
    }

    [HttpGet]
    public string GetDeviceByPlatform(int id)
    {
        StringBuilder result = new StringBuilder();
        //result.Append("<select>");
        List<Device> devices = SelectDeviceByPlatform(id);
        foreach (Device device in devices)
        {
            result.Append(string.Format("<option value='{0}'>{1}</option>", device.Id, device.DeviceName));
        }
        //result.Append("</select>");
        return result.ToString();
    }

    [HttpGet]
    public PartialViewResult GetPartial(string name)
    {
        return PartialView(name);
    }

    [HttpPost]
    public JsonResult GetExercise(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        IEnumerable<Exercise> exercises;

        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            exercises = SelectExercise(string.Format("SELECT  Id, Name, TrainerId, CategoryId, IsActive FROM dbo.Exercise WHERE {0} AND TrainerId IS NULL ORDER BY {1} {2}", search, sortColumn, sortOrder));
        }
        else
        {
            exercises = SelectExercise(string.Format("SELECT  Id, Name, TrainerId, CategoryId, IsActive FROM dbo.Exercise WHERE TrainerId IS NULL ORDER BY {0} {1}", sortColumn, sortOrder));
        }

        List<Exercise> result = exercises.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
            {
                page = currentPage,
                records = exercises.Count(),
                total = (int)Math.Ceiling((float)exercises.Count() / pagesize),
                root = (result.Select(item => new
                    {
                        id = item.Id,
                        cell =
                                                  new[]
                                                      {
                                                          item.Id.ToString(),
                                                          item.Category,
                                                          item.ExerciseName,
                                                          item.IsActive.ToString(),
                                                          item.CategoryId.ToString()
                                                      }
                    })).ToArray()
            };
        return Json(jsonData);
    }

    [HttpPost]
    public JsonResult GetExerciseTrainer(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        IEnumerable<Exercise> exercises;

        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            exercises = SelectExercise(string.Format("SELECT  Id, Name, TrainerId, CategoryId, IsActive FROM dbo.Exercise WHERE {0} AND TrainerId = {1} ORDER BY {2} {3}", search, user.Id, sortColumn, sortOrder));
        }
        else
        {
            exercises = SelectExercise(string.Format("SELECT  Id, Name, TrainerId, CategoryId, IsActive FROM dbo.Exercise WHERE TrainerId = {0} ORDER BY {1} {2}", user.Id, sortColumn, sortOrder));
        }

        List<Exercise> result = exercises.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
        {
            page = currentPage,
            records = exercises.Count(),
            total = (int)Math.Ceiling((float)exercises.Count() / pagesize),
            root = (result.Select(item => new
            {
                id = item.Id,
                cell =
                                          new[]
                                                      {
                                                          item.Id.ToString(),
                                                          item.Category,
                                                          item.ExerciseName,
                                                          item.IsActive.ToString(),
                                                          item.CategoryId.ToString()
                                                      }
            })).ToArray()
        };
        return Json(jsonData);
    }

    [HttpPost]
    public JsonResult GetRoutine(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        IEnumerable<Routine> routines;
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
        int documentId = int.Parse(nameValueCollection["id"]);
        Document document = new Document(documentId);

        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            routines = SelectRoutineByWorkout(string.Format("SELECT dbo.Exercise.Id AS ExerciseId, dbo.Exercise.Name , dbo.Exercise.TrainerId ,  dbo.Exercise.CategoryId, dbo.Exercise.IsActive, dbo.Routine.Id, dbo.Routine.Value, dbo.Routine.StateId, dbo.Routine.SortOrder FROM dbo.Routine INNER JOIN dbo.Exercise ON dbo.Routine.ExerciseId = dbo.Exercise.Id WHERE {0} AND WorkoutId = {1} ORDER BY {2} {3}", search, document.Id, sortColumn, sortOrder));
        }
        else
        {
            routines = SelectRoutineByWorkout(string.Format("SELECT dbo.Exercise.Id AS ExerciseId , dbo.Exercise.Name , dbo.Exercise.TrainerId , dbo.Exercise.CategoryId, dbo.Exercise.IsActive, dbo.Routine.Id, dbo.Routine.Value, dbo.Routine.StateId, dbo.Routine.SortOrder FROM dbo.Routine INNER JOIN dbo.Exercise ON dbo.Routine.ExerciseId = dbo.Exercise.Id WHERE WorkoutId = {0} ORDER BY {1} {2}", document.Id, sortColumn, sortOrder));
        }

        List<Routine> result = routines.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
        {
            page = currentPage,
            records = routines.Count(),
            total = (int)Math.Ceiling((float)routines.Count() / pagesize),
            root = (result.Select(item => new
            {
                id = item.Id,
                cell =
                                          new[]
                                                      {
                                                          item.Id.ToString(), 
                                                          item.Exercise.Category,
                                                          item.Exercise.ExerciseName, 
                                                          item.Exercise.Type,
                                                          item.Exercise.Unit,
                                                          item.State,
                                                          item.Exercise.CategoryId.ToString(),
                                                          item.Exercise.Id.ToString(),
                                                          item.SortOrder.ToString()
                                                      }
            })).ToArray()
        };
        return Json(jsonData);
    }

    [HttpPost]
    public JsonResult GetDevice(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        IEnumerable<Device> devices;
        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            devices = SelectDevice(string.Format("SELECT Id, PlatformId, Name, IsActive FROM dbo.Device WHERE {0} ORDER BY {1} {2}", search, sortColumn, sortOrder));

        }
        else
        {
            devices = SelectDevice(string.Format("SELECT Id, PlatformId, Name, IsActive FROM dbo.Device ORDER BY {0} {1}", sortColumn, sortOrder));
        }

        List<Device> result = devices.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
        {
            page = currentPage,
            records = devices.Count(),
            total = (int)Math.Ceiling((float)devices.Count() / pagesize),
            root = (result.Select(item => new
            {
                id = item.Id,
                cell =
                                          new[]
                                                      {
                                                          item.Id.ToString(), 
                                                          item.Platform,
                                                          item.DeviceName,
                                                          item.IsActive.ToString(), 
                                                          item.PlatformId.ToString()
                                                      }
            })).ToArray()
        };
        return Json(jsonData);
    }

    [HttpPost]
    public JsonResult GetNotification(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        IEnumerable<PushNotification> notifications;
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
        int memberId = int.Parse(nameValueCollection["id"]);
        Member member = new Member(memberId);
        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            notifications = SelectDeviceByMember(string.Format("SELECT dbo.Notification.Id, dbo.Notification.MemberId, dbo.Notification.Token, dbo.Notification.DeviceId, dbo.Notification.IsActive, dbo.Device.Name, dbo.Device.PlatformId FROM dbo.Notification INNER JOIN dbo.Device ON dbo.Notification.DeviceId = dbo.Device.Id WHERE {0} AND MemberId = {1} ORDER BY {2} {3}", search, member.Id, sortColumn, sortOrder));
        }
        else
        {
            notifications = SelectDeviceByMember(string.Format("SELECT dbo.Notification.Id, dbo.Notification.MemberId, dbo.Notification.Token, dbo.Notification.DeviceId, dbo.Notification.IsActive, dbo.Device.Name, dbo.Device.PlatformId FROM dbo.Notification INNER JOIN dbo.Device ON dbo.Notification.DeviceId = dbo.Device.Id WHERE MemberId = {0} ORDER BY {1} {2}", member.Id, sortColumn, sortOrder));
        }

        List<PushNotification> result = notifications.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
        {
            page = currentPage,
            records = notifications.Count(),
            total = (int)Math.Ceiling((float)notifications.Count() / pagesize),
            root = (result.Select(item => new
            {
                id = item.Id,
                cell =
                                          new[]
                                                      {
                                                          item.Id.ToString(), 
                                                          item.Platform,
                                                          item.Device, 
                                                          item.Token,
                                                          item.IsActive.ToString(),
                                                          item.MemberId.ToString(),
                                                          item.PlatformId.ToString(),
                                                          item.DeviceId.ToString()
                                                      }
            })).ToArray()
        };
        return Json(jsonData);
    }

    [HttpPost]
    public JsonResult UpdateOrder( IEnumerable<int> routines )
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        for (int i = 0; i < routines.Count(); i++)
        {
            SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateRoutineOrder",
                    new SqlParameter { ParameterName = "@Id", Value = routines.ToList()[i], Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@SortOrder", Value = i+1, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.TinyInt }
                    );
        }
       
        return Json("Success");
    }

    public string GetFilter(string field, string op, string data)
    {
        string operation = string.Empty;
        switch (op)
        {
            case "eq":
                operation = (string.Format(" {0} = '{1}' ", field, data)); // "eq" - equal
                break;
            case "ne":
                operation = (string.Format(" {0} <> {1} ", field, data)); // "ne" - not equal
                break;
            case "lt":
                operation = (string.Format(" {0} < {1} ", field, data)); // "lt" - less than
                break;
            case "le":
                operation = (string.Format(" {0} <= {1} ", field, data)); // "le" - less than or equal to
                break;
            case "gt":
                operation = (string.Format(" {0} > {1} ", field, data)); // "gt" - greater than
                break;
            case "ge":
                operation = (string.Format(" {0} >= {1} ", field, data)); // "ge" - greater than or equal to
                break;
            case "bw":
                operation = (string.Format(" {0} LIKE '{1}%' ", field, data)); // "bw" - begins with
                break;
            case "bn":
                operation = (string.Format(" {0} NOT LIKE '{1}%' ", field, data)); // "bn" - does not begin with
                break;
            case "ew":
                operation = (string.Format(" {0} LIKE '%{1}' ", field, data)); // "ew" - ends with
                break;
            case "en":
                operation = (string.Format(" {0} NOT LIKE '%{1}' ", field, data)); // "en" - does not end with
                break;
            case "cn":
                operation = (string.Format(" {0} LIKE '%{1}%' ", field, data)); // "cn" - contains
                break;
            case "nc":
                operation = (string.Format(" {0} NOT LIKE '%{1}%' ", field, data)); // "nc" - does not contain
                break;
            case "in":
                operation = (string.Format(" {0} IS IN ({1}) ", field, data)); // "in" - is in
                break;
            case "ni":
                operation = (string.Format(" {0} IS NOT IN ({1}) ", field, data)); // "ni" - is not in
                break;
            case "nu":
                operation = (string.Format(" {0} IS NULL ", field, data)); // "nu" - is null
                break;
            case "nn":
                operation = (string.Format(" {0} IS NOT NULL ", field, data)); // "nn" - is not null
                break;
        }
        return operation;
    }

    [HttpGet]
    public ContentResult Test()
    {
        return Content("Hello World");
    }

    [HttpPost]
    public JsonResult SetExercise(Exercise exercise, string oper)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        switch (oper)
        {
            case "add":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertExercise",
                    new SqlParameter { ParameterName = "@Id", Value = exercise.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = exercise.ExerciseName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    //new SqlParameter { ParameterName = "@Description", Value = exercise.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = "@TrainerId", Value = (object)exercise.TrainerId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@TypeId", Value = int.Parse(exercise.Type), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@UnitId", Value = int.Parse(exercise.Unit), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@CategoryId", Value = int.Parse(exercise.Category), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                    );
                break;
            case "edit":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateExercise",
                    new SqlParameter { ParameterName = "@Id", Value = exercise.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = exercise.ExerciseName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    //new SqlParameter { ParameterName = "@Description", Value = exercise.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
                    //new SqlParameter { ParameterName = "@TypeId", Value = int.Parse(exercise.Type), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@UnitId", Value = int.Parse(exercise.Unit), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@CategoryId", Value = int.Parse(exercise.Category), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@IsActive", Value = exercise.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                    );
                break;
            case "del":
                break;

        }
        return Json("Success Save");
    }

    [HttpPost]
    public JsonResult SetExerciseTrainer(Exercise exercise, string oper)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        switch (oper)
        {
            case "add":
                User user = umbraco.BusinessLogic.User.GetCurrent();
                exercise.TrainerId = user.Id;
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertExercise",
                    new SqlParameter { ParameterName = "@Id", Value = exercise.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = exercise.ExerciseName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    //new SqlParameter { ParameterName = "@Description", Value = exercise.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = "@TrainerId", Value = (object)exercise.TrainerId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@TypeId", Value = int.Parse(exercise.Type), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@UnitId", Value = int.Parse(exercise.Unit), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@CategoryId", Value = int.Parse(exercise.Category), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                    );
                break;
            case "edit":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateExercise",
                    new SqlParameter { ParameterName = "@Id", Value = exercise.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = exercise.ExerciseName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    //new SqlParameter { ParameterName = "@Description", Value = exercise.Description, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar },
                    //new SqlParameter { ParameterName = "@TypeId", Value = int.Parse(exercise.Type), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    //new SqlParameter { ParameterName = "@UnitId", Value = int.Parse(exercise.Unit), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@CategoryId", Value = int.Parse(exercise.Category), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@IsActive", Value = exercise.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                    );
                break;
            case "del":
                break;

        }
        return Json("Success Save");
    }

    [HttpPost]
    public JsonResult SetRoutine(Routine routine, string oper, string Id, string Category, string ExerciseName, string Value, string sortOrder)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        switch (oper)
        {
            case "add":
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
                int documentId = int.Parse(nameValueCollection["id"]);
                Document document = new Document(documentId);
                Document parentDocument = new Document(document.ParentId);

                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertRoutine",
                    new SqlParameter { ParameterName = "@Id", Value = routine.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@DocumentId", Value = parentDocument.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ExerciseId", Value = int.Parse(ExerciseName), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@MemberId", Value = Convert.ToInt32(parentDocument.getProperty("member").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@TrainerId", Value = Convert.ToInt32(parentDocument.getProperty("trainer").Value), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@WorkoutId", Value = document.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Value", Value = Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal }
                    );
                break;
            case "edit":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateRoutine",
                    new SqlParameter { ParameterName = "@Id", Value = Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@ExerciseId", Value = int.Parse(ExerciseName), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Value", Value = Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Decimal }
                    );
                break;
            case "del":
                break;

        }
        return Json("Success Save");
    }

    [HttpPost]
    public JsonResult SetDevice(Device device, string oper)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        switch (oper)
        {
            case "add":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertDevice",
                    new SqlParameter { ParameterName = "@Id", Value = device.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@PlatformId", Value = int.Parse(device.Platform), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = device.DeviceName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 }
                    );
                break;
            case "edit":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateDevice",
                    new SqlParameter { ParameterName = "@Id", Value = device.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                   new SqlParameter { ParameterName = "@PlatformId", Value = int.Parse(device.Platform), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Name", Value = device.DeviceName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@IsActive", Value = device.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                    );
                break;
            case "del":
                break;

        }
        return Json("Success Save");
    }

    [HttpPost]
    public JsonResult SetNotification(PushNotification notification, string oper)
    {
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);

        switch (oper)
        {
            case "add":
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
                int memberId = int.Parse(nameValueCollection["id"]);
                Member member = new Member(memberId);
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertNotification",
                    new SqlParameter { ParameterName = "@Id", Value = notification.Id, Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@MemberId", Value = member.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@DeviceId", Value = int.Parse(notification.Device), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
                    );
                break;
            case "edit":
                SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "UpdateNotification",
                    new SqlParameter { ParameterName = "@Id", Value = notification.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@Token", Value = notification.Token, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@DeviceId", Value = int.Parse(notification.Device), Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "@IsActive", Value = notification.IsActive, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Bit }
                    );
                break;
            case "del":
                break;

        }
        return Json("Success Save");
    }

    [HttpPost]
    public JsonResult InsertTemplateWorkout(string name)
    {
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
        int id = int.Parse(nameValueCollection["id"]);
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlHelper.ExecuteNonQuery(cn, CommandType.StoredProcedure, "InsertTemplateWorkout",
            new SqlParameter { ParameterName = "@ObjectId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@Name", Value = name, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 50 }
            );
        return Json("Success");
    }

    [HttpPost]
    public JsonResult GetTemplate(int currentPage, int pagesize, string sortColumn, string sortOrder, bool isSearch, string searchField, string searchValue, string searchOper, string filters)
    {
        IEnumerable<TemplateWorkout> templates;
        User user = umbraco.BusinessLogic.User.GetCurrent();
        if (isSearch)
        {
            List<string> whereFilter = new List<string>();
            dynamic filter = JsonConvert.DeserializeObject(filters);
            foreach (var r in filter.rules)
            {
                whereFilter.Add(GetFilter((string)r.field, (string)r.op, (string)r.data));
            }
            string search = string.Join((string)filter.groupOp, whereFilter);
            templates = SelectTemplateByTrainerId(string.Format("SELECT  dbo.TemplateWorkout.Id, dbo.TemplateWorkout.Name, dbo.TemplateWorkout.TrainerId, dbo.TemplateWorkout.ExerciseId, dbo.Exercise.Name, dbo.Exercise.Description, dbo.Exercise.TypeId, dbo.Exercise.UnitId, dbo.Exercise.CategoryId FROM dbo.TemplateWorkout INNER JOIN dbo.Exercise ON dbo.TemplateWorkout.ExerciseId = dbo.Exercise.Id WHERE {0} AND dbo.TemplateWorkout.TrainerId = {1} ORDER BY {2} {3}", search, user.Id, sortColumn, sortOrder));
        }
        else
        {
            templates = SelectTemplateByTrainerId(string.Format("SELECT  dbo.TemplateWorkout.Id, dbo.TemplateWorkout.Name, dbo.TemplateWorkout.TrainerId, dbo.TemplateWorkout.ExerciseId, dbo.Exercise.Name, dbo.Exercise.Description, dbo.Exercise.TypeId, dbo.Exercise.UnitId, dbo.Exercise.CategoryId FROM dbo.TemplateWorkout INNER JOIN dbo.Exercise ON dbo.TemplateWorkout.ExerciseId = dbo.Exercise.Id WHERE dbo.TemplateWorkout.TrainerId = {0} ORDER BY {1} {2}", user.Id, sortColumn, sortOrder));
        }

        List<TemplateWorkout> result = templates.Skip((currentPage - 1) * pagesize).Take(pagesize).ToList();
        var jsonData = new
        {
            page = currentPage,
            records = templates.Count(),
            total = (int)Math.Ceiling((float)templates.Count() / pagesize),
            root = (result.Select(item => new
            {
                id = item.Id,
                cell =
                                          new[]
                                                      {
                                                          item.Id.ToString(),
                                                          //item.TemplateName,
                                                          //item.Exercise.Category,
                                                          //item.Exercise.ExerciseName,
                                                          //item.Exercise.Type,
                                                          //item.Exercise.Unit
                                                      }
            })).ToArray()
        };
        return Json(jsonData);
    }

    [HttpGet]
    public PartialViewResult GetMeasurement()
    {
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
        int GymnastId = int.Parse(nameValueCollection["id"]);
        Measurement measurement = SelectMeasurementByGymnastId(GymnastId);
        List<string> imagesFolder = SelectImagesMeasurement(GymnastId, measurement.CreatedDate.ToString("MMddyyyy"));
        List<string> thumbails = new List<string>(imagesFolder.Where(s => s.Contains("_thumb")));
        List<string> images = new List<string>(imagesFolder.Where(s => !s.Contains("_thumb")));

        return PartialView("_Measurement", new Measurement123ViewModel { Measurement = measurement, Thumbails = thumbails, Images = images });
    }

    #region DataAccess

    private List<Exercise> SelectExercise(string cmd)
    {
        List<Exercise> exercises = new List<Exercise>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.Text, cmd);
        while (reader.Read())
        {
            exercises.Add(new Exercise
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                ExerciseName = reader.GetValue(1).ToString(),
                TrainerId = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(3)),
                CategoryId = Convert.ToInt32(reader.GetValue(3)),
                Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(3)),
                IsActive = Convert.ToBoolean(reader.GetValue(4))
            });
        }
        return exercises;
    }

    private List<Exercise> SelectExerciseByCategory(int id)
    {
        User user = umbraco.BusinessLogic.User.GetCurrent();
        List<Exercise> exercises = new List<Exercise>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        string cmd = "SelectExerciseByCategory";
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, cmd, new SqlParameter { ParameterName = "@CategoryId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@TrainerId", Value = user.Id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int }
            );
        while (reader.Read())
        {
            exercises.Add(new Exercise
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                ExerciseName = reader.GetValue(1).ToString(),
                TrainerId = reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(3)),
                CategoryId = Convert.ToInt32(reader.GetValue(3)),
                Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(3)),
                IsActive = Convert.ToBoolean(reader.GetValue(4))
            });
        }
        return exercises;
    }

    private List<Routine> SelectRoutineByWorkout(string cmd)
    {
        List<Routine> routines = new List<Routine>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.Text, cmd);
        while (reader.Read())
        {
            routines.Add(new Routine
            {
                Exercise = new Exercise
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    ExerciseName = reader.GetValue(1).ToString(),
                    Description = reader.GetValue(2).ToString(),
                    TrainerId = reader.IsDBNull(3) ? (int?)null : Convert.ToInt32(reader.GetValue(3)),
                    TypeId = Convert.ToInt32(reader.GetValue(4)),
                    Type = UmbracoCustom.PropertyValue(UmbracoType.Type, reader.GetValue(4)),
                    UnitId = Convert.ToInt32(reader.GetValue(5)),
                    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(5)),
                    CategoryId = Convert.ToInt32(reader.GetValue(6)),
                    Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(6)),
                    IsActive = Convert.ToBoolean(reader.GetValue(7)),
                },
                Id = Convert.ToInt32(reader.GetValue(8)),
                //Value = Convert.ToDecimal(reader.GetValue(9).ToString()),
                StateId = reader.IsDBNull(10) ? (int?)null : Convert.ToInt32(reader.GetValue(10)),
                State = UmbracoCustom.PropertyValue(UmbracoType.State, reader.GetValue(10)),
                SortOrder = Convert.ToByte(reader.GetValue(11))
            });
        }
        return routines;
    }

    private List<Device> SelectDevice(string cmd)
    {
        List<Device> devices = new List<Device>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.Text, cmd);
        while (reader.Read())
        {
            devices.Add(new Device
            {
                Id = int.Parse(reader.GetValue(0).ToString()),
                PlatformId = int.Parse(reader.GetValue(1).ToString()),
                Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(1)),
                DeviceName = reader.GetValue(2).ToString(),
                IsActive = bool.Parse(reader.GetValue(3).ToString())
            });
        }
        return devices;
    }

    private List<PushNotification> SelectDeviceByMember(string cmd)
    {
        List<PushNotification> notifications = new List<PushNotification>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.Text, cmd);
        while (reader.Read())
        {
            notifications.Add(new PushNotification
            {
                Id = int.Parse(reader.GetValue(0).ToString()),
                MemberId = int.Parse(reader.GetValue(1).ToString()),
                Token = reader.GetValue(2).ToString(),
                DeviceId = int.Parse(reader.GetValue(3).ToString()),
                IsActive = bool.Parse(reader.GetValue(4).ToString()),
                Device = reader.GetValue(5).ToString(),
                PlatformId = int.Parse(reader.GetValue(6).ToString()),
                Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(6))
            });
        }
        return notifications;
    }

    private List<Device> SelectDeviceByPlatform(int id)
    {
        List<Device> devices = new List<Device>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        string cmd = "SelectDeviceByPlatform";
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, cmd, new SqlParameter { ParameterName = "@PlatformId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            devices.Add(new Device
            {
                Id = int.Parse(reader.GetValue(0).ToString()),
                PlatformId = int.Parse(reader.GetValue(1).ToString()),
                Platform = UmbracoCustom.PropertyValue(UmbracoType.Platform, reader.GetValue(1)),
                DeviceName = reader.GetValue(2).ToString(),
                IsActive = bool.Parse(reader.GetValue(3).ToString())
            });
        }
        return devices;
    }

    private List<TemplateWorkout> SelectTemplateByTrainerId(string cmd)
    {
        List<TemplateWorkout> templates = new List<TemplateWorkout>();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);

        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.Text, cmd);
        while (reader.Read())
        {
            templates.Add(new TemplateWorkout
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                //TemplateName = reader.GetValue(1).ToString(),
                //Exercise = new Exercise
                //{
                //    TrainerId = Convert.ToInt32(reader.GetValue(2)),
                //    Id = Convert.ToInt32(reader.GetValue(3)),
                //    ExerciseName = reader.GetValue(4).ToString(),
                //    Description = reader.GetValue(5).ToString(),
                //    TypeId = Convert.ToInt32(reader.GetValue(6)),
                //    Type = UmbracoCustom.PropertyValue(UmbracoType.Type, reader.GetValue(6)),
                //    UnitId = Convert.ToInt32(reader.GetValue(7)),
                //    Unit = UmbracoCustom.PropertyValue(UmbracoType.Unit, reader.GetValue(7)),
                //    CategoryId = Convert.ToInt32(reader.GetValue(8)),
                //    Category = UmbracoCustom.PropertyValue(UmbracoType.Category, reader.GetValue(8))
                //}
            });

        }
        return templates;
    }

    private Measurement SelectMeasurementByGymnastId(int id)
    {
        Measurement measurement = new Measurement();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastId", new SqlParameter { ParameterName = "@GymnastId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int });
        while (reader.Read())
        {
            measurement = new Measurement
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                GymnastId = Convert.ToInt32(reader.GetValue(1)),
                Weight = Convert.ToDecimal(reader.GetValue(2)),
                Neck = Convert.ToDecimal(reader.GetValue(3)),
                Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                RightArm = Convert.ToDecimal(reader.GetValue(5)),
                LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                Chest = Convert.ToDecimal(reader.GetValue(7)),
                BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                Hips = Convert.ToDecimal(reader.GetValue(9)),
                RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                Arm = Convert.ToDecimal(reader.GetValue(14)),
                Waist = Convert.ToDecimal(reader.GetValue(15)),
                Thigh = Convert.ToDecimal(reader.GetValue(16)),
                Back = Convert.ToDecimal(reader.GetValue(17)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(18))
            };
        }
        return measurement;
    }

    private Measurement SelectMeasurementByGymnastIdByDate(int id, DateTime from, DateTime to)
    {
        Measurement measurement = new Measurement();
        string cn = UmbracoCustom.GetParameterValue(UmbracoType.Connection);
        SqlDataReader reader = SqlHelper.ExecuteReader(cn, CommandType.StoredProcedure, "SelectMeasurementByGymnastIdByDate",
            new SqlParameter { ParameterName = "@GymnastId", Value = id, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.Int },
            new SqlParameter { ParameterName = "@FromDate", Value = from, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
            new SqlParameter { ParameterName = "@ToDate", Value = to, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime }
            );
        while (reader.Read())
        {
            measurement = new Measurement
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                GymnastId = Convert.ToInt32(reader.GetValue(1)),
                Weight = Convert.ToDecimal(reader.GetValue(2)),
                Neck = Convert.ToDecimal(reader.GetValue(3)),
                Shoulders = Convert.ToDecimal(reader.GetValue(4)),
                RightArm = Convert.ToDecimal(reader.GetValue(5)),
                LeftArm = Convert.ToDecimal(reader.GetValue(6)),
                Chest = Convert.ToDecimal(reader.GetValue(7)),
                BellyButton = Convert.ToDecimal(reader.GetValue(8)),
                Hips = Convert.ToDecimal(reader.GetValue(9)),
                RightThigh = Convert.ToDecimal(reader.GetValue(10)),
                LeftThigh = Convert.ToDecimal(reader.GetValue(11)),
                RightCalf = Convert.ToDecimal(reader.GetValue(12)),
                LeftCalf = Convert.ToDecimal(reader.GetValue(13)),
                Arm = Convert.ToDecimal(reader.GetValue(14)),
                Waist = Convert.ToDecimal(reader.GetValue(15)),
                Thigh = Convert.ToDecimal(reader.GetValue(16)),
                Back = Convert.ToDecimal(reader.GetValue(17)),
                CreatedDate = Convert.ToDateTime(reader.GetValue(18))
            };
        }
        return measurement;
    }

    private List<string> SelectImagesMeasurement(int id, string date)
    {
        //http://localhost/Photo/1159/02252013/fruits2.jpg
        //D:\Photo\1159\02252013\fruits10_thumb.jpg
        List<string> result = new List<string>();
        string datePath = Path.Combine(UmbracoCustom.GetParameterValue(UmbracoType.Photo), id.ToString(), date);
        if (Directory.Exists(datePath))
        {
            string[] images = Directory.GetFiles(datePath);
            foreach (string image in images)
            {
                int position = image.IndexOf("Photo");
                result.Add("http://localhost/" + image.Substring(position).Replace(@"\", "/"));
            }
        }
        return result;
    }

    #endregion
}

public class Measurement123ViewModel
{
    public Measurement Measurement { get; set; }
    public List<string> Images { get; set; }
    public List<string> Thumbails { get; set; }
}