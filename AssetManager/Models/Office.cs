using System.ComponentModel.DataAnnotations;

namespace AssetManager.Models
{
    public class Office
    {
        [Key]
        public int OfficeID { get; set; }

        public required string OfficeName { get; set; }
        public required string Location { get; set; }
    }
}
