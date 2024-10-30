using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManager.Models
{
    public class CheckedOutAsset
    {
        [Key]
        public int CheckedOutID { get; set; }
        public int AssetID { get; set; }
        public Asset Asset { get; set; } // Navigation property   
        public int UserID { get; set; }
        public User User { get; set; } // Navigation property
        public DateTime DateLentOut { get; set; }
   
        public DateTime? DateReturned { get; set; }
        public DateTime? DueDate { get; set; }

    }
}
