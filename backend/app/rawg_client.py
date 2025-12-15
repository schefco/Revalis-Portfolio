import httpx
from app.config import API_KEY

BASE_URL = "https://api.rawg.io/api"

async def fetch_games(query: str, page_size: int = 20, page: int = 1):
    params = {
        "key": API_KEY,
        "search": query,
        "page_size": page_size,
        "page": page
    }

    async with httpx.AsyncClient() as client:
        # First call: search list
        response = await client.get(f"{BASE_URL}/games", params=params)
        response.raise_for_status()
        data = response.json().get("results", [])

        detailed = []
        for g in data:
            game_id = g.get("id")
            if not game_id:
                continue

            # Second call: game detail
            detail_resp = await client.get(f"{BASE_URL}/games/{game_id}", params={"key": API_KEY})
            detail_resp.raise_for_status()
            detail = detail_resp.json()

            detailed.append(detail)

        return detailed


