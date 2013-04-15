using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Post
{
    public int Id { get; set; }
    public string Message { get; set; }
    public int TopicId { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public string User { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsOwner { get; set; }
    public bool ReportAbuse { get; set; }
}

