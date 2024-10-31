namespace AssetManager.Models
{
    public class AssetDamage
    {
        public int AssetDamageID { get; set; }
        public int AssetID { get; set; }
        public string DamageDescription { get; set; }
        public string Notes { get; set; }
        public string RepairStatus { get; set; }
        public DateTime DateReported { get; set; }

        public DateTime RepairDate { get; set; }

        public Asset Asset { get; set; }
    }

}
