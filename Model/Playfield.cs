using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytet.Model
{
	public struct Playfield
	{
		public const int Width = 10;
		public const int Height = 40;
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

		public static bool IsInRange(int x, int y)
		{
			return 0 <= x && x < Width &&
				0 <= y && y < Height;
		}

		private static int PointsForRows(int nRows) => nRows switch
		{
			0 => 0,
			1 => 40,
			2 => 100,
			3 => 300,
			4 => 1200,
			_ => throw new ArgumentException()
		};

		private readonly byte[] array;

		private readonly IList<int> removedRows;

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
			removedRows = new List<int>();
		}

		public int Tick()
		{
			MoveDownClearedRows();

			return DetectAndClearFullRows();
		}

		private void MoveDownClearedRows()
		{
			for (int i = 0; i < removedRows.Count; i++)
			{
				int current = removedRows[i];
				int next = i + 1 < removedRows.Count ? removedRows[i + 1] : -1;
				int steps = i + 1;

				for (int y = current - 1; y > next; y--)
				{
					for (int x = 0; x < Width; x++)
					{
						this[x, y + steps] = this[x, y];
						this[x, y] = Piece.Empty;
					}
				}
			}

			removedRows.Clear();
		}

		private int DetectAndClearFullRows()
		{
			for (int y = Height - 1; y >= 0; y--)
			{
				if (IsRowFull(y))
				{
					for (int x = 0; x < Width; x++)
					{
						this[x, y] = Piece.Empty;
					}

					removedRows.Add(y);
				}
			}

			return PointsForRows(removedRows.Count);
		}
		private bool IsRowFull(int y)
		{
			for (int x = 0; x < Width; x++)
			{
				if (this[x, y] == Piece.Empty)
				{
					return false;
				}
			}

			return true;
		}

		public byte[] CloneArray()
		{
			byte[] array = new byte[this.array.Length];
			this.array.CopyTo(array, 0);
			return array;
		}
	}
}