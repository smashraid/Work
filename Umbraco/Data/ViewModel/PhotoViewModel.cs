using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PhotoViewModel
{
    public int Id { get; set; }
    public IEnumerable<byte[]> Photos { get; set; }
}

