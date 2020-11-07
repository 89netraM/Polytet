using System;
using System.Linq;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0110, MessageReceiver.Client)]
	public readonly struct PointsUpdateServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize + 4)
			{
				throw new Exception("Too short message");
			}

			BigInteger affectedPlayerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);
			uint newTotalPoints = BitConverter.ToUInt32(message.Skip(playerIntegerSize).Take(4).Reverse().ToArray(), 0);

			return new PointsUpdateServer(affectedPlayerId, newTotalPoints);
		}

		public BigInteger AffectedPlayerId { get; }
		public uint NewTotalPoints { get; }

		public PointsUpdateServer(BigInteger affectedPlayerId, uint newTotalPoints)
		{
			AffectedPlayerId = affectedPlayerId;
			NewTotalPoints = newTotalPoints;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[playerIntegerSize + 4];

			Helpers.InsertPlayerInteger(message, AffectedPlayerId, 0, playerIntegerSize);

			byte[] newTotalPointsArray = BitConverter.GetBytes(NewTotalPoints);
			Array.Reverse(newTotalPointsArray);
			newTotalPointsArray.CopyTo(message, playerIntegerSize);

			return message;
		}
	}
}