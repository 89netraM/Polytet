using System;
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

			if (array.Any(ByteIsInvalid))
			{
				throw new ArgumentException("All bytes must be valid piece enums", nameof(array));
			}

			return new Playfield(array);

			static bool ByteIsInvalid(byte b)
			{
				return !b.TryGetPieceFromTop(out Piece p) || !b.TryGetPieceFromBottom(out p);
			}
		}

		private static bool IsInRange(int x, int y)
		{
			return 0 <= x && x < Width &&
				0 <= y && y < Height;
		}

		private byte[] array;

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
			set
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
		}

		public void RemoveRows(int startY, int length)
		{
			for (int y = 0; y < length; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					this[x, startY + y] = Piece.Empty;
				}
			}
		}

		public void Tick()
		{
			for (int y = Height - 2; y >= 0; y--)
			{
				for (int x = 0; x < Width; x++)
				{
					if (this[x, y] != Piece.Empty && this[x, y + 1] == Piece.Empty)
					{
						this[x, y + 1] = this[x, y];
						this[x, y] = Piece.Empty;
					}
				}
			}
		}
	}
}