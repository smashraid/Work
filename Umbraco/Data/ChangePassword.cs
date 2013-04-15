using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public    class ChangePassword
    {
        //[Required]
        //[Display(Name = "User")]
        public string LoginName { get; set; }
        //[Required]
        //[DataType(DataType.Password)]
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }

