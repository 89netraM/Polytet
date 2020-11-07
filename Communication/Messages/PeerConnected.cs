using System;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0001, MessageReceiver.Client)]
	public readonly struct PeerConnectedServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < 1 + playerIntegerSize * 2)
			{
				throw new Exception("Too short message");
			}

			bool isConnecting = message[0] != 0;
			BigInteger newPlayerCount = Helpers.ReadPlayerInteger(message, 1, playerIntegerSize);
			BigInteger affectedPlayerId = Helpers.ReadPlayerInteger(message, 1 + playerIntegerSize, playerIntegerSize);

			return new PeerConnectedServer(isConnecting, newPlayerCount, affectedPlayerId);
		}

		public bool IsConnecting { get; }
		public bool IsDisconnecting => !IsConnecting;
		public BigInteger NewPlayerCount { get; }
		public BigInteger AffectedPlayerId { get; }

		public PeerConnectedServer(bool isConnecting, BigInteger newPlayerCount, BigInteger affectedPlayerId)
		{
			IsConnecting = isConnecting;
			NewPlayerCount = newPlayerCount;
			AffectedPlayerId = affectedPlayerId;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[1 + playerIntegerSize * 2];

			message[0] = Convert.ToByte(IsConnecting);
			Helpers.InsertPlayerInteger(message, NewPlayerCount, 1, playerIntegerSize);
			Helpers.InsertPlayerInteger(message, AffectedPlayerId, 1 + playerIntegerSize, playerIntegerSize);

			return message;
		}
	}
}