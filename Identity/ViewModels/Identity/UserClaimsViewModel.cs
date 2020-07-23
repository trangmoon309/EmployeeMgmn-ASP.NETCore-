using Identity.Views.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.ViewModels.Identity
{ //Show list các claims của 1 user
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }

        public List<UserClaim> Claims { get; set; }
        public string UserId { get; set; }
    }
}
