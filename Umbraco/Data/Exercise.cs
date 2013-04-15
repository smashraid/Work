using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Exercise
/// </summary>
public class Exercise
{
    public Exercise()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Exercise(int id, string exerciseName, string description, int typeId, int unitId, int categoryId, bool isActive)
    {
        Id = id;
        ExerciseName = exerciseName;
        Description = description;
        TypeId = typeId;
        UnitId = unitId;
        CategoryId = categoryId;
        IsActive = isActive;
    }

    public int Id { get; set; }
    public string ExerciseName { get; set; }
    public string Description { get; set; }
    public int? TrainerId { get; set; }
    public int TypeId { get; set; }
    public string Type{ get; set; }
    public int UnitId { get; set; }
    public string Unit{ get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
}