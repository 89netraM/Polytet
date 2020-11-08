using CliFx;
using Polytet.Communication;
using Polytet.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly IDictionary<EndPoint, (BigInteger id, ClientHandler handler)> gamers = new Dictionary<EndPoint, (BigInteger, ClientHandler)>();

		public GameManager(IConsole console)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
		}

		public void AddToGame(TcpClient client)
		{
			NetworkStream ns = client.GetStream();
			TCPMessageSender messageSender = new TCPMessageSender(ns);
			messageSender.PlayerIntegerSize = PlayerIntegerSize;
			ClientHandler handler = new ClientHandler(console, messageSender, GetStateOf);
			TCPMessagePasser messagePasser = new TCPMessagePasser(ns, handler);
			messagePasser.PlayerIntegerSize = PlayerIntegerSize;

			handler.Connect += Connect;
			Task.Run(Listen);

			void Connect()
			{
				gamerRights.EnterUpgradeableReadLock();
				try
				{
					if (gamers.Count + 1 < byte.MaxValue)
					{
						BigInteger id = gamers.Count;
						gamerRights.EnterWriteLock();
						try
						{
							gamers.Add(client.Client.RemoteEndPoint, (id, handler));
						}
						finally
						{
							gamerRights.ExitWriteLock();
						}

						messageSender.QueueMessage(new ConnectServer(true, PlayerIntegerSize, id, gamers.Count));
						messageSender.SendOneMessage();

						SendToAllExcept(new PeerConnectedServer(true, gamers.Count, id), client.Client.RemoteEndPoint);

						handler.PassToPeers += PassToPeers;
						handler.StartGame += StartGame;
						handler.Broadcast += SendToAll;
						handler.IsConnected = true;
						handler.Id = id;

						Task.Run(Send);
					}
					else
					{
						messageSender.QueueMessage(new ConnectServer(false, 0, 0, 0));
						messageSender.SendOneMessage();

						client.Dispose();
						handler.Connect -= Connect;

						return;
					}
				}
				finally
				{
					gamerRights.ExitUpgradeableReadLock();
				}

				if (gamers.Count == byte.MaxValue)
				{
					StartGame();
				}
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
				handler.PassToPeers -= PassToPeers;
				handler.Connect -= Connect;
				handler.StartGame -= StartGame;

				gamerRights.EnterWriteLock();
				try
				{
					var (id, _) = gamers[endPoint];
					gamers.Remove(endPoint);
					SendToAllExcept(new PeerConnectedServer(false, gamers.Count, id), endPoint);
				}
				finally
				{
					gamerRights.ExitWriteLock();
				}
			}
		}

		private void StartGame()
		{
			HasStarted = true;
			foreach (var kvp in gamers)
			{
				kvp.Value.handler.MessageSender.QueueMessage(new StartGame());
				kvp.Value.handler.Start();
			}
		}

		private StateUpdateServer GetStateOf(BigInteger id)
		{
			return gamers.First(g => g.Value.id == id).Value.handler.GetState();
		}

		private void SendToAll(IMessage message) => SendToAllExcept(message, null);
		private void SendToAllExcept(IMessage message, EndPoint? except)
		{
			gamerRights.EnterReadLock();
			try
			{
				foreach (var kvp in gamers)
				{
					if (kvp.Key != except)
					{
						kvp.Value.handler.MessageSender.QueueMessage(message);
					}
				}
			}
			finally
			{
				gamerRights.ExitReadLock();
			}
		}
		private void SendRawToAllExcept(byte[] bytes, EndPoint? except)
		{
			gamerRights.EnterReadLock();
			try
			{
				foreach (var kvp in gamers)
				{
					if (kvp.Key != except)
					{
						kvp.Value.handler.MessageSender.QueueRawMessage(bytes);
					}
				}
			}
			finally
			{
				gamerRights.ExitReadLock();
			}
		}
	}
}