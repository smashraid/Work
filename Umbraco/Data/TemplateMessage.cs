﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TemplateMessage
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string MessageType { get; set; }
    public int ObjectId { get; set; }
    public int ObjectType { get; set; }
    public int Badge { get; set; }
}