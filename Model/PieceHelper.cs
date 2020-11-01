﻿namespace Polytet.Model
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
	}
}