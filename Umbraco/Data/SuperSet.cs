using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class SuperSet
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public int? Reps { get; set; }
    public int? Sets { get; set; }
    //public decimal? Resistance { get; set; }
    public int ResistanceId { get; set; }
    public string Resistance { get; set; }
    public int? UnitId { get; set; }
    public string Unit { get; set; }
    public string Note { get; set; }
    public DateTime CreatedDate { get; set; }
}

