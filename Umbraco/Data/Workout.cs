using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Workout
/// </summary>
public class Workout
{
	public Workout()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public int Id { get; set; }
    public int ParentId { get; set; }
    public int MemberId { get; set; }
    public string Name { get; set; }
    public DateTime? DateScheduled { get; set; }
    public DateTime? DateCompleted { get; set; }
    public int? TimeSpent { get; set; }
    public string Description { get; set; }
    public int? RateId { get; set; }
    public string Rate { get; set; }
    public string Note { get; set; }
    public int? StateId { get; set; }
    public string State { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int SortOrder { get; set; }
}