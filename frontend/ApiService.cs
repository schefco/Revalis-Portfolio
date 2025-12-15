using Revalis.Models;
using Revalis.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace Revalis
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");
        }

        public async Task<List<GameDTO>> SearchGamesAsync(string query, int limit = 20, int offset = 0)
        {
            string url = $"games/search?query={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";
            return await _httpClient.GetFromJsonAsync<List<GameDTO>>(url);
        }

        public async Task<List<GameDTO>> GetGamesAsync(int page, int pageSize)
        {
            int offset = (page - 1) * pageSize;
            string url = $"games/search?limit={pageSize}&offset={offset}";
            return await _httpClient.GetFromJsonAsync<List<GameDTO>>(url);
        }

        public async Task<GameDTO> GetGameByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<GameDTO>($"games/{id}");
        }

        public async Task<GameDTO> AddGameAsync(GameDTO game)
        {
            var response = await _httpClient.PostAsJsonAsync("games", game);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameDTO>();
        }

        public async Task<GameDTO> UpdateGameAsync(GameDTO game)
        {
            var id = Convert.ToString(game.Id);
            var response = await _httpClient.PutAsJsonAsync($"games/{id}", game);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameDTO>();
        }

        public async Task DeleteGameAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"games/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<GameDTO>> SearchGamesAsync(
            string query = null,
            int? genreId = null,
            int? platformId = null,
            double? minRating = null,
            double? maxRating = null,
            string sort = null,
            int limit = 20,
            int offset = 0)
        {

            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(query)) queryParams.Add($"query={Uri.EscapeDataString(query)}");
            if (genreId.HasValue && genreId.Value != 0) queryParams.Add($"genre={genreId.Value}");
            if (platformId.HasValue && platformId.Value != 0) queryParams.Add($"platform={platformId.Value}");
            if (minRating.HasValue) queryParams.Add($"min_rating={minRating.Value}");
            if (maxRating.HasValue) queryParams.Add($"max_rating={maxRating.Value}");
            if (!string.IsNullOrWhiteSpace(sort)) queryParams.Add($"sort={sort}");
            queryParams.Add($"limit={limit}");
            queryParams.Add($"offset={offset}");

            var url = $"games/search";
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            return await _httpClient.GetFromJsonAsync<IEnumerable<GameDTO>>(url);
        }

        public async Task<List<GenreDTO>> GetGenresAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<GenreDTO>>("genres");
        }

        public async Task<List<PlatformDTO>> GetPlatformsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PlatformDTO>>("platforms");
        }

        public async Task<List<Revalis.Models.WalkthroughLinkDTO>> GetWalkthroughForGameAsync(string gameName)
        {
            var response = await _httpClient.GetAsync($"/walkthroughs/{Uri.EscapeDataString(gameName)}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<WalkthroughResponse>();
            return result?.Suggestions ?? new List<WalkthroughLinkDTO>();
        }

        public class WalkthroughResponse
        {
            public string Game {  get; set; }
            public List<Revalis.Models.WalkthroughLinkDTO> Suggestions { get; set; }
        }
    }
}
