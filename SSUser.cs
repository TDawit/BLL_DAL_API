using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SSBOL
{
    public class SSUser: IdentityUser //SSuser is a user that inhertinces from IdentityUser we can extend 
    {
     
        public DateTime DOB { get; set; }
        public string ProfilePicPath { get; set; }

        public virtual IEnumerable<Story> Stories { get; set; } //A user can post a number of stories 
    }
}


