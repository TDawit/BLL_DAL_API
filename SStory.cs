using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSBOL
{
  
    public class Story

    {
       [Key] // to show the Primary key
        public int SSId { get; set; }
       [Required]
        public string SSTitle { get; set; }
       [Required]
        public string SSDescription { get; set; }
        

        public DateTime CreatedOn { get; set; }

        public bool IsApproved { get; set; }

        public int Like { get; set; }
        public int Dislike { get; set; }

     [ForeignKey("SSUser")]
        public string Id { get; set; } // we use the foreign key to see who posted story 

        public virtual SSUser SSUser { get; set; } // Navigation Property 
    }

    }

