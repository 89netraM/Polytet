namespace Polytet.Communication
{
	public interface IReactor
	{
		void DefaultReaction(byte[] bytes);
	}
}