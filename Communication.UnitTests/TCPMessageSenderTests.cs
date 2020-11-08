using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polytet.Communication.Messages;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Polytet.Communication.UnitTests
{
	[TestClass]
	public class TCPMessageSenderTests
	{
		private readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 1337);
		private const byte playerIntegerSize = 4;

		[TestMethod]
		public async Task ReactToMessages()
		{
			IMessage message = new ChatMessageClient(0, "Hello World!");

			TcpListener listener = new TcpListener(endPoint);
			listener.Start();
			Task<TcpClient> receiverTask = listener.AcceptTcpClientAsync();

			using TcpClient sender = new TcpClient();
			sender.Connect(endPoint);
			TCPMessageSender messageSender = new TCPMessageSender(sender.GetStream());
			messageSender.PlayerIntegerSize = playerIntegerSize;
			messageSender.QueueMessage(message);
			messageSender.SendOneMessage();
			sender.Close();

			using TcpClient receiver = await receiverTask;
			listener.Stop();

			byte[] bytes = new byte[256];
			receiver.GetStream().Read(bytes, 0, bytes.Length);
			IMessage receivedMessage = Serializer.DeSerializeClient(bytes[4..], playerIntegerSize);

			Assert.AreEqual(message, receivedMessage, "Should receive the same message as was sent.");
		}
	}
}