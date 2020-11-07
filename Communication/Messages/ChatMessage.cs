using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Polytet.Communication.Messages
{
	[Header(0b_0000_0000, MessageSender.Client)]
	public readonly struct ChatMessageClient : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize + 2)
			{
				throw new Exception("Too short message");
			}

			BigInteger senderPlayerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);
			ushort messageLength = BitConverter.ToUInt16(message.Skip(playerIntegerSize).Take(2).Reverse().ToArray(), 0);

			if (message.Length < playerIntegerSize + 2 + messageLength)
			{
				throw new Exception("Too short message");
			}
			string chatMessage = Encoding.UTF8.GetString(message, playerIntegerSize + 2, messageLength);

			return new ChatMessageClient(senderPlayerId, chatMessage);
		}

		public BigInteger SenderPlayerId { get; }
		public string ChatMessage { get; }

		public ChatMessageClient(BigInteger senderPlayerId, string chatMessage)
		{
			SenderPlayerId = senderPlayerId;
			ChatMessage = chatMessage;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] chatMessage = Encoding.UTF8.GetBytes(ChatMessage ?? "");
			ushort messageLength = (ushort)chatMessage.Length;

			byte[] message = new byte[playerIntegerSize + 2 + messageLength];

			Helpers.InsertPlayerInteger(message, SenderPlayerId, 0, playerIntegerSize);

			byte[] messageLengthArray = BitConverter.GetBytes(messageLength);
			Array.Reverse(messageLengthArray);
			messageLengthArray.CopyTo(message, playerIntegerSize);

			chatMessage.CopyTo(message, playerIntegerSize + 2);

			return message;
		}
	}
}