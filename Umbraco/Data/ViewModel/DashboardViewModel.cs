using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class DashboardViewModel
{
    public List<MeasurementViewModel> Measurements { get; set; }
    public List<Chat> Chats { get; set; }
    public List<Chat> UnreadChats { get; set; }
    public List<Topic> Topics { get; set; }
    public int NumberChat { get; set; }
    public int NumberUnreadChat { get; set; }
    public int NumberTopic { get; set; }
    public int NumberWorkout { get; set; }
    public int NumberViewedWorkout { get; set; }
    public int NumberNotViewedWorkout { get; set; }
    public int NumberCompleteWorkout { get; set; }
    public int NumberNotCompleteWorkout { get; set; }
    public Account Account { get; set; }
    public string Avatar { get; set; }
}

