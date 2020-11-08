using CliFx;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{
		public static Task<int> Main() =>
			new CliApplicationBuilder()
				.AddCommandsFromThisAssembly()
				.Build()
				.RunAsync()
				.AsTask();
	}
}