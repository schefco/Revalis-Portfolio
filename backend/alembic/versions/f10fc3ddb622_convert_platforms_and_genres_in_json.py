"""Convert platforms and genres in JSON

Revision ID: f10fc3ddb622
Revises: 
Create Date: 2025-11-03 13:17:42.047262

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
import json


# revision identifiers, used by Alembic.
revision: str = 'f10fc3ddb622'
down_revision: Union[str, Sequence[str], None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Upgrade schema for converting platforms and genres to JSON.
    This is for appending platforms and genres to the search filter dropdowns in the frontend"""
    # Add temp JSON columns
    op.add_column('games', sa.Column('platforms_json', sa.JSON(), nullable=True))
    op.add_column('games', sa.Column('genres_json', sa.JSON(), nullable=True))

    # Migrate existing string data into JSON arrays
    conn = op.get_bind()
    results = conn.execute(sa.text("SELECT id, platforms, genres FROM games")).fetchall()

    for row in results:
        platforms = []
        genres = []

        if row.platforms:
            platforms = [{"id": idx+1, "name": p.strip()} for idx, p in enumerate(row.platforms.split(",")) if p.strip()]
        if row.genres:
            genres = [{"id": idx+1, "name": g.strip()} for idx, g in enumerate(row.genres.split(",")) if g.strip()]

        conn.execute(sa.text("UPDATE games SET platforms_json = :platforms, genres_json = :genres WHERE id = :id"),
                     {"platforms": json.dumps(platforms), "genres": json.dumps(genres), "id": row.id})

    # Drop old string columns
    op.drop_column('games', 'platforms')
    op.drop_column('games', 'genres')

    # Rename new JSON columns
    op.alter_column('games', 'platforms_json', new_column_name='platforms')
    op.alter_column('games', 'genres_json', new_column_name='genres')


def downgrade() -> None:
    """Downgrade schema for converting the JSON platforms and genres list back to the DB for storage"""

    # Add back old string columns
    op.add_column('games', sa.Column('platforms', sa.String(), nullable=True))
    op.add_column('games', sa.Column('genres', sa.String(), nullable=True))

    # Convert JSON array back into comma-seperated strings
    conn = op.get_bind()
    results = conn.execute(sa.text("SELECT id, platforms, genres FROM games")). fetchall()

    for row in results:
        platform_str = ",".join([p["name"] for p in (row.platforms or [])])
        genres_str = ",".join([g["name"] for g in (row.genres or [])])

        conn.execute(
            sa.text("UPDATE games SET platforms = :platforms, genres = :genres WHERE id = :id"),
            {"platforms": platform_str, "genres": genres_str, "id": row.id}
        )

    # Drop JSON columns
    op.drop_column('games', 'platforms')
    op.drop_column('games', 'genres')
