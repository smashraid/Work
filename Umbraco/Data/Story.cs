using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


   public class Story
    {
       public int Id { get; set; }
       public int ActionId { get; set; }
       public string Action { get; set; }
       public int TrainingId { get; set; }
       public string Training { get; set; }
       public int? UnitId { get; set; }
       public string Unit { get; set; }
       public int ObjectId { get; set; }
       public int ObjectType { get; set; }
       public decimal Value { get; set; }
       public int TypeId { get; set; }
       public string Type { get; set; }
       public int UserId { get; set; }
       public int UserType { get; set; }
       public string Note { get; set; }
       public DateTime CreatedDate { get; set; }
       public DateTime? UpdatedDate { get; set; }
    }

