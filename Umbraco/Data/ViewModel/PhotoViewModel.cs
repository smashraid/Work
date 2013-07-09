using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PhotoViewModel
{
    public int Id { get; set; }
    public IEnumerable<Photo> Photos { get; set; }
}

