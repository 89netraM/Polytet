using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polytet.Communication.Messages;
using Polytet.Model;
using System.Linq;

namespace Polytet.Communication.UnitTests
{
	[TestClass]
	public class Messages
	{
		private const byte playerIntegerSize = 4;

		[TestMethod]
		public void SerializeAndDeSerializeConnectServerMessages()
		{
			ConnectServer connect = new ConnectServer(true, playerIntegerSize, 0, 1);

			byte[] bytes = Serializer.Serialize(connect, 0);
			ConnectServer message = (ConnectServer)Serializer.DeSerializeClient(bytes, 0);

			Assert.AreEqual(connect, message, "Should be able to serialize and deserialize the connect server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeConnectClientMessages()
		{
			ConnectClient connect = new ConnectClient();

			byte[] bytes = Serializer.Serialize(connect, 0);
			ConnectClient message = (ConnectClient)Serializer.DeSerializeServer(bytes, 0);

			Assert.AreEqual(connect, message, "Should be able to serialize and deserialize the connect client message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializePeerConnectedServerMessages()
		{
			PeerConnectedServer peerConnected = new PeerConnectedServer(true, 2, 1);

			byte[] bytes = Serializer.Serialize(peerConnected, playerIntegerSize);
			PeerConnectedServer message = (PeerConnectedServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(peerConnected, message, "Should be able to serialize and deserialize the peer connected server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeStartGameMessages()
		{
			StartGame startGame = new StartGame();

			byte[] bytes = Serializer.Serialize(startGame, playerIntegerSize);
			StartGame messageServer = (StartGame)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(startGame, messageServer, "Should be able to serialize and deserialize the start game server message.");

			StartGame messageClient = (StartGame)Serializer.DeSerializeServer(bytes, playerIntegerSize);

			Assert.AreEqual(startGame, messageClient, "Should be able to serialize and deserialize the start game client message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeNextPieceServerMessages()
		{
			NextPieceServer nextPiece = new NextPieceServer(0, Piece.I);

			byte[] bytes = Serializer.Serialize(nextPiece, playerIntegerSize);
			NextPieceServer message = (NextPieceServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(nextPiece, message, "Should be able to serialize and deserialize the next piece server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeTickServerMessages()
		{
			TickServer tick = new TickServer();

			byte[] bytes = Serializer.Serialize(tick, playerIntegerSize);
			TickServer message = (TickServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(tick, message, "Should be able to serialize and deserialize the tick server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeMoveServerMessages()
		{
			MoveServer move = new MoveServer(0, Move.NotAllowed);

			byte[] bytes = Serializer.Serialize(move, playerIntegerSize);
			MoveServer message = (MoveServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(move, message, "Should be able to serialize and deserialize the move server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeMoveClientMessages()
		{
			MoveClient move = new MoveClient(Move.MoveRight);

			byte[] bytes = Serializer.Serialize(move, playerIntegerSize);
			MoveClient message = (MoveClient)Serializer.DeSerializeServer(bytes, playerIntegerSize);

			Assert.AreEqual(move, message, "Should be able to serialize and deserialize the move client message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializePointsUpdateServerMessages()
		{
			PointsUpdateServer pointsUpdate = new PointsUpdateServer(0, 1200);

			byte[] bytes = Serializer.Serialize(pointsUpdate, playerIntegerSize);
			PointsUpdateServer message = (PointsUpdateServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(pointsUpdate, message, "Should be able to serialize and deserialize the points update server message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeStateUpdateServerMessages()
		{
			Game game = new Game();
			game.AddCommingPiece(Piece.I);
			game.Tick();
			game.AddCommingPiece(Piece.J);
			game.MoveDown();
			game.Tick();
			game.Tick();

			StateUpdateServer stateUpdate = StateUpdateServer.CreateState(0, game);

			byte[] bytes = Serializer.Serialize(stateUpdate, playerIntegerSize);
			StateUpdateServer message = (StateUpdateServer)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.IsTrue(AreStateUpdatesEqual(stateUpdate, message), "Should be able to serialize and deserialize the state update server message.");

			Game messageGame = message.ToGame();
			Assert.AreEqual(game.Floating, messageGame.Floating, "Should produce an exact copy of the floating piece.");
			Assert.IsTrue(game.ClonePlayField().SequenceEqual(messageGame.ClonePlayField()), "Should produce an exact copy of the playfield.");

			static bool AreStateUpdatesEqual(StateUpdateServer a, StateUpdateServer b) =>
				a.PlayerId.Equals(b.PlayerId) &&
				a.Playfield.SequenceEqual(b.Playfield) &&
				a.FloatingPiece == b.FloatingPiece &&
				b.XPosition == b.XPosition &&
				b.YPosition == b.YPosition &&
				b.Rotation == b.Rotation;
	}

		[TestMethod]
		public void SerializeAndDeSerializeStateUpdateClientMessages()
		{
			StateUpdateClient stateUpdate = new StateUpdateClient(1);

			byte[] bytes = Serializer.Serialize(stateUpdate, playerIntegerSize);
			StateUpdateClient message = (StateUpdateClient)Serializer.DeSerializeServer(bytes, playerIntegerSize);

			Assert.AreEqual(stateUpdate, message, "Should be able to serialize and deserialize the state update client message.");
		}

		[TestMethod]
		public void SerializeAndDeSerializeChatMessageClientMessages()
		{
			ChatMessageClient chatMessage = new ChatMessageClient(0, "Hello World!");

			byte[] bytes = Serializer.Serialize(chatMessage, playerIntegerSize);
			ChatMessageClient message = (ChatMessageClient)Serializer.DeSerializeClient(bytes, playerIntegerSize);

			Assert.AreEqual(chatMessage, message, "Should be able to serialize and deserialize the chat message client message.");
		}
	}
}