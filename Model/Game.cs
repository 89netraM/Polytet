using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytet.Model
{
	public class Game
	{
		private Playfield playfield;
		private (Piece piece, int x, int y, int rotation)? floating;

		public Piece this[int x, int y]
		{
			get
			{
				Piece fromField = playfield[x, y];

				if (fromField == Piece.Empty)
				{
					if (floating.HasValue && GetPositionsOfPiece().Any(p => p.x == x && p.y == y))
					{
						return floating.Value.piece;
					}
					else
					{
						return Piece.Empty;
					}
				}
				else
				{
					return fromField;
				}
			}
		}

		public Game()
		{
			playfield = Playfield.CreateEmpty();
			floating = null;
		}

		public Game(byte[] playfield) : this()
		{
			this.playfield = Playfield.CreateFromSource(playfield);
		}

		private IEnumerable<(int x, int y)> GetPositionsOfPiece(int? rotation = null, int? x = null, int? y = null)
		{
			if (!floating.HasValue)
			{
				throw new InvalidOperationException();
			}

			return floating.Value.piece
				.GetOffsets(rotation ?? floating.Value.rotation)
				.Select(p => (p.x + x ?? floating.Value.x, p.y + y ?? floating.Value.y));
		}

		public void PlacePiece()
		{
			if (!floating.HasValue)
			{
				throw new InvalidOperationException();
			}

			IEnumerable<(int, int)> positions = GetPositionsOfPiece();

			if (!IsValidPosition(positions))
			{
				throw new ArgumentException("Can not place piece there");
			}

			foreach (var (x, y) in positions)
			{
				playfield[x, y] = floating.Value.piece;
			}
		}

		public bool IsValidPosition()
		{
			return IsValidPosition(GetPositionsOfPiece());
		}
		private bool IsValidPosition(IEnumerable<(int, int)> positions)
		{
			return positions.All(IsValidPosition);
		}
		private bool IsValidPosition((int x, int y) position)
		{
			return Playfield.IsInRange(position.x, position.y) &&
				playfield[position.x, position.y] == Piece.Empty;
		}
	}
}