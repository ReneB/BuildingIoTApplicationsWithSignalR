using Server.Models;

namespace Server.Services {
    public class AnnouncementLog {
        private readonly List<Announcement> pastAnnouncements = new();

        public void RegisterAsDone(Announcement announcement) {
            pastAnnouncements.Add(announcement);
        }

        public bool HasBeenBroadcast(Announcement announcement) {
            return pastAnnouncements.Any(a => a.Id == announcement.Id);
        }
    }
}