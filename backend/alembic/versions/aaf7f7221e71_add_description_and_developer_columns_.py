"""add description and developer columns to games

Revision ID: aaf7f7221e71
Revises: add_description_developer
Create Date: 2025-12-12 10:30:23.291847

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision: str = 'aaf7f7221e71'
down_revision: Union[str, Sequence[str], None] = 'add_description_developer'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Upgrade schema."""
    pass


def downgrade() -> None:
    """Downgrade schema."""
    pass
