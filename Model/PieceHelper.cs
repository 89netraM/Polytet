using System;
using System.Collections.Generic;

namespace Polytet.Model
{
	public static class PieceHelper
	{
		public static Piece GetPieceFromTop(this byte b)
		{
			return GetPieceFromBottom((byte)(b >> 4));
		}
		public static Piece GetPieceFromBottom(this byte b)
		{
			return (Piece)(b & 0b_0000_1111);
		}

		public static bool TryGetPieceFromTop(this byte b, out Piece piece)
		{
			return TryGetPieceFromBottom((byte)(b >> 4), out piece);
		}
		public static bool TryGetPieceFromBottom(this byte b, out Piece piece)
		{
			b = (byte)(b & 0b_0000_1111);

			if (typeof(Piece).IsEnumDefined(b))
			{
				piece = (Piece)b;
				return true;
			}
			else
			{
				piece = Piece.Empty;
				return false;
			}
		}

		public static byte SetPieceToTop(this byte b, Piece piece)
		{
			return (byte)(b & 0b_0000_1111 | (byte)((byte)piece << 4));
		}
		public static byte SetPieceToBottom(this byte b, Piece piece)
		{
			return (byte)(b & 0b_1111_0000 | (byte)piece);
		}

		private static readonly IReadOnlyDictionary<Piece, IReadOnlyList<IEnumerable<(int, int)>>> PieceOffsets = new Dictionary<Piece, IReadOnlyList<IEnumerable<(int, int)>>>
		{
			{
				Piece.I,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 1), (0, 1), (1, 1), (2, 1) },
					new[] { (1, -1), (1, 0), (1, 1), (1, 2) },
				}
			},
			{
				Piece.J,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 0), (0, 0), (1, 0), (1, 1) },
					new[] { (0, -1), (0, 0), (0, 1), (-1, 1) },
					new[] { (-1, -1), (-1, 0), (0, 0), (1, 0) },
					new[] { (1, -1), (0, -1), (0, 0), (0, 1) },
				}
			},
			{
				Piece.L,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 1), (-1, 0), (0, 0), (1, 0) },
					new[] { (-1, -1), (0, -1), (0, 0), (0, 1) },
					new[] { (-1, 0), (0, 0), (1, 0), (1, -1) },
					new[] { (0, -1), (0, 0), (0, 1), (1, 1) },
				}
			},
			{
				Piece.O,
				new IEnumerable<(int, int)>[]
				{
					new[] { (0, 0), (1, 0), (0, 1), (1, 1) },
				}
			},
			{
				Piece.S,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 1), (0, 1), (0, 0), (1, 0) },
					new[] { (0, -1), (0, 0), (1, 0), (1, 1) },
				}
			},
			{
				Piece.T,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 0), (0, 0), (0, 1), (1, 0) },
					new[] { (0, -1), (-1, 0), (0, 0), (0, 1) },
					new[] { (-1, 0), (0, 0), (0, -1), (1, 0) },
					new[] { (0, -1), (0, 0), (1, 0), (0, 1) },
				}
			},
			{
				Piece.Z,
				new IEnumerable<(int, int)>[]
				{
					new[] { (-1, 0), (0, 0), (0, 1), (1, 1) },
					new[] { (1, -1), (1, 0), (0, 0), (0, 1) },
				}
			}
		};
		public static IEnumerable<(int x, int y)> GetOffsets(this Piece piece, int rotation)
		{
			if (piece == Piece.Empty)
			{
				throw new ArgumentException();
			}

			IReadOnlyList<IEnumerable<(int, int)>> offsets = PieceOffsets[piece];
			return offsets[Math.Abs(rotation % offsets.Count)];
		}
	}
}