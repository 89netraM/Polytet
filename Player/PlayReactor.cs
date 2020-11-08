using ConsoleElmish;
using Polytet.Communication;
using Polytet.Communication.Messages;
using System;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polytet.Player
{
	class PlayReactor : DefaultReactionReactor, IDisposable
	{
		private TcpClient client;
		private TCPMessagePasser messagePasser;
		private TCPMessageSender messageSender;

		private BigInteger playerId;

		private MainComponent main;

		public PlayReactor(TcpClient client, ConnectServer ss)
		{
			playerId = ss.PlayerId;

			this.client = client;
			messagePasser = new TCPMessagePasser(this.client.GetStream(), this);
			messagePasser.PlayerIntegerSize = ss.PlayerIntegerSize;
			messageSender = new TCPMessageSender(this.client.GetStream());
			messageSender.PlayerIntegerSize = ss.PlayerIntegerSize;

			main = new MainComponent();
		}

		public void Start()
		{
			main.SendMessage += Main_SendMessage;

			Renderer renderer = Renderer.Create(PlayCommand.Height, PlayCommand.Width);

			Console.OutputEncoding = Encoding.UTF8;
			Console.CursorVisible = false;
			Console.CancelKeyPress += (s, e) =>
			{
				renderer.Stop();
				Console.CursorVisible = true;
				Environment.Exit(0);
			};

			main.Start();
			renderer.Render(main);

			Task.Run(Send);
			Task.Run(Listen);
		}
		private void Send()
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
				Dispose();
			}
		}
		private void Listen()
		{
			try
			{
				while (true)
				{
					messagePasser.ListenAndReactClient();
				}
			}
			catch (Exception ex) when (!(ex is ObjectDisposedException))
			{
				Dispose();
			}
		}

		[Header(0b_1000_0001)]
		public void ReceivePeerConnected(PeerConnectedServer message)
		{
			main.AddChatMessage(message.AffectedPlayerId, $"Player {message.AffectedPlayerId} {(message.IsConnecting ? "joined" : "left")}");
		}

		[Header(0b_0000_0000)]
		public void ReceiveChatMessage(ChatMessageClient message)
		{
			main.AddChatMessage(message.SenderPlayerId, message.ChatMessage);
		}
		private void Main_SendMessage(string message)
		{
			main.AddChatMessage(playerId, message);
			messageSender.QueueMessage(new ChatMessageClient(playerId, message));
		}

		public void Dispose()
		{
			main.SendMessage -= Main_SendMessage;

			client.Close();
			client.Dispose();
		}
	}
}