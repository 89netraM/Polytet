using Polytet.Model;
using System;
using System.Numerics;

namespace Polytet.Communication.Messages
{
	[Header(0b_1000_0111, MessageReceiver.Client)]
	public readonly struct StateUpdateServer : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize + 204)
			{
				throw new Exception("Too short message");
			}

			BigInteger playerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);

			byte[] playfield = new byte[200];
			Array.Copy(message, playerIntegerSize, playfield, 0, 200);

			Piece floatingPiece = (Piece)message[playerIntegerSize + 200];
			if (!Enum.IsDefined(typeof(Piece), floatingPiece))
			{
				throw new Exception($"Malformated enum \"{nameof(Piece)}\"");
			}

			byte xPosition = message[playerIntegerSize + 201];
			byte yPosition = message[playerIntegerSize + 202];
			byte rotation = message[playerIntegerSize + 203];

			return new StateUpdateServer(playerId, playfield, floatingPiece, xPosition, yPosition, rotation);
		}

		public static StateUpdateServer CreateState(BigInteger playerId, Game game)
		{
			var (floatingPiece, xPosition, yPosition, rotation) = game.Floating ?? (Piece.Empty, 0, 0, 0);

			return new StateUpdateServer(
				playerId,
				game.ClonePlayField(),
				floatingPiece,
				(byte)xPosition,
				(byte)yPosition,
				(byte)rotation
			);
		}

		public BigInteger PlayerId { get; }
		public byte[] Playfield { get; }
		public Piece FloatingPiece { get; }
		public byte XPosition { get; }
		public byte YPosition { get; }
		public byte Rotation { get; }

		public StateUpdateServer(BigInteger playerId, byte[] playfield, Piece floatingPiece, byte xPosition, byte yPosition, byte rotation)
		{
			PlayerId = playerId;
			Playfield = playfield ?? throw new ArgumentNullException(nameof(playfield));
			FloatingPiece = floatingPiece;
			XPosition = xPosition;
			YPosition = yPosition;
			Rotation = rotation;
		}

		public Game ToGame()
		{
			(Piece, int, int, int)? floating;
			if (FloatingPiece == Piece.Empty)
			{
				floating = null;
			}
			else
			{
				floating = (
					FloatingPiece,
					XPosition,
					YPosition,
					Rotation
				);
			}

			return new Game(Playfield, floating);
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[playerIntegerSize + 204];

			Helpers.InsertPlayerInteger(message, PlayerId, 0, playerIntegerSize);

			if (!(Playfield is null))
			{
				Playfield.CopyTo(message, playerIntegerSize);
			}

			message[playerIntegerSize + 200] = (byte)FloatingPiece;
			message[playerIntegerSize + 201] = XPosition;
			message[playerIntegerSize + 202] = YPosition;
			message[playerIntegerSize + 203] = Rotation;

			return message;
		}
	}

	[Header(0b_1000_0111, MessageReceiver.Server)]
	public readonly struct StateUpdateClient : IMessage
	{
		internal static IMessage DeSerialize(byte[] message, byte playerIntegerSize)
		{
			if (message.Length < playerIntegerSize)
			{
				throw new Exception("Too short message");
			}

			BigInteger playerId = Helpers.ReadPlayerInteger(message, 0, playerIntegerSize);

			return new StateUpdateClient(playerId);
		}

		public BigInteger PlayerId { get; }

		public StateUpdateClient(BigInteger playerId)
		{
			PlayerId = playerId;
		}

		byte[] IMessage.Serialize(byte playerIntegerSize)
		{
			byte[] message = new byte[playerIntegerSize];

			Helpers.InsertPlayerInteger(message, PlayerId, 0, playerIntegerSize);

			return message;
		}
	}
}