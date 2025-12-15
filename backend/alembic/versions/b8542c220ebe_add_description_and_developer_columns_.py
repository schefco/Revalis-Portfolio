"""add description and developer columns to games

Revision ID: b8542c220ebe
Revises: aaf7f7221e71
Create Date: 2025-12-12 10:50:10.165792

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision: str = 'b8542c220ebe'
down_revision: Union[str, Sequence[str], None] = 'aaf7f7221e71'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Upgrade schema."""
    pass


def downgrade() -> None:
    """Downgrade schema."""
    pass
