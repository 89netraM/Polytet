using ConsoleElmish;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Polytet.Player
{
	public static class Program
	{
		private const int Width = 22;
		private const int Height = 22;

		public static Task Main()
		{
			if (Console.WindowHeight < Height || Console.WindowWidth < Width)
			{
				Console.WriteLine("Window size must be at least 22x22");
				return Task.CompletedTask;
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

				renderer.Render(new GameComponent());

				return Task.Delay(-1);
			}
		}
	}
}