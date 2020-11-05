using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using Polytet.Model;
using System;

namespace Polytet.Player
{
	class NextPieceComponent : Component<EmptyState>
	{
		private readonly Game game;

		public NextPieceComponent(Game game) : base()
		{
			this.game = game ?? throw new ArgumentNullException(nameof(game));

			this.game.Update += Game_Update;
		}

		private void Game_Update(Game.UpdateReason reason)
		{
			if (reason == Game.UpdateReason.NextPieceChange)
			{
				ForceReRender();
			}
		}

		public override Buffer Render(uint height, uint width)
		{
			Buffer buffer = new Buffer();

			if (game.NextPiece is Piece nextPiece && nextPiece != Piece.Empty)
			{
				uint offset = nextPiece == Piece.I || nextPiece == Piece.O ? 1u : 2u;
				foreach (var (x, y) in nextPiece.GetOffsets(0))
				{
					buffer[((uint)y, (uint)(x + 1) * 2 + offset)] = '█'.WithColors(foreground: PlayComponent.PieceColorMap[nextPiece]);
					buffer[((uint)y, (uint)(x + 1) * 2 + offset + 1)] = '█'.WithColors(foreground: PlayComponent.PieceColorMap[nextPiece]);
				}
			}

			return buffer;
		}

		public override void Dispose()
		{
			game.Update -= Game_Update;
		}
	}
}