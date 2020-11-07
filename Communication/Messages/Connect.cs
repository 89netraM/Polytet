using System;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_000, MessageSender.Server)]
	public readonly struct ConnectServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte _)
		{
			if (message.Length < 2)
			{
				throw new Exception("Too short message");
			}

			bool canHost = message[0] != 0;
			byte playerIntegerSize = message[1];

			if (message.Length < 2 + playerIntegerSize * 2)
			{
				throw new Exception("Too short message");
			}

			BigInteger playerId = Helpers.ReadPlayerInteger(message, 2, playerIntegerSize);
			BigInteger playerCount = Helpers.ReadPlayerInteger(message, 2 + playerIntegerSize, playerIntegerSize);

			return new ConnectServer(canHost, playerIntegerSize, playerId, playerCount);
		}

		public bool CanHost { get; }
		public byte PlayerIntegerSize { get; }
		public BigInteger PlayerId { get; }
		public BigInteger PlayerCount { get; }

		public ConnectServer(bool canHost, byte playerIntegerSize, BigInteger playerId, BigInteger playerCount)
		{
			CanHost = canHost;
			PlayerIntegerSize = playerIntegerSize;
			PlayerId = playerId;
			PlayerCount = playerCount;
		}

		byte[] IMessage.Serialize(byte _)
		{
			byte[] message = new byte[2 + PlayerIntegerSize * 2];

			message[0] = Convert.ToByte(CanHost);
			message[1] = PlayerIntegerSize;
			Helpers.InsertPlayerInteger(message, PlayerId, 2, PlayerIntegerSize);
			Helpers.InsertPlayerInteger(message, PlayerCount, 2 + PlayerIntegerSize, PlayerIntegerSize);

			return message;
		}
	}

	[Header(0b_1000_0000, MessageSender.Client)]
	public readonly struct ConnectClient : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			return new ConnectClient();
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			return new byte[0];
		}
	}
}