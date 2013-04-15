using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


 public   class Login
    {
        //[Required]
        //[Display(Name = "User")]
        public string LoginName { get; set; }
        //[Required]
        //[DataType(DataType.Password)]
        public string Password { get; set; }
        //[Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

