using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Notification
/// </summary>
public class PushNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public PushNotification()
    {
        
    }

    /// <summary>
    /// Type: int
    /// Description: Identifier of the class
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type: int
    /// Description: Umbraco MemberId
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// Type: string
    /// Description: Identifier from the device
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Type: int
    /// Description: Device identifier
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Type: string
    /// Description: Device name
    /// </summary>
    public string Device { get; set; }
    
    /// <summary>
    /// Type: int
    /// Description: Platform identifier
    /// </summary>
    public int PlatformId { get; set; }

    /// <summary>
    /// Type: int
    /// Description: Platform name
    /// </summary>
    public string Platform { get; set; }
    public bool IsActive { get; set; }
}

