using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Topic
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public string User { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsOwner { get; set; }
}

