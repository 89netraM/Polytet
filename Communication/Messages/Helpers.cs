using System;
using System.Linq;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	static class Helpers
	{
		public static void InsertPlayerInteger(byte[] array, BigInteger integer, int index, int playerIntegerSize)
		{
			byte[] playerCountArray = integer.ToByteArray().Reverse().ToArray();

			if (playerCountArray.Length > playerIntegerSize)
			{
				throw new Exception("Player integer is too big");
			}

			playerCountArray.CopyTo(array, index + (playerIntegerSize - playerCountArray.Length));
		}

		public static BigInteger ReadPlayerInteger(byte[] array, int index, int playerIntegerSize)
		{
			return new BigInteger(array.Skip(index).Take(playerIntegerSize).Reverse().ToArray());
		}
	}
}