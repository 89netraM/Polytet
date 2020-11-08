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

			byte[] all = new byte[bytes.Length + 4];
			for (int i = 0; i < 4; i++)
			{
				all[i] = (byte)(bytes.Length >> (24 - 8 * i) & 0b_1111_1111);
			}
			bytes.CopyTo(all, 4);

			networkStream.Write(all, 0, all.Length);
		}

		public void QueueMessage(IMessage message) => QueueRawMessage(Serializer.Serialize(message, PlayerIntegerSize));
		public void QueueRawMessage(byte[] bytes) => messages.Enqueue(bytes);
	}
}