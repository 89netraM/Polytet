using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Polytet.Communication
{
	public class TCPMessageSender
	{
		public byte PlayerIntegerSize { get; set; } = 0;
		private readonly ConcurrentQueue<IMessage> messages = new ConcurrentQueue<IMessage>();
		private readonly NetworkStream networkStream;

		public TCPMessageSender(NetworkStream networkStream)
		{
			this.networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
		}

		public void SendOneMessage()
		{
			IMessage message;
			while (!messages.TryDequeue(out message));

			byte[] bytes = Serializer.Serialize(message, PlayerIntegerSize);
			networkStream.Write(bytes, 0, bytes.Length);
		}

		public void QueueMessage(IMessage message) => messages.Enqueue(message);
	}
}