using AssetManager.Models;

namespace AssetManager.Helpers
{
    public class ActivityLogger
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public void LogActivity(int userId, ActivityType activityType, string description, DateTime createdAt)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                ActivityType = activityType.ToString(),
                Description = description,
                CreatedAt = createdAt
            };

            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
