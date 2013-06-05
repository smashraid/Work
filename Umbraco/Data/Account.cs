using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Account
/// </summary>
public class Account
{
    public Account()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public int Id { get; set; }
    //[Required]
    public string Name { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
    //[Required]
    //[DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    //[Required]
    //[Display(Name = "User")]
    public string LoginName { get; set; }
    public string Password { get; set; }

    #region UserInformation

    public string Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    //[Display(Name = "Zip Code")]
    public string ZipCode { get; set; }
    //[DataType(DataType.MultilineText)]
    public string Address { get; set; }
    public string Phone { get; set; }

    #endregion

    #region Audit

    public bool IsActive { get; set; }

    #endregion

    #region SocialMedia

    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Google { get; set; }

    #endregion

    #region GymnastInformation

    public int Gymnast { get; set; }
    public decimal? Height { get; set; }
    public decimal? StartWeight { get; set; }
    public decimal? GoalWeight { get; set; }
    public bool UseMetric { get; set; }
    public int TrainerId { get; set; }

    #endregion

    #region Notification

    public bool EmailAlert { get; set; }

    #endregion
}