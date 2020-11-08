using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using Polytet.Model;
using System;
using System.Collections.Generic;
using Polytet.Communication.Messages;

namespace Polytet.Player
{
	class PlayComponent : Component<EmptyState>
	{
		public static IReadOnlyDictionary<Piece, ConsoleColor> PieceColorMap { get; } = new Dictionary<Piece, ConsoleColor>
		{
			{ Piece.I, ConsoleColor.Cyan },
			{ Piece.J, ConsoleColor.DarkBlue },
			{ Piece.L, ConsoleColor.DarkYellow },
			{ Piece.O, ConsoleColor.Yellow },
			{ Piece.S, ConsoleColor.Green },
			{ Piece.T, ConsoleColor.DarkMagenta },
			{ Piece.Z, ConsoleColor.Red }
		};

		private readonly Game game;
		public bool ShouldAcceptInput { get; }
		private readonly Action<Move>? makeMove;

		public PlayComponent(Game game, bool shouldAcceptInput, Action<Move>? makeMove) : base()
		{
			this.game = game ?? throw new ArgumentNullException(nameof(game));

			this.game.Update += Game_Update;

			ShouldAcceptInput = shouldAcceptInput;

			this.makeMove = makeMove;

			Input.KeyDown += Input_KeyDown;
			Input.Start();
		}

		private void Input_KeyDown(ConsoleKeyInfo obj)
		{
			if (ShouldAcceptInput)
			{
				switch (obj.Key)
				{
					case ConsoleKey.LeftArrow:
					case ConsoleKey.A:
						makeMove?.Invoke(Move.MoveLeft);
						break;
					case ConsoleKey.RightArrow:
					case ConsoleKey.D:
						makeMove?.Invoke(Move.MoveRight);
						break;
					case ConsoleKey.DownArrow:
					case ConsoleKey.S:
						makeMove?.Invoke(Move.MoveDown);
						break;
					case ConsoleKey.OemComma:
					case ConsoleKey.Q:
						makeMove?.Invoke(Move.RotateCounterClockwise);
						break;
					case ConsoleKey.OemPeriod:
					case ConsoleKey.E:
						makeMove?.Invoke(Move.RotateClockwise);
						break;
					default:
						break;
				}
			}
		}

		private void Game_Update(Game.UpdateReason reason)
		{
			switch (reason)
			{
				case Game.UpdateReason.Tick:
				case Game.UpdateReason.RotateCounterClockwise:
				case Game.UpdateReason.RotateClockwise:
				case Game.UpdateReason.MoveLeft:
				case Game.UpdateReason.MoveRight:
				case Game.UpdateReason.MoveDown:
					ForceReRender();
					break;
				default:
					break;
			}
		}

		public override Buffer Render(uint height, uint width)
		{
			Buffer buffer = new Buffer();

			if (game.Floating.HasValue)
			{
				try
				{
					foreach (var (x, y) in game.GetPositionsOfPiece(y: game.PreviewDropY()))
					{
						buffer[((uint)y - 20, (uint)x * 2)] = '\u2591'.WithColors(foreground: PieceColorMap[game.Floating.Value.piece]);
						buffer[((uint)y - 20, (uint)x * 2 + 1)] = '\u2591'.WithColors(foreground: PieceColorMap[game.Floating.Value.piece]);
					}
				} catch { }
			}

			for (int y = 0; y < 20; y++)
			{
				for (int x = 0; x < 10; x++)
				{
					Piece piece = game[x, y + 20];
					if (piece != Piece.Empty)
					{
						buffer[((uint)y, (uint)x * 2)] = '█'.WithColors(foreground: PieceColorMap[piece]);
						buffer[((uint)y, (uint)x * 2 + 1)] = '█'.WithColors(foreground: PieceColorMap[piece]);
					}
				}
			}

			return buffer;
		}

		public override void Dispose()
		{
			game.Update -= Game_Update;

			Input.KeyDown -= Input_KeyDown;
		}
	}
}