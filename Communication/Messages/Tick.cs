namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0100, MessageReceiver.Client)]
	public readonly struct TickServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			return new TickServer();
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			return new byte[0];
		}
	}
}