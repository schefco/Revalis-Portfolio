from audioop import reverse

import httpx
from fastapi import APIRouter, Depends, Header, HTTPException, Query
from typing import Optional

from sqlalchemy import Float
from sqlalchemy.orm import Session

from app.config import API_KEY
from app.db import SessionLocal
from app.crud import get_games, create_game, store_rawg_games, create_games_bulk
from app.schemas import GameRead, GameCreate
from app.rawg_client import fetch_games
from app.walkthroughs import build_walkthrough_suggestions
from app.models import Game

router = APIRouter()

def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

# GET

@router.get("/")
def root():
    return {
        "message": "Welcome to Revalis API",
        "docs": "/docs",
        "endpoints": {
            "list_games": "/games",
            "get_game_by_id": "/games/{game_id}",
            "add_game": "/games",
            "bulk_insert": "/games/bulk"
        }
    }

@router.get("/games", response_model=list[GameRead])
def read_games(db: Session = Depends(get_db)):
    return get_games(db)

@router.get("/genres")
async def get_genres():
    async with httpx.AsyncClient(timeout=30.0) as client:
        response = await client.get(f"https://api.rawg.io/api/genres?key={API_KEY}")
        response.raise_for_status()
        data = response.json()
        return [{"id": g["id"], "name": g["name"]} for g in data.get("results", [])]

@router.get("/platforms")
async def get_platforms():
    async with httpx.AsyncClient(timeout=30.0) as client:
        response = await client.get(f"https://api.rawg.io/api/platforms?key={API_KEY}")
        response.raise_for_status()
        data = response.json()
        return [{"id": p["id"], "name": p["name"]} for p in data.get("results", [])]

@router.get("/walkthroughs/{game_name}")
def walkthroughs_for_game(game_name: str):
    return {"game": game_name, "suggestions": build_walkthrough_suggestions(game_name)}

@router.get("/walkthroughs")
def walkthroughs_for_all(db: Session = Depends(get_db)):
    games = db.query(Game).all()
    return{
        g.name: build_walkthrough_suggestions(g.name)
        for g in games
    }

@router.get("/games/search", response_model=list[GameRead])
async def search_games(
    query: Optional[str] = Query(None, description="Search by name"),
    genre: Optional[int] = Query(None, description="Filter by genre id"),
    platform: Optional[int] = Query(None, description="Filter by platform id"),
    min_rating: Optional[float] = Query(None, description="Minimum Rating"),
    max_rating: Optional[float] = Query(None, description="Maximum Rating"),
    sort: Optional[str] = Query(None, description="Sort field (name, released, rating)"),
    limit: int = Query(20, ge=1, le=100, description="Max results per page"),
    offset: int = Query(0, ge=0, description="Number of results to skip"),
    db: Session = Depends(get_db)
):
    results: list[dict] = []

    # --- Search local DB ---
    q = db.query(Game)

    if query:
        q = q.filter(Game.name.ilike(f"%{query}%"))
    if min_rating is not None:
        q = q.filter(Game.rating >= min_rating)
    if max_rating is not None:
        q = q.filter(Game.rating <= max_rating)

    db_results = q.all()
    for g in db_results:
        results.append({
            "id": g.id,
            "name": g.name,
            "released": g.released,
            "rating": float(g.rating) if g.rating is not None else None,
            "description": g.description,
            "developer": g.developer,
            "platforms": [
                {"id": p.get("id"), "name": p.get("name"), "icon": p.get("icon")}
                for p in (g.platforms or [])
            ],
            "genres": [
                {"id": gr.get("id"), "name": gr.get("name")}
                for gr in (g.genres or [])
            ],
            "cover_image_url": g.cover_image_url
        })

    # --- Search RAWG API ---
    if query:
        rawg_data = await fetch_games(query)
        for g in rawg_data:
            results.append({
                "id": g.get("id"),
                "name": g.get("name"),
                "released": g.get("released"),
                "rating": float(g.get("rating")) if g.get("rating") is not None else None,
                "description": g.get("description_raw") or None,
                "developer": (g.get("developers")[0].get("name") if g.get("developers") else None),
                "platforms": [
                    {"id": p.get("platform", {}).get("id") or 0,
                     "name": p.get("platform", {}).get("name") or "",
                     "icon": resolve_platform_icon(p.get("platform", {}).get("name"))
                    }
                    for p in (g.get("platforms") or [])
                    if p.get("platform")
                ],
                "genres": [
                    {"id": gr.get("id") or 0, "name": gr.get("name") or ""}
                    for gr in (g.get("genres") or [])
                    if gr and gr.get("name")
                ],
                "cover_image_url": g.get("background_image") or None
            })

    # --- Deduplicate by name ---
    unique = {r["name"]: r for r in results}
    merged_results = list(unique.values())

    # --- Apply filters ---
    if genre is not None:
        merged_results = [
            r for r in merged_results
            if any(gr.get("id") == genre for gr in (r.get("genres") or []))
        ]
    if platform is not None:
        merged_results = [
            r for r in merged_results
            if any(p.get("id") == platform for p in (r.get("platforms") or []))
        ]

    # --- Sorting ---
    if sort == "rating":
        merged_results.sort(key=lambda g: (g["rating"] or 0), reverse=True)
    elif sort == "released":
        merged_results.sort(key=lambda g: (g["released"] or ""), reverse=True)
    else:
        merged_results.sort(key=lambda g: g["name"].lower())

    # --- Paging ---
    paged_results = merged_results[offset: offset + limit]

    return paged_results

@ router.get("/games/{game_id}", response_model=GameRead)
def read_game_by_id(game_id: int, db: Session = Depends(get_db)):
    game = db.query(Game).filter(Game.id == game_id).first()
    if not game:
        raise HTTPException(status_code=404, detail="Game not found")
    return game

# POST

@router.post("/games", response_model=GameRead)
def add_game(game: GameCreate, db: Session = Depends(get_db)):
    return create_game(db, game)

@router.post("/games/bulk")
def add_games_bulk(games: list[GameCreate], db: Session = Depends(get_db)):
    return create_games_bulk(db, games)

@router.post("/import", response_model=list[GameRead])
async def import_games(query: str, pages: int = 1, x_api_key: str = Header(...), db: Session = Depends(get_db)):
    if x_api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Invalid API key")

    all_games = []
    for page in range(1, pages + 1):
        rawg_data = await fetch_games(query, page_size=10, page=page)
        stored = store_rawg_games(db, rawg_data)
        all_games.extend(stored)
    return all_games

# DELETE

@router.delete("/games/{game_id}", status_code=204)
def delete_game(game_id: int, db: Session = Depends(get_db)):
    game = db.query(Game).filter(Game.id == game_id).first()
    if not game:
        raise HTTPException(status_code=404, detail="Game not found")
    db.delete(game)
    db.commit()
    return

# PUT

@router.put("/games/{game_id}", response_model=GameRead)
def update_game(game_id: int, game_update: GameCreate, db: Session = Depends(get_db)):
    game = db.query(Game).filter(Game.id == game_id).first()
    if not game:
        raise HTTPException(status_code=404, detail="Game not found")

    for field, value in game_update.dict(exclude_unset=True).items():
        setattr(game, field, value)

    db.commit()
    db.refresh(game)
    return game

# Helper to resolve platform icons
def resolve_platform_icon(name: str) -> str:
    if not name:
        return None
    name = name.lower()
    if "pc" in name or "windows" in name or "steam" in name:
        return "pc.png"
    if "xbox" in name:
        return "xbox.png"
    if "playstation" in name or "ps" in name:
        return "playstation.png"
    if "switch" in name or "nintendo" in name:
        return "switch.png"
    if "android" in name:
        return "android.png"
    if "ios" in name:
        return "ios.png"
    if "macos" in name or "mac" in name or "apple" in name:
        return "macos.png"
    return "default.png"