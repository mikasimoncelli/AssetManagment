using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManager.Models
{
    public class CheckedOutAsset
    {
        [Key]
        public int CheckedOutID { get; set; }

        [ForeignKey("Asset")]
        public int AssetID { get; set; }
        public Asset Asset { get; set; } // Navigation property

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User User { get; set; } // Navigation property

        [DataType(DataType.Date)]
        public DateTime DateLentOut { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateReturned { get; set; }
    }
}
