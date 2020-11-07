#pragma warning disable CliFx0100
using CliFx;
using CliFx.Attributes;
using ConsoleElmish;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Polytet.Player
{
	[Command]
	public class PlayCommand : ICommand
	{
		private const int Width = 34;
		private const int Height = 22;

		public ValueTask ExecuteAsync(IConsole console)
		{
			if (Console.WindowHeight < Height || Console.WindowWidth < Width)
			{
				Console.WriteLine($"Window size must be at least {Height}x{Width}");
				return default;
			}
			else
			{
				Renderer renderer = Renderer.Create(Height, Width);

				Console.OutputEncoding = Encoding.UTF8;
				Console.CursorVisible = false;
				Console.CancelKeyPress += (s, e) =>
				{
					renderer.Stop();
					Console.CursorVisible = true;
					Environment.Exit(0);
				};

				renderer.Render(new GameComponent(GameComponent.Side.Left));

				return new ValueTask(Task.Delay(-1));
			}
		}
	}
}