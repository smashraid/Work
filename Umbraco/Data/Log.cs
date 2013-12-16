using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Log
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? UserType { get; set; }
    public int? ObjectId { get; set; }
    public int? ObjectType { get; set; }
    public string Endpoint { get; set; }
    public string RequestObject { get; set; }
    public string ExceptionMessage { get; set; }
    public string ExceptionType { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    public string Operation { get; set; }
    public DateTime CreatedDate { get; set; }
}

