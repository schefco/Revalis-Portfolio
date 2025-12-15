from fastapi import FastAPI
from app.api import router
from app.db import Base, engine
from app.models import Game

# Game.__table__.drop(bind=engine, checkfirst=True)

Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="Revalis API",
    description="Backend service for searching and storing games from RAWG",
    version="1.0.0"
)
app.include_router(router)