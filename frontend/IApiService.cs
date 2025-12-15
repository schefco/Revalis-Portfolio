using Revalis.Models;

namespace Revalis
{
    public interface IApiService
    {
        Task<List<GameDTO>> GetGamesAsync(int page, int pageSize);
        Task<GameDTO> GetGameByIdAsync(int id);
        Task<GameDTO> AddGameAsync(GameDTO game);
        Task DeleteGameAsync(int id);
        Task<IEnumerable<GameDTO>> SearchGamesAsync(string query, int? genre, int? platform, double? minRating, double? maxRating, string sort, int limit, int offset);
    }
}
