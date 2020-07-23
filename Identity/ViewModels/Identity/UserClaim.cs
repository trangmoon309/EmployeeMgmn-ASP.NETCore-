using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Views.Administration
{
    public class UserClaim
    { //Show từng Claim trong list
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }
}
