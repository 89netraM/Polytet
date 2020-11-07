namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0010, MessageReceiver.Server)]
	[Header(0b_1000_0010, MessageReceiver.Client)]
	public readonly struct StartGame : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			return new StartGame();
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			return new byte[0];
		}
	}
}