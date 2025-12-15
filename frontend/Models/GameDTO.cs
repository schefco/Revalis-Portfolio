using System.Text.Json.Serialization;

namespace Revalis.Models
{
    public class GameDTO : IListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Released { get; set; }
        public double? Rating { get; set; }
        public string Notes { get; set; }

        [JsonPropertyName("cover_image_url")]
        public string CoverImageUrl { get; set; }

        // Detail View Attributes
        public string Developer {  get; set; }
        public string Description { get; set; }
        public double Progress { get; set; } // percentage 0 to 100
        public List<string> Achievements { get; set; } = new();

        public List<GenreDTO> Genres { get; set; } = new();
        public List<PlatformDTO> Platforms { get; set; } = new();
        public string GenresDisplay => Genres != null && Genres.Any() ? string.Join(", ", Genres.Select(g => g.Name)) : string.Empty;
        public List<WalkthroughLinkDTO> Walkthroughs { get; set; } = new();
    }

    public class GenresResponse
    {
        public List<GenreDTO> Results { get; set; }
    }

    public class GenreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PlatformsResponse
    {
        public List<PlatformDTO> Results { get; set; }
    }

    public class PlatformDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class WalkthroughLinkDTO
    {
        public string Source { get; set; }
        public string Url { get; set; }
    }
}
