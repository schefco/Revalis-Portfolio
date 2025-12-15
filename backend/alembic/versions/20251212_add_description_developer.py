"""add description and developer columns to games

Revision ID: add_description_developer
Revises: f10fc3ddb622
Create Date: 2025-12-12 10:30:00

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = "add_description_developer"
down_revision = "f10fc3ddb622"   # replace with your last migration ID
branch_labels = None
depends_on = None


def upgrade():
    op.add_column("games", sa.Column("description", sa.String(), nullable=True))
    op.add_column("games", sa.Column("developer", sa.String(), nullable=True))


def downgrade():
    op.drop_column("games", "description")
    op.drop_column("games", "developer")

