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

      
        public int OfficeID { get; set; }

       
      
        public Office Office { get; set; }
    }

}
