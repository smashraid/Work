using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Summary description for Message
/// </summary>
public class PushMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public PushMessage()
    {
    }

    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Token { get; set; }
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public int ObjectId { get; set; }
    public int ObjectType { get; set; }
    public string Message { get; set; }
    public DateTime SendDate { get; set; }
    public string Type { get; set; }

    public string Title { get; set; }
    public string Alert { get; set; }
    public int Badge { get; set; }
    public string Sound { get; set; }
}

