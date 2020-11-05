using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using Polytet.Model;
using ConsoleElmish.Common;

namespace Polytet.Player
{

	class ScoreComponent : Component<EmptyState>
	{
		private readonly Game game;

		public ScoreComponent(Game game)
		{
			this.game = game;

			this.game.Update += Game_Update;
		}

		private void Game_Update(Game.UpdateReason reason)
		{
			if (reason == Game.UpdateReason.ScoreChange)
			{
				ForceReRender();
			}
		}

		public override Buffer Render(uint height, uint width)
		{
			return new Buffer
			{
				{ new Area(0, 0, 1, width), new TextComponent("Score:") },
				{ new Area(1, 0, 1, width), new TextComponent(game.Score.ToString(), TextComponent.Alignment.Right) }
			};
		}

		public override void Dispose()
		{
			game.Update -= Game_Update;
		}
	}
}