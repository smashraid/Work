using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Photo
/// </summary>
public class Photo
{
	public Photo()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public string FileName { get; set; }
    //public string ContentType { get; set; }
    //public long ContentLength { get; set; }
    //public MemoryStream InputStream { get; set; }
    public byte[] PhotoArray { get; set; }
    public string Base64 { get; set; }
}