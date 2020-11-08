using CliFx;
using Polytet.Communication;
using Polytet.Communication.Messages;
using System;
using System.Net;
using System.Net.Sockets;

namespace Polytet.Player
{
	class ConnectionReactor : DefaultReactionReactor
	{
		private readonly IConsole console;

		private readonly IPAddress ipAddress;
		private readonly int port;
		private readonly int? localPort;

		private ConnectServer? serverState;

		public ConnectionReactor(IConsole console, IPAddress ipAddress, int port, int? localPort)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
			this.ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
			this.port = port;
			this.localPort = localPort;
		}

		[Header(0b_1000_0000)]
		public void ReceiveConnect(ConnectServer message)
		{
			serverState = message;
		}

		public PlayReactor? Connect()
		{
			TcpClient client = localPort is int port ? new TcpClient(new IPEndPoint(IPAddress.Any, port)) : new TcpClient();
			console.Output.WriteLine($"Connecting to {ipAddress}:{this.port}...");
			client.Connect(new IPEndPoint(ipAddress, this.port));
			console.Output.WriteLine("Connected!");

			TCPMessageSender messageSender = new TCPMessageSender(client.GetStream());
			messageSender.QueueMessage(new ConnectClient());
			console.Output.WriteLine("Hailing server...");
			messageSender.SendOneMessage();

			TCPMessagePasser messagePasser = new TCPMessagePasser(client.GetStream(), this);
			while (!serverState.HasValue)
			{
				console.Output.WriteLine("Listening for response...");
				messagePasser.ListenAndReactClient();
			}

			ConnectServer ss = serverState.Value;
			if (ss.CanHost)
			{
				return new PlayReactor(client, ss);
			}
			else
			{
				return null;
			}
		}
	}
}