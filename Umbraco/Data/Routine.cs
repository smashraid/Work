using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Routine
/// </summary>
public class Routine
{
    public Routine()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public int UserType { get; set; }
    public int ObjectId { get; set; }
    public int ObjectType { get; set; }
    public int? StateId { get; set; }
    public string State { get; set; }
    public Exercise Exercise { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? CanceledDate { get; set; }
    public int SortOrder { get; set; }
    public int? Reps { get; set; }
    public int? Sets { get; set; }
    public decimal? Resistance { get; set; }
    public int? UnitId { get; set; }
    public string Unit { get; set; }
    public string Note { get; set; }
}