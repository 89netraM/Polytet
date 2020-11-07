using System;

namespace Polytet.Communication
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
	public class HeaderAttribute : Attribute
	{
		public byte HeaderCode { get; }
		internal MessageReceiver From { get; }

		public HeaderAttribute(byte headerCode) : this(headerCode, MessageReceiver.Unknown) { }
		internal HeaderAttribute(byte headerCode, MessageReceiver from)
		{
			HeaderCode = headerCode;
			From = from;
		}

		public static implicit operator ValueTuple<byte, MessageReceiver>(HeaderAttribute header)
		{
			return (header.HeaderCode, header.From);
		}
	}

	public enum MessageReceiver
	{
		Unknown,
		Server,
		Client
	}
}