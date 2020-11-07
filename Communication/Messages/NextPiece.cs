using Polytet.Model;
using System;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0011, MessageSender.Server)]
	public readonly struct NextPieceServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize + 1)
			{
				throw new Exception("Too short message");
			}

			BigInteger affectedPlayerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);

			Piece piece = (Piece)message[playerIntegerSize];
			if (!Enum.IsDefined(typeof(Piece), piece))
			{
				throw new Exception($"Malformated enum \"{nameof(Model.Piece)}\"");
			}

			return new NextPieceServer(affectedPlayerId, piece);
		}

		public BigInteger AffectedPlayerId { get; }
		public Piece Piece { get; }

		public NextPieceServer(BigInteger affectedPlayerId, Piece piece)
		{
			AffectedPlayerId = affectedPlayerId;
			Piece = piece;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[playerIntegerSize + 1];

			Helpers.InsertPlayerInteger(message, AffectedPlayerId, 0, playerIntegerSize);
			message[playerIntegerSize] = (byte)Piece;

			return message;
		}
	}
}