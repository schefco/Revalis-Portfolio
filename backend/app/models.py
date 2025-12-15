from sqlalchemy import Column, Integer, String, Float, JSON
from app.db import Base

class Game(Base):
    __tablename__ = "games"
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, index=True, unique=True)
    released = Column(String)
    rating = Column(Float)
    description = Column(String, nullable=True)
    developer = Column(String, nullable=True)
    platforms = Column(JSON, nullable=True)
    genres = Column(JSON, nullable=True)
    cover_image_url = Column(String, nullable=True)
