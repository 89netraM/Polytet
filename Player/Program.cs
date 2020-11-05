using ConsoleElmish;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Polytet.Player
{
	public static class Program
	{
		private const int Width = 34;
		private const int Height = 22;

		public static Task Main(string[] args)
		{
			if (Console.WindowHeight < Height || Console.WindowWidth < Width)
			{
				Console.WriteLine($"Window size must be at least {Height}x{Width}");
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

				renderer.Render(new GameComponent(GetSide(args)));

				return Task.Delay(-1);
			}
		}

		private static GameComponent.Side GetSide(string[] args)
		{
			if (args.Length == 0)
			{
				return GameComponent.Side.Left;
			}
			else if (args.Length == 1 && Enum.TryParse(Char.ToUpper(args[0][0]) + args[0][1..].ToLower(), out GameComponent.Side side))
			{
				return side;
			}
			else
			{
				Console.WriteLine("Provide no argument or Left|Right");
				Environment.Exit(-1);
				return (GameComponent.Side)2;
			}
		}
	}
}