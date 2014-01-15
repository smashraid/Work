using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Package
{
    public string Id { get; set; }
    public int DocumentId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool State { get; set; }
    public IEnumerable<Workout> Workouts { get; set; }
}