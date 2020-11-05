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

			player.Start();
		}

		private void Player_GameOver()
		{
			Renderer.Instance.Stop();
			Environment.Exit(0);
		}

		public override Buffer Render(uint height, uint width)
		{
			return new Buffer
			{
				{ new Area(0, 0, 22, 22), new BorderComponent(new PlayComponent(game)) },
				{ new Area(0, 21, 4, 12), new BorderComponent(new NextPieceComponent(game)) },
				{ new Area(0, 21, 1, 1), '╦' },
				{ new Area(3, 21, 1, 1), '╠' }
			};
		}

		public override void Dispose()
		{
			player.GameOver -= Player_GameOver;
		}
	}
}