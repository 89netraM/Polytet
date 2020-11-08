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
		private readonly Action<byte[]> defaultReaction;

		public TCPMessagePasser(NetworkStream networkStream, IReactor reactor)
		{
			this.networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
			messageTypes = FindReactorMethods(reactor);
			defaultReaction = reactor.DefaultReaction;

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
			byte[] size = new byte[4];
			int readSize = networkStream.Read(size, 0, size.Length);
			if (readSize != size.Length)
			{
				throw new Exception("Stream end");
			}

			int messageLength = 0;
			for (int i = 0; i < size.Length; i++)
			{
				messageLength |= size[i] << (24 - 8 * i);
			}

			byte[] bytes = new byte[messageLength];
			int readBytes = networkStream.Read(bytes, 0, messageLength);
			if (readBytes != messageLength)
			{
				throw new Exception("Stream end");
			}

			try
			{
				IMessage message = Serializer.DeSerialize(bytes, PlayerIntegerSize, receiver);

				if (messageTypes.TryGetValue(bytes[0], out React value))
				{
					value(message);
					return;
				}
			}
			catch { }

			defaultReaction(bytes);
		}
	}
}