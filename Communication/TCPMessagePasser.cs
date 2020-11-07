using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace Polytet.Communication
{
	public class TCPMessagePasser
	{
		private const int bufferSize = 256;

		private delegate void React(IMessage message);

		public byte PlayerIntegerSize { get; set; } = 0;
		private readonly NetworkStream networkStream;
		private readonly IReadOnlyDictionary<byte, React> messageTypes;

		public TCPMessagePasser(NetworkStream networkStream, object reactor)
		{
			this.networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
			messageTypes = FindReactorMethods(reactor);

			static IReadOnlyDictionary<byte, React> FindReactorMethods(object reactor)
			{
				return reactor.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SelectMany(SearchForMethods).ToDictionary(t => t.header, t => t.react);

				IEnumerable<(byte header, React react)> SearchForMethods(MethodInfo info)
				{
					if (info.GetParameters() is ParameterInfo[] ps &&
						ps.Length == 1 &&
						typeof(IMessage).IsAssignableFrom(ps[0].ParameterType))
					{
						foreach (HeaderAttribute header in Attribute.GetCustomAttributes(info, typeof(HeaderAttribute)))
						{
							yield return (
								header.HeaderCode,
								m => info.Invoke(reactor, new object[] { m })
							);
						}
					}
				}
			}
		}

		public void ListenAndReactServer() => ListenAndReact(MessageReceiver.Server);
		public void ListenAndReactClient() => ListenAndReact(MessageReceiver.Client);
		internal void ListenAndReact(MessageReceiver receiver)
		{
			IList<(byte[] buffer, int length)> buffers = new List<(byte[], int)>();
			do
			{
				byte[] buffer = new byte[bufferSize];

				int length;
				do
				{
					length = networkStream.Read(buffer, 0, bufferSize);
				} while (length == 0);

				buffers.Add((buffer, length));
			} while (networkStream.DataAvailable);

			byte[] bytes = new byte[buffers.Sum(t => t.length)];
			int i = 0;
			foreach (var (buffer, length) in buffers)
			{
				Array.Copy(buffer, 0, bytes, i, length);
				i += length;
			}

			IMessage message = Serializer.DeSerialize(bytes, PlayerIntegerSize, receiver);

			if (messageTypes.TryGetValue(bytes[0], out React value))
			{
				value(message);
			}
		}
	}
}