using CliFx;
using System.Threading.Tasks;

namespace Polytet.Player
{
	public static class Program
	{
		public static Task<int> Main() =>
			new CliApplicationBuilder()
				.AddCommandsFromThisAssembly()
				.Build()
				.RunAsync()
				.AsTask();
	}
}