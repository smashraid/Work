using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Device
/// </summary>
public class Device
{
	public Device()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public int Id { get; set; }
    public string Platform { get; set; }
    public int PlatformId { get; set; }
    public string DeviceName { get; set; }
    public bool IsActive { get; set; }
}