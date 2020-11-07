using System;

namespace Polytet.Communication
{
	[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
	class HeaderAttribute : Attribute
	{
		public byte HeaderCode { get; }
		public MessageSender From { get; }

		public HeaderAttribute(byte headerCode, MessageSender from)
		{
			HeaderCode = headerCode;
			From = from;
		}

		public static implicit operator ValueTuple<byte, MessageSender>(HeaderAttribute header)
		{
			return (header.HeaderCode, header.From);
		}
	}

	enum MessageSender
	{
		Server,
		Client
	}
}