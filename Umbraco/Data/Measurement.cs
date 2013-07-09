using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


  public class Measurement
    {
      public int Id { get; set; }
      public int GymnastId { get; set; }
      public decimal Weight { get; set; }
      public decimal Neck { get; set; }
      public decimal Shoulders { get; set; }
      public decimal RightArm { get; set; }
      public decimal LeftArm { get; set; }
      public decimal Chest { get; set; }
      public decimal BellyButton { get; set; }
      public decimal Hips { get; set; }
      public decimal RightThigh { get; set; }
      public decimal LeftThigh { get; set; }
      public decimal RightCalf { get; set; }
      public decimal LeftCalf { get; set; }
      public decimal Arm { get; set; }
      public decimal Waist { get; set; }
      public decimal Thigh { get; set; }
      public decimal Back { get; set; }
      public DateTime CreatedDate { get; set; }
      public bool HasPhotos { get; set; }
    }

