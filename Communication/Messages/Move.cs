using System;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0101, MessageReceiver.Client)]
	public readonly struct MoveServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize + 1)
			{
				throw new Exception("Too short message");
			}

			BigInteger affectedPlayerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);
			
			Move move = (Move)message[playerIntegerSize];
			if (!Enum.IsDefined(typeof(Move), move))
			{
				throw new Exception($"Malformated enum \"{nameof(Move)}\"");
			}

			return new MoveServer(affectedPlayerId, move);
		}

		public BigInteger AffectedPlayerId { get; }
		public Move Move { get; }

		public MoveServer(BigInteger affectedPlayerId, Move move)
		{
			AffectedPlayerId = affectedPlayerId;
			Move = move;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[playerIntegerSize + 1];

			Helpers.InsertPlayerInteger(message, AffectedPlayerId, 0, playerIntegerSize);
			message[playerIntegerSize] = (byte)Move;

			return message;
		}
	}

	[Header(0b_1000_0101, MessageReceiver.Server)]
	public readonly struct MoveClient : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < 1)
			{
				throw new Exception("Too short message");
			}

			Move move = (Move)message[0];

			return new MoveClient(move);
		}

		public Move Move { get; }

		public MoveClient(Move move)
		{
			Move = move;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			return new[] { (byte)Move };
		}
	}

	public enum Move : byte
	{
		NotAllowed             = 0b_0000_0000,
		MoveLeft               = 0b_0000_0001,
		MoveRight              = 0b_0000_0010,
		MoveDown               = 0b_0000_0100,
		RotateCounterClockwise = 0b_0000_1000,
		RotateClockwise	       = 0b_0001_0000
	}
}