using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManager.Models
{
    public class Asset
    {
        public int AssetID { get; set; }
        public string AssetNumber { get; set; }
        public string SerialNumber { get; set; }
        public string EquipmentType { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }

        // Foreign key for Office
        public int OfficeID { get; set; }

        // Navigation property for Office
      
        public Office Office { get; set; }
    }

}
