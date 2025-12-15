import re
from urllib.parse import quote_plus

def slugify(name: str) -> str:
    return re.sub(r'[^a-z0-9-]', '', name.lower().replace(' ', '-'))

def build_walkthrough_suggestions(game_name: str) -> list[dict]:
    q = quote_plus(game_name.strip())
    slug = slugify(game_name)

    suggestions = [
        {"source": "IGN", "url": f"https://www.ign.com/wikis/{slug}"},
        {"source": "Neoseeker", "url": f"https://www.neoseeker.com/{slug}/"},
        {"source": "TrueAchievements", "url": f"https://www.trueachievements.com/game/{slug}/walkthrough"},
        {"source": "GameSpot", "url": f"https://www.gamespot.com/games/{slug}/guides/"},
    ]
    return suggestions