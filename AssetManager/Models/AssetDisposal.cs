namespace AssetManager.Models
{
    public class AssetDisposal
    {
        public int AssetDisposalID { get; set; }
        public int AssetID { get; set; }
        public string DisposalReason { get; set; }

        public string DisposalStatus { get; set; }

        public string DisposalDescription { get; set; }

        public string? Notes { get; set; }
        public DateTime DateDisposed { get; set; }

        public Asset Asset { get; set; }

    }
}
