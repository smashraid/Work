using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class EmailMessage
{
    public EmailMessage()
    {
        
    }

    public int Id { get; set; }
    public int TrainerId { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public int ObjectId { get; set; }
    public int ObjectType { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
    public DateTime SendDate { get; set; }
}

