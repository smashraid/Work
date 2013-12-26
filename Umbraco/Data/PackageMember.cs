using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PackageMember
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int PackageId { get; set; }
    public bool WasBought { get; set; }
    public string Receipt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Package Package { get; set; }
}