using CliFx;
using Polytet.Communication;
using Polytet.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Polytet.Server
{
	class GameManager
	{
		public const byte PlayerIntegerSize = 1;

		public bool HasStarted { get; private set; } = false;

		private readonly IConsole console;

		private readonly ReaderWriterLockSlim gamerRights = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private readonly IDictionary<EndPoint, (BigInteger id, TCPMessageSender messageSender)> gamers = new Dictionary<EndPoint, (BigInteger, TCPMessageSender)>();

		public GameManager(IConsole console)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
		}

		public void AddToGame(TcpClient client)
		{
			NetworkStream ns = client.GetStream();
			TCPMessageSender messageSender = new TCPMessageSender(ns);
			messageSender.PlayerIntegerSize = PlayerIntegerSize;
			MessageReactor reactor = new MessageReactor(console);
			TCPMessagePasser messagePasser = new TCPMessagePasser(ns, reactor);
			messagePasser.PlayerIntegerSize = PlayerIntegerSize;

			reactor.Connect += Connect;
			Task.Run(Listen);

			void Connect()
			{
				Debug.WriteLine("1");
				gamerRights.EnterUpgradeableReadLock();
				if (gamers.Count + 1 < byte.MaxValue)
				{
					Debug.WriteLine("2");
					BigInteger id = gamers.Count;
					gamerRights.EnterWriteLock();
					Debug.WriteLine("3");
					gamers.Add(client.Client.RemoteEndPoint, (id, messageSender));
					gamerRights.ExitWriteLock();
					Debug.WriteLine("4");

					messageSender.QueueMessage(new ConnectServer(true, PlayerIntegerSize, id, gamers.Count));
					messageSender.SendOneMessage();
					Debug.WriteLine("5");

					SendToAllExcept(new PeerConnectedServer(true, gamers.Count, id), client.Client.RemoteEndPoint);
					Debug.WriteLine("6");

					reactor.PassToPeers += PassToPeers;
					reactor.IsConnected = true;

					Task.Run(Send);
					Debug.WriteLine("7");
				}
				else
				{
					messageSender.QueueMessage(new ConnectServer(false, 0, 0, 0));
					messageSender.SendOneMessage();

					client.Dispose();
					reactor.Connect -= Connect;
				}
				gamerRights.ExitUpgradeableReadLock();
			}
			void Send()
			{
				try
				{
					while (true)
					{
						messageSender.SendOneMessage();
					}
				}
				catch (Exception ex) when (!(ex is ObjectDisposedException))
				{
					Destroy(nameof(Send), ex);
				}
			}
			void Listen()
			{
				try
				{
					while (true)
					{
						messagePasser.ListenAndReactServer();
					}
				}
				catch (Exception ex) when (!(ex is ObjectDisposedException))
				{
					Destroy(nameof(Listen), ex);
				}
			}
			void PassToPeers(byte[] bytes) => SendRawToAllExcept(bytes, client.Client.RemoteEndPoint);
			void Destroy(string destroyer, Exception ex)
			{
				EndPoint endPoint = client.Client.RemoteEndPoint;
				console.Error.WriteLine($"Destroying {endPoint} because of {destroyer}. {ex?.Message ?? ""}");
				client.Dispose();
				reactor.PassToPeers -= PassToPeers;
				reactor.Connect -= Connect;

				gamerRights.EnterWriteLock();
				var (id, _) = gamers[endPoint];
				gamers.Remove(endPoint);
				SendToAllExcept(new PeerConnectedServer(false, gamers.Count, id), endPoint);
				gamerRights.ExitWriteLock();
			}
		}

		private void SendToAllExcept(IMessage message, EndPoint? except)
		{
			gamerRights.EnterReadLock();
			foreach (var kvp in gamers)
			{
				if (kvp.Key != except)
				{
					kvp.Value.messageSender.QueueMessage(message);
				}
			}
			gamerRights.ExitReadLock();
		}
		private void SendRawToAllExcept(byte[] bytes, EndPoint? except)
		{
			gamerRights.EnterReadLock();
			foreach (var kvp in gamers)
			{
				if (kvp.Key != except)
				{
					kvp.Value.messageSender.QueueRawMessage(bytes);
				}
			}
			gamerRights.ExitReadLock();
		}
	}
}