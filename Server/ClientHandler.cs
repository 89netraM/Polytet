using CliFx;
using Polytet.Communication;
using Polytet.Communication.Messages;
using Polytet.Model;
using System;
using System.Numerics;

namespace Polytet.Server
{
	public class ClientHandler : IReactor
	{
		private const int tickInterval = 1000;

		public event Action? Connect;
		public event Action? StartGame;
		public event Action<byte[]>? PassToPeers;
		public event Action<IMessage>? Broadcast;
		public bool IsConnected { get; set; } = false;
		public BigInteger Id { get; set; }

		private Game game;
		private Player player;

		private readonly IConsole console;
		public TCPMessageSender MessageSender { get; }
		private readonly Func<BigInteger, StateUpdateServer> getStateOf;

		public ClientHandler(IConsole console, TCPMessageSender messageSender, Func<BigInteger, StateUpdateServer> getStateOf)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
			MessageSender = messageSender;
			this.getStateOf = getStateOf;

			game = new Game();
			game.Update += Game_Update;
			player = new Player(game, tickInterval);
		}

		public void Start()
		{
			player.Start();
		}

		private void Game_Update(Game.UpdateReason reason)
		{
			switch (reason)
			{
				case Game.UpdateReason.Tick:
					MessageSender.QueueMessage(new TickServer());
					break;
				case Game.UpdateReason.NextPieceChange:
					if (game.NextPiece is Piece nextPiece)
					{
						Broadcast?.Invoke(new NextPieceServer(Id, nextPiece));
					}
					break;
				case Game.UpdateReason.ScoreChange:
					Broadcast?.Invoke(new PointsUpdateServer(Id, (uint)game.Score));
					break;
			}
		}

		[Header(0b_1000_0000)]
		public void ReceiveConnect(ConnectClient message)
		{
			if (!IsConnected)
			{
				Connect?.Invoke();
			}
		}

		[Header(0b_1000_0010)]
		public void ReceiveStartGame(StartGame message)
		{
			if (Id == 0)
			{
				StartGame?.Invoke();
			}
		}

		[Header(0b_1000_0101)]
		public void ReceiveMove(MoveClient message)
		{
			bool allowed = message.Move switch
			{
				Move.MoveDown => game.MoveDown(),
				Move.MoveLeft => game.MoveLeft(),
				Move.MoveRight => game.MoveRight(),
				Move.RotateClockwise => game.RotateClockwise(),
				Move.RotateCounterClockwise => game.RotateCounterClockwise(),
				_ => false
			};

			if (allowed)
			{
				Broadcast?.Invoke(new MoveServer(Id, message.Move));
			}
			else
			{
				MessageSender.QueueMessage(new MoveServer(Id, Move.NotAllowed));
			}
		}

		[Header(0b_1000_0111)]
		public void ReceiveStateUpdate(StateUpdateClient message)
		{
			MessageSender.QueueMessage(getStateOf(message.PlayerId));
		}

		public StateUpdateServer GetState() => StateUpdateServer.CreateState(Id, game);

		public void DefaultReaction(byte[] bytes)
		{
			if (bytes.Length > 0)
			{
				if (Serializer.IntendedReceiver(bytes) == MessageReceiver.Server)
				{
					string binary = Convert.ToString(bytes[0], 2).PadLeft(8, '0');
					console.Error.WriteLine($"Received unknown message. Header 0b_{binary.Substring(0, 4)}_{binary.Substring(4)}.");
				}
				else
				{
					PassToPeers?.Invoke(bytes);
				}
			}
		}
	}
}