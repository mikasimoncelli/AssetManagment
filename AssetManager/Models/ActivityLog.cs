namespace AssetManager.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign Key

        public User User { get; set; } // Navigation Property

        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
