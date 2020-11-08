using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using System;

namespace Polytet.Player
{
	using ConsoleElmish.Common;
	using Polytet.Model;
	using System.Collections.Immutable;
	using System.Numerics;

	class MainComponent : Component<MainState>
	{
		public event Action<string>? SendMessage;

		private readonly Game game;
		private readonly Player player;

		private ChatComponent? chatComponent = null;

		public MainComponent() : base(new MainState(true, ImmutableList.Create<(BigInteger, string)>()))
		{
			game = new Game();

			player = new Player(game, 1000);
			player.GameOver += Player_GameOver;

			Input.KeyDown += Input_KeyDown;
		}

		public void Start()
		{
			player.Start();
			Input.Start();
		}

		private void Input_KeyDown(ConsoleKeyInfo obj)
		{
			if (obj.Key == ConsoleKey.Enter && State.FocusOnGame)
			{
				State = new MainState(false, State.Messages);
			}
		}

		private void Player_GameOver()
		{
			Renderer.Instance.Stop();
			Environment.Exit(0);
		}

		private void ChatComponent_SendMessage(string? message)
		{
			if (!(message is null))
			{
				SendMessage?.Invoke(message);
			}
			else
			{
				State = new MainState(true, State.Messages);
			}
		}
		public void AddChatMessage(BigInteger id, string message)
		{
			State = new MainState(true, State.Messages.Add((id, message)));
		}

		public override Buffer Render(uint height, uint width)
		{
			DisposeOfChatComponent();
			chatComponent = new ChatComponent(!State.FocusOnGame, State.Messages);
			chatComponent.SendMessage += ChatComponent_SendMessage;

			return new Buffer
			{
				{ new Area(0, 0, 22, 33), new GameComponent(GameComponent.Side.Left, game, State.FocusOnGame) },
				{ new Area(0, 32, 22, 33), new GameComponent(GameComponent.Side.Right, game, false) },
				{ new Area(6, 21, 16, 23), new BorderComponent(chatComponent) },
				{ new Area(0, 32, 1, 1), '╦' },
				{ new Area(3, 32, 1, 1), '╬' },
				{ new Area(6, 32, 1, 1), '╩' },
				{ new Area(6, 21, 1, 1), '╠' },
				{ new Area(6, 43, 1, 1), '╣' },
				{ new Area(19, 21, 1, 1), '╟' },
				{ new Area(19, 43, 1, 1), '╢' },
				{ new Area(21, 21, 1, 1), '╩' },
				{ new Area(21, 43, 1, 1), '╩' }
			};
		}

		private void DisposeOfChatComponent()
		{
			if (!(chatComponent is null))
			{
				chatComponent.SendMessage -= ChatComponent_SendMessage;
			}
		}

		public override void Dispose()
		{
			DisposeOfChatComponent();
			player.GameOver -= Player_GameOver;
			Input.KeyDown -= Input_KeyDown;
		}
	}

	readonly struct MainState
	{
		public bool FocusOnGame { get; }
		public IImmutableList<(BigInteger, string)> Messages { get; }

		public MainState(bool focusOnGame, IImmutableList<(BigInteger, string)> messages)
		{
			FocusOnGame = focusOnGame;
			Messages = messages ?? throw new ArgumentNullException(nameof(messages));
		}
	}
}