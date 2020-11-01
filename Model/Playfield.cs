using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytet.Model
{
	public struct Playfield
	{
		private const int Width = 10;
		private const int Height = 40;
		private const int Length = Width * Height / 2;

		public static Playfield CreateEmpty()
		{
			return new Playfield(new byte[Length]);
		}

		public static Playfield CreateFromSource(byte[] array)
		{
			if (array is null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (array.Length != Length)
			{
				throw new ArgumentException($"Length must be {Length}", nameof(array));
			}

			if (!array.All(ByteIsValid))
			{
				throw new ArgumentException("All bytes must be valid piece enums", nameof(array));
			}

			return new Playfield(array);

			static bool ByteIsValid(byte b)
			{
				return b.TryGetPieceFromTop(out Piece p) && b.TryGetPieceFromBottom(out p);
			}
		}

		private static bool IsInRange(int x, int y)
		{
			return 0 <= x && x < Width &&
				0 <= y && y < Height;
		}

		private readonly byte[] array;

		private (int startY, int length)? removedRows;

		public Piece this[int x, int y]
		{
			get
			{
				if (!IsInRange(x, y))
				{
					throw new ArgumentOutOfRangeException();
				}

				int index = x + y * Width;
				byte b = array[index / 2];
				if (index % 2 == 0)
				{
					return b.GetPieceFromTop();
				}
				else
				{
					return b.GetPieceFromBottom();
				}
			}
			private set
			{
				if (!IsInRange(x, y))
				{
					throw new ArgumentOutOfRangeException();
				}

				int index = x + y * Width;
				if (index % 2 == 0)
				{
					array[index / 2] = array[index / 2].SetPieceToTop(value);
				}
				else
				{
					array[index / 2] = array[index / 2].SetPieceToBottom(value);
				}
			}
		}

		private Playfield(byte[] array)
		{
			this.array = array;
			removedRows = null;
		}

		private IEnumerable<(int, int)> GetPositionsOfPiece(Piece piece, int x, int y, int rotation)
		{
			return piece
				.GetOffsets(rotation)
				.Select(p => (p.x + x, p.y + y));
		}

		public void PlacePiece(Piece piece, int x, int y, int rotation)
		{
			IEnumerable<(int x, int y)> positions = GetPositionsOfPiece(piece, x, y, rotation);

			if (!IsValidPosition(positions))
			{
				throw new ArgumentException("Can not place piece there");
			}

			foreach (var p in positions)
			{
				this[p.x, p.y] = piece;
			}
		}

		public bool IsValidPosition(Piece piece, int x, int y, int rotation)
		{
			return IsValidPosition(GetPositionsOfPiece(piece, x, y, rotation));
		}
		private bool IsValidPosition(IEnumerable<(int, int)> positions)
		{
			return positions.All(IsValidPosition);
		}
		private bool IsValidPosition((int x, int y) position)
		{
			return IsInRange(position.x, position.y) &&
				this[position.x, position.y] == Piece.Empty;
		}

		public void RemoveRows(int startY, int length)
		{
			if (length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(length), "Can not remove zero rows");
			}

			if (startY + length > Height)
			{
				throw new ArgumentOutOfRangeException(nameof(startY));
			}

			for (int y = 0; y < length; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					this[x, startY + y] = Piece.Empty;
				}
			}

			removedRows = (startY, length);
		}

		public void Tick()
		{
			if (removedRows.HasValue)
			{
				for (int y = removedRows.Value.startY - 1; y >= 0; y--)
				{
					for (int x = 0; x < Width; x++)
					{
						this[x, y + removedRows.Value.length] = this[x, y];
					}
				}

				removedRows = null;
			}
		}
	}
}