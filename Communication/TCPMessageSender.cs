using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Polytet.Communication
{
	public class TCPMessageSender
	{
		public byte PlayerIntegerSize { get; set; } = 0;
		private readonly ConcurrentQueue<byte[]> messages = new ConcurrentQueue<byte[]>();
		private readonly NetworkStream networkStream;

		public TCPMessageSender(NetworkStream networkStream)
		{
			this.networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
		}

		public void SendOneMessage()
		{
			byte[] bytes;
			while (!messages.TryDequeue(out bytes));

			networkStream.Write(bytes, 0, bytes.Length);
		}

		public void QueueMessage(IMessage message) => QueueRawMessage(Serializer.Serialize(message, PlayerIntegerSize));
		public void QueueRawMessage(byte[] bytes) => messages.Enqueue(bytes);
	}
}