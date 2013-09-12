using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InfomationViewModel
{
    public List<MeasurementViewModel> Measurements { get; set; }
    public List<Chat> RecentChats { get; set; }
    public List<Chat> ReadChats { get; set; }
    public List<Chat> UnreadChats { get; set; }
    public int NumberUnreadChat { get; set; }
    public int NumberNotViewedWorkout { get; set; }
}

