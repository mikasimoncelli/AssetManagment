namespace AssetManager.Models
{
    public class AssetDisposal
    {
        public string AssetDisposalID { get; set; }
        public string AssetID { get; set; }
        public string DisposalReason { get; set; }
        public string Notes { get; set; }
        public DateTime DisposalDate { get; set; }

        public Asset Asset { get; set; }

    }
}
