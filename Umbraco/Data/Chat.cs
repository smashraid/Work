using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Chat
{
    public int Id { get; set; }
    public int GymnastId { get; set; }
    public string Subject { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public string User { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsOwner { get; set; }
    public bool IsRead { get; set; }
    public Talk LastTalk { get; set; }
}

