using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using ConsoleElmish.Common;
using System;

namespace Polytet.Player
{
	using Model;

	class GameComponent : Component<EmptyState>
	{
		private readonly Game game;
		private readonly Player player;

		public GameComponent() : base()
		{
			game = new Game();

			player = new Player(game, 1000);
			player.GameOver += Player_GameOver;
		}

		private void Player_GameOver()
		{
			Renderer.Instance.Stop();
			Environment.Exit(0);
		}

		public override Buffer Render(uint height, uint width)
		{
			player.Start();

			return new Buffer
			{
				{ new Area(0, 0, height, width), new BorderComponent(new PlayComponent(game)) },
				{ new Area(0, 0, 1, width), new TextComponent("Polytet", true) }
			};
		}
	}
}