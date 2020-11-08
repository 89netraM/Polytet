using CliFx;
using Polytet.Communication;
using Polytet.Communication.Messages;
using System;

namespace Polytet.Server
{
	public class MessageReactor : IReactor
	{
		public event Action? Connect;
		public event Action<byte[]>? PassToPeers;
		public bool IsConnected { get; set; } = false;

		private readonly IConsole console;

		public MessageReactor(IConsole console)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
		}

		[Header(0b_1000_0000)]
		public void ReceiveConnect(ConnectClient message)
		{
			if (!IsConnected)
			{
				Connect?.Invoke();
			}
		}

		public void DefaultReaction(byte[] bytes)
		{
			if (bytes.Length > 0)
			{
				if (Serializer.IntendedReceiver(bytes) == MessageReceiver.Server)
				{
					string binary = Convert.ToString(bytes[0], 2).PadLeft(8, '0');
					console.Error.WriteLine($"Received unknown message. Header 0b_{binary.Substring(0, 4)}_{binary.Substring(4)}.");
				}
				else
				{
					PassToPeers?.Invoke(bytes);
				}
			}
		}
	}
}