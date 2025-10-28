namespace PodcastWebApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Episode> FeaturedEpisodes { get; set; } = new();
        public List<Episode> LatestEpisodes { get; set; } = new();
        public List<Podcast> FeaturedPodcasts { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public User User { get; set; } = new();
        public List<Podcast> UserPodcasts { get; set; } = new();
        public List<Subscription> UserSubscriptions { get; set; } = new();
        public List<Episode> RecentEpisodes { get; set; } = new();
        public List<Comment> RecentComments { get; set; } = new();
        public int TotalViews { get; set; }
        public int TotalComments { get; set; }
    }

    public class PodcasterDashboardViewModel
    {
        public int TotalViews { get; set; }
        public int TotalEpisodes { get; set; }
        public int TotalComments { get; set; }
        public int TotalSubscribers { get; set; }
        public List<Podcast> Podcasts { get; set; } = new List<Podcast>();
    }

    public class EpisodeDetailViewModel
    {
        public Episode Episode { get; set; } = new();
        public Podcast Podcast { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public bool IsSubscribed { get; set; }
        public bool CanEdit { get; set; }
    }

    public class SearchResultsViewModel
    {
        public string Query { get; set; } = "";
        public List<Episode> Episodes { get; set; } = new();
        public List<Podcast> Podcasts { get; set; } = new();
        public string FilterType { get; set; } = "";
        public string SortBy { get; set; } = "";
    }

    public class AnalyticsViewModel
    {
        public List<Episode> TopEpisodes { get; set; } = new();
        public Dictionary<string, int> ViewsByDate { get; set; } = new();
        public int TotalViews { get; set; }
        public int TotalComments { get; set; }
        public int TotalSubscribers { get; set; }
    }
}