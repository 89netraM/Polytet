using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polytet.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Polytet.Communication.UnitTests
{
	[TestClass]
	public class TCPMessagePasserTests
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
			sender.GetStream().Write(Serializer.Serialize(message, playerIntegerSize));
			sender.Close();

			using TcpClient receiver = await receiverTask;
			listener.Stop();
			Reactor reactor = new Reactor();
			TCPMessagePasser messagePasser = new TCPMessagePasser(receiver.GetStream(), reactor);
			messagePasser.PlayerIntegerSize = playerIntegerSize;
			messagePasser.ListenAndReactClient();

			Assert.IsTrue(new[] { message }.SequenceEqual(reactor.Log), "Should receive the same message as was sent.");
		}

		[TestMethod]
		public async Task ReactToLongMessages()
		{
			/// <see cref="TCPMessagePasser"/> has a buffer size of 256
			const int tooLongMessage = 300;

			IMessage message = new ChatMessageClient(0, new String('e', tooLongMessage));

			TcpListener listener = new TcpListener(endPoint);
			listener.Start();
			Task<TcpClient> receiverTask = listener.AcceptTcpClientAsync();

			using TcpClient sender = new TcpClient();
			sender.Connect(endPoint);
			sender.GetStream().Write(Serializer.Serialize(message, playerIntegerSize));
			sender.Close();

			using TcpClient receiver = await receiverTask;
			listener.Stop();
			Reactor reactor = new Reactor();
			TCPMessagePasser messagePasser = new TCPMessagePasser(receiver.GetStream(), reactor);
			messagePasser.PlayerIntegerSize = playerIntegerSize;
			messagePasser.ListenAndReactClient();

			Assert.IsTrue(new[] { message }.SequenceEqual(reactor.Log), "Should receive the same message as was sent even if it's longer than the buffer.");
		}

		[TestMethod]
		[ExpectedException(typeof(NotImplementedException), "Expected DefaultReaction to be called")]
		public async Task ReactToUnknownMessages()
		{
			IMessage message = new TickServer();

			TcpListener listener = new TcpListener(endPoint);
			listener.Start();
			Task<TcpClient> receiverTask = listener.AcceptTcpClientAsync();

			using TcpClient sender = new TcpClient();
			sender.Connect(endPoint);
			sender.GetStream().Write(Serializer.Serialize(message, playerIntegerSize));
			sender.Close();

			using TcpClient receiver = await receiverTask;
			listener.Stop();
			Reactor reactor = new Reactor();
			TCPMessagePasser messagePasser = new TCPMessagePasser(receiver.GetStream(), reactor);
			messagePasser.PlayerIntegerSize = playerIntegerSize;

			messagePasser.ListenAndReactServer();
		}
	}

	class Reactor : IReactor
	{
		private readonly ICollection<IMessage> log = new List<IMessage>();
		public IEnumerable<IMessage> Log => log;

		public void DefaultReaction(byte[] bytes) => throw new NotImplementedException($"{nameof(DefaultReaction)} was called");

		[Header(0b_0000_0000)]
		public void ReceiveChatMessage(ChatMessageClient message)
		{
			log.Add(message);
		}
	}
}