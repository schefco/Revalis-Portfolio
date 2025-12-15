from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from starlette.exceptions import HTTPException
from . import models, schemas

def create_game(db: Session, game: schemas.GameCreate):
    existing = db.query(models.Game).filter(models.Game.name == game.name).first()
    if existing:
        raise HTTPException(status_code=409, detail=f"Game '{game.name}' already exists")

    db_game = models.Game(
        name = game.name,
        released = game.released,
        rating = game.rating,
        description = game.description,
        developer = game.developer,
        platforms = game.platforms,
        genres = game.genres,
        cover_image_url = game.cover_image_url
    )

    try:
        db.add(db_game)
        db.commit()
        db.refresh(db_game)
        return db_game
    except IntegrityError:
        db.rollback()
        raise HTTPException(status_code=500, detail="Database integrity error")

def create_games_bulk(db: Session, games: list[schemas.GameCreate]):
    stored = []
    errors = []

    for g in games:
        try:
            stored.append(create_game(db, g))
        except HTTPException as e:
            errors.append({"name": g.name, "error": e.detail})
    return {"inserted": stored, "errors": errors}

def get_games(db: Session):
    return db.query(models.Game).all()

def store_rawg_games(db: Session, games: list):
    stored = []

    for g in games:
        existing = db.query(models.Game).filter_by(name=g.get("name")).first()
        if existing:
            stored.append(existing)
            continue # Skip duplicate game data

        game = models.Game(
            name=g.get("name"),
            released=g.get("released"),
            rating=g.get("rating"),
            description=g.get("description_raw"),
            developer=(g.get("developers")[0]["name"] if g.get("developers") else None),
            platforms=[
                {"id": p["platform"]["id"], "name": p["platform"]["name"]}
                for p in g.get("platforms", [])
            ],
            genres=[
                {"id": genre["id"], "name": genre["name"]}
                for genre in g.get("genres", [])
            ],
            cover_image_url=g.get("background_image")
        )
        db.add(game)
        db.commit()
        db.refresh(game)
        stored.append(game)

    return stored

def get_games_with_suggestions(db: Session):
    from app.walkthroughs import build_walkthrough_suggestions
    games = db.query(models.Game).all()
    return [
        {
            "id": g.id,
            "name": g.name,
            "released": g.released,
            "rating": g.rating,
            "description": g.description,
            "developer": g.developer,
            "platforms": g.platforms,
            "genres": g.genres,
            "suggestions": build_walkthrough_suggestions(g.name)
        } for g in games
    ]