using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using ConsoleElmish.Common;
using System;

namespace Polytet.Player
{
	using Model;

	class GameComponent : Component<EmptyState>
	{
		private static uint MainColumn(Side side) => side switch
		{
			Side.Left => 0,
			Side.Right => 11,
			_ => throw new NotImplementedException()
		};
		private static uint SidebarColumn(Side side) => side switch
		{
			Side.Left => 21,
			Side.Right => 0,
			_ => throw new NotImplementedException(),
		};
		private static uint CenterColumn(Side side) => side switch
		{
			Side.Left => SidebarColumn(side),
			Side.Right => MainColumn(side),
			_ => throw new NotImplementedException(),
		};
		private static uint SidebarOuterColumn(Side side) => side switch
		{
			Side.Left => 32,
			Side.Right => 0,
			_ => throw new NotImplementedException(),
		};
		private static char InnerConector(Side side) => side switch
		{
			Side.Left => '╠',
			Side.Right => '╣',
			_ => throw new NotImplementedException(),
		};
		private static char OuterConector(Side side) => side switch
		{
			Side.Left => '╣',
			Side.Right => '╠',
			_ => throw new NotImplementedException(),
		};

		private readonly Side side;

		private readonly Game game;
		private readonly Player player;

		public GameComponent(Side side) : base()
		{
			this.side = side;

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
				{ new Area(0, MainColumn(side), 22, 22), new BorderComponent(new PlayComponent(game)) },
				{ new Area(0, SidebarColumn(side), 4, 12), new BorderComponent(new NextPieceComponent(game)) },
				{ new Area(3, SidebarColumn(side), 4, 12), new BorderComponent(new ScoreComponent(game)) },
				{ new Area(0, CenterColumn(side), 1, 1), '╦' },
				{ new Area(3, CenterColumn(side), 1, 1), InnerConector(side) },
				{ new Area(3, SidebarOuterColumn(side), 1, 1), OuterConector(side) },
				{ new Area(6, CenterColumn(side), 1, 1), InnerConector(side) }
			};
		}

		public override void Dispose()
		{
			player.GameOver -= Player_GameOver;
		}

		public enum Side
		{
			Left,
			Right
		}
	}
}