using Polytet.Communication;
using System;
using System.Diagnostics;

namespace Polytet.Player
{
	abstract class DefaultReactionReactor : IReactor
	{
		public void DefaultReaction(byte[] bytes)
		{
#if DEBUG
			string binary = Convert.ToString(bytes[0], 2).PadLeft(8, '0');
			Debug.WriteLine($"Received unknown message. Header 0b_{binary.Substring(0, 4)}_{binary.Substring(4)} ({bytes[0]}).");
			Debug.WriteLine('\t' + String.Join(", ", bytes));
#endif
		}
	}
}