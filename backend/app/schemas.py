from pydantic import BaseModel, Field
from typing import List, Optional

class GenreDTO(BaseModel):
    id: Optional[int]
    name: Optional[str]

class PlatformDTO(BaseModel):
    id: int
    name: str
    icon: Optional[str] = None

class GameBase(BaseModel):
    name: str
    released: Optional[str] = None
    rating: Optional[float] = None
    description: Optional[str] = None
    developer: Optional[str] = None
    platforms: List[PlatformDTO] = Field(default_factory=list)
    genres: List[GenreDTO] = Field(default_factory=list)
    cover_image_url: Optional[str] = None

class GameCreate(GameBase):
    pass

class GameRead(GameBase):
    id: int

    class Config:
        from_attributes = True

class WalkthroughSuggestion(BaseModel):
    source: str
    url: str

class GameWithWalkthrough(BaseModel):
    name: str
    suggestions: list[WalkthroughSuggestion]

