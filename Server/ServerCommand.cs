using CliFx;
using CliFx.Attributes;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Polytet.Server
{
	[Command]
	public class ServerCommand : ICommand
	{
		[CommandParameter(0)]
		public int Port { get; set; }

		private GameManager? gameManager;

		public ValueTask ExecuteAsync(IConsole console)
		{
			TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
			listener.Start();
			console.Output.WriteLine($"Listening on port {Port}");

			while (true)
			{
				TcpClient client = listener.AcceptTcpClient();

				if (gameManager is null || gameManager.HasStarted)
				{
					gameManager = new GameManager(console);
				}

				console.Output.WriteLine($"Adding {client.Client.RemoteEndPoint} to a game");
				gameManager.AddToGame(client);
			}
		}
	}
}