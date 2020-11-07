namespace Polytet.Communication
{
	public interface IMessage
	{
		internal byte[] Serialize(byte playerIntegerSize);
	}
}