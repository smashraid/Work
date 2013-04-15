using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

/// <summary>
/// Summary description for Gymnast
/// </summary>
public class Gymnast
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Gymnast()
    {
    }

    /// <summary>
    /// Identifier of the class
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id from the parent node in Umbraco
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Type: string
    /// Description: Gymnast name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type: int
    /// Description: Umbraco MemberId
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// Type: int
    /// Description: Id of the trainer
    /// </summary>
    public int? TrainerId { get; set; }

    /// <summary>
    /// Type: string
    /// Description: Trainer name 
    /// </summary>
    public string Trainer { get; set; }

    //public Gymnast(int id, int parentId, string name, int memberId, Property bodyWeights)
    //{
    //    Id = id;
    //    ParentId = parentId;
    //    Name = name;
    //    MemberId = memberId;
    //    XDocument xmlDocument = XDocument.Parse((string)bodyWeights.Value);
    //    BodyWeights = (from x in xmlDocument.Descendants("item")
    //                   select new BodyWeight
    //                   {
    //                       Id = int.Parse(x.Attribute("id").Value),
    //                       SortOrder = int.Parse(x.Attribute("sortOrder").Value),
    //                       Weight = decimal.Parse(x.Element("weight").Value),
    //                       Date = DateTime.Parse(x.Element("date").Value)
    //                   }).ToList();
    //}

}

//public class BodyWeight
//{
//    public int Id { get; set; }
//    public int SortOrder { get; set; }
//    public decimal Weight { get; set; }
//    public DateTime Date { get; set; }

    
//}