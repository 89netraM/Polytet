#pragma warning disable CliFx0100
using CliFx;
using CliFx.Attributes;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Polytet.Player
{
	[Command]
	public class PlayCommand : ICommand
	{
		public const int Width = 65;
		public const int Height = 22;

		[CommandParameter(0, Description = "The server IP address.")]
		public IPAddress IPAddress { get; set; } = IPAddress.None;
		[CommandParameter(1, Description = "The server port.")]
		public int Port { get; set; } = -1;
		[CommandOption("local-port", 'l', Description = "The local port to use.")]
		public int? LocalPort { get; set; } = null;

		public ValueTask ExecuteAsync(IConsole console)
		{
			if (Console.WindowHeight < Height || Console.WindowWidth < Width)
			{
				console.Output.WriteLine($"Window size must be at least {Height}x{Width}");
				return default;
			}
			else
			{
				ConnectionReactor reactor = new ConnectionReactor(console, IPAddress, Port, LocalPort);
				PlayReactor? playReactor = reactor.Connect();
				if (playReactor is null)
				{
					console.Output.WriteLine("Server can't host right now");
					console.Output.WriteLine("Shuting down");
					return default;
				}
				else
				{
					console.Output.WriteLine("Starting...");
					playReactor.Start();

					return new ValueTask(Task.Delay(-1));
				}
			}
		}
	}
}