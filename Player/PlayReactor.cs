using ConsoleElmish;
using Polytet.Communication;
using Polytet.Communication.Messages;
using Polytet.Model;
using System;
using System.Diagnostics;
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

		private bool hasNoOpponent = true;
		private BigInteger? opponent;

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
			main.MakeMove += Main_MakeMove;

			Renderer renderer = Renderer.Create(PlayCommand.Height, PlayCommand.Width);

			Console.OutputEncoding = Encoding.UTF8;
			Console.CursorVisible = false;
			Console.CancelKeyPress += (s, e) =>
			{
				renderer.Stop();
				Console.CursorVisible = true;
				Environment.Exit(0);
			};

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

			if (hasNoOpponent && message.IsConnecting)
			{
				hasNoOpponent = false;
				opponent = message.AffectedPlayerId;
			}
			else if (message.IsDisconnecting && opponent == message.AffectedPlayerId)
			{
				opponent = null;
			}
		}

		[Header(0b_1000_0010)]
		public void ReceiveStartGame(StartGame message)
		{
			Debug.WriteLine($"Received: {nameof(StartGame)}");
			main.Start();
		}

		[Header(0b_1000_0011)]
		public void ReceiveNextPiece(NextPieceServer message)
		{
			Debug.WriteLine($"Received: {nameof(NextPieceServer)} ({message.AffectedPlayerId}, {message.Piece})");

			if (message.AffectedPlayerId == playerId)
			{
				main.PlayerGame.AddCommingPiece(message.Piece);
			}
			else if (message.AffectedPlayerId == opponent)
			{
				main.OpponentGame.AddCommingPiece(message.Piece);
			}
		}

		[Header(0b_1000_0100)]
		public void ReceiveTick(TickServer message)
		{
			Debug.WriteLine($"Received: {nameof(TickServer)}");

			main.PlayerGame.Tick();
			if (!(opponent is null))
			{
				main.OpponentGame.Tick();
			}
		}

		[Header(0b_1000_0101)]
		public void ReceiveMove(MoveServer message)
		{
			Debug.WriteLine($"Received: {nameof(MoveServer)} ({message.AffectedPlayerId}, {message.Move})");

			if (message.AffectedPlayerId == playerId)
			{
				MakeMove(main.PlayerGame, message.Move);
			}
			else if (message.AffectedPlayerId == opponent)
			{
				MakeMove(main.OpponentGame, message.Move);
			}

			static void MakeMove(Game game, Move move)
			{
				switch (move)
				{
					case Move.MoveLeft:
						game.MoveLeft();
						break;
					case Move.MoveRight:
						game.MoveRight();
						break;
					case Move.MoveDown:
						game.MoveDown();
						break;
					case Move.RotateCounterClockwise:
						game.RotateCounterClockwise();
						break;
					case Move.RotateClockwise:
						game.RotateClockwise();
						break;
				}
			}
		}
		private void Main_MakeMove(Move move)
		{
			messageSender.QueueMessage(new MoveClient(move));
		}

		[Header(0b_0000_0000)]
		public void ReceiveChatMessage(ChatMessageClient message)
		{
			main.AddChatMessage(message.SenderPlayerId, message.ChatMessage);
		}
		private void Main_SendMessage(string message)
		{
			if (message.StartsWith("/"))
			{
				if (message == "/start")
				{
					messageSender.QueueMessage(new StartGame());
				}
				else
				{
					main.AddChatMessage(playerId, "Unknown command");
				}
			}
			else
			{
				main.AddChatMessage(playerId, message);
				messageSender.QueueMessage(new ChatMessageClient(playerId, message));
			}
		}

		public void Dispose()
		{
			main.SendMessage -= Main_SendMessage;

			client.Close();
			client.Dispose();
		}
	}
}