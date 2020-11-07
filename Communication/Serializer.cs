using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Polytet.Communication
{
	public static class Serializer
	{
		private delegate IMessage DeSerializer(byte[] message, byte playerIntegerSize);

		private static readonly Lazy<IReadOnlyDictionary<(byte, MessageReceiver), DeSerializer>> messageTypes = new Lazy<IReadOnlyDictionary<(byte, MessageReceiver), DeSerializer>>(PopulateMessageTypes);

		private static IReadOnlyDictionary<(byte, MessageReceiver), DeSerializer> PopulateMessageTypes()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			return assembly.GetTypes().SelectMany(SearchForHeader).ToDictionary(t => t.header, t => t.deserializer);

			static IEnumerable<((byte, MessageReceiver) header, DeSerializer deserializer)> SearchForHeader(Type t)
			{
				foreach (HeaderAttribute header in Attribute.GetCustomAttributes(t, typeof(HeaderAttribute)))
				{
					DeSerializer? deserializer;
					try
					{
						deserializer = (DeSerializer)t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(IsDeSerializer).CreateDelegate(typeof(DeSerializer));
					}
					catch
					{
						deserializer = null;
					}

					if (!(deserializer is null))
					{
						yield return (header, deserializer);
					}
				}

				static bool IsDeSerializer(MethodInfo info)
				{
					if (info.GetParameters() is ParameterInfo[] ps)
					{
						return info.ReturnType == typeof(IMessage) &&
							info.IsStatic &&
							ps.Length == 2 &&
							ps[0].ParameterType == typeof(byte[]) &&
							ps[1].ParameterType == typeof(byte);
					}
					else
					{
						return false;
					}
				}
			}
		}

		public static byte[] Serialize(IMessage message, byte playerIntegerSize)
		{
			byte[] body = message.Serialize(playerIntegerSize);
			byte header = Attribute.GetCustomAttributes(message.GetType(), typeof(HeaderAttribute)).FirstOrDefault() is HeaderAttribute h ? h.HeaderCode : throw new Exception("Unknown header code");
			byte[] all = new byte[body.Length + 1];
			all[0] = header;
			body.CopyTo(all, 1);
			return all;
		}

		public static MessageReceiver IntendedReceiver(byte[] message)
		{
			if (message.Length == 0)
			{
				return MessageReceiver.Unknown;
			}
			else if ((message[0] & 0b_1000_000) == 0)
			{
				return MessageReceiver.Client;
			}
			else
			{
				return MessageReceiver.Server;
			}
		}

		public static IMessage DeSerializeServer(byte[] message, byte playerIntegerSize) => DeSerialize(message, playerIntegerSize, MessageReceiver.Server);
		public static IMessage DeSerializeClient(byte[] message, byte playerIntegerSize) => DeSerialize(message, playerIntegerSize, MessageReceiver.Client);
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize, MessageReceiver receiver)
		{
			if (message.Length > 0 && messageTypes.Value.TryGetValue((message[0], receiver), out DeSerializer deserializer))
			{
				byte[] body = new byte[message.Length - 1];
				Array.Copy(message, 1, body, 0, body.Length);
				return deserializer(body, playerIntegerSize);
			}
			else
			{
				throw new Exception("Unknown header code");
			}
		}
	}
}