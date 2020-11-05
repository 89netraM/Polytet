using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using ConsoleElmish.Common;
using System;

namespace Polytet.Player
{
	using Model;

	class GameComponent : Component<GameState>
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

		private readonly Game game;
		private readonly Player player;

		public GameComponent(Side side) : base(new GameState(side))
		{
			game = new Game();
			game.Update += Game_Update;

			player = new Player(game, 1000);
			player.GameOver += Player_GameOver;

			player.Start();
		}

		private void Game_Update(Game.UpdateReason reason)
		{
			if (reason == Game.UpdateReason.ScoreChange)
			{
				State = new GameState(State.Side switch
				{
					Side.Left => Side.Right,
					Side.Right => Side.Left,
					_ => throw new NotImplementedException()
				});
			}
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
				{ new Area(0, MainColumn(State.Side), 22, 22), new BorderComponent(new PlayComponent(game)) },
				{ new Area(0, SidebarColumn(State.Side), 4, 12), new BorderComponent(new NextPieceComponent(game)) },
				{ new Area(3, SidebarColumn(State.Side), 4, 12), new BorderComponent(new ScoreComponent(game)) },
				{ new Area(0, CenterColumn(State.Side), 1, 1), '╦' },
				{ new Area(3, CenterColumn(State.Side), 1, 1), InnerConector(State.Side) },
				{ new Area(3, SidebarOuterColumn(State.Side), 1, 1), OuterConector(State.Side) },
				{ new Area(6, CenterColumn(State.Side), 1, 1), InnerConector(State.Side) }
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

	readonly struct GameState
	{
		public GameComponent.Side Side { get; }

		public GameState(GameComponent.Side side)
		{
			Side = side;
		}
	}
}