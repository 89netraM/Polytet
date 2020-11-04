using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Polytet.Model
{
	public class Game
	{
		private Playfield playfield;
		public (Piece piece, int x, int y, int rotation)? Floating { get; private set; }
		private readonly ReaderWriterLockSlim floatingLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		public int Score { get; private set; } = 0;

		private readonly Queue<Piece> nextPieces = new Queue<Piece>();
		public Piece? NextPiece => nextPieces.Count > 0 ? (Piece?)nextPieces.Peek() : null;

		public Piece this[int x, int y]
		{
			get
			{
				Piece fromField = playfield[x, y];

				if (fromField == Piece.Empty)
				{
					floatingLock.EnterReadLock();
					try
					{
						if (Floating.HasValue && GetPositionsOfPiece().Any(p => p.x == x && p.y == y))
						{
							return Floating.Value.piece;
						}
						else
						{
							return Piece.Empty;
						}
					}
					finally
					{
						floatingLock.ExitReadLock();
					}
				}
				else
				{
					return fromField;
				}
			}
		}

		public event Action<UpdateReason>? Update;

		public Game() : this(Playfield.CreateEmpty()) { }
		public Game(byte[] playfield) : this(Playfield.CreateFromSource(playfield)) { }
		private Game(Playfield playfield)
		{
			this.playfield = playfield;
			Floating = null;
		}

		public void AddCommingPiece(Piece piece)
		{
			nextPieces.Enqueue(piece);
			Update?.Invoke(UpdateReason.NewNextPiece);
		}

		public (int x, int y)[] GetPositionsOfPiece(int? rotation = null, int? x = null, int? y = null)
		{
			floatingLock.EnterReadLock();
			try
			{
				if (!Floating.HasValue)
				{
					throw new InvalidOperationException();
				}

				return Floating.Value.piece
					.GetOffsets(rotation ?? Floating.Value.rotation)
					.Select(p => (p.x + (x ?? Floating.Value.x), p.y + (y ?? Floating.Value.y)))
					.ToArray();
			}
			finally
			{
				floatingLock.ExitReadLock();
			}
		}

		private void PlacePiece()
		{
			floatingLock.EnterReadLock();
			try
			{
				if (!Floating.HasValue)
				{
					throw new InvalidOperationException();
				}

				IEnumerable<(int, int)> positions = GetPositionsOfPiece();

				if (!IsValidPositions(positions))
				{
					throw new ArgumentException("Can not place piece there");
				}

				foreach (var (x, y) in positions)
				{
					playfield[x, y] = Floating.Value.piece;
				}
			}
			finally
			{
				floatingLock.ExitReadLock();
			}
		}

		private bool IsValidPositions(IEnumerable<(int, int)> positions)
		{
			return positions.All(IsValidPosition);
		}
		private bool IsValidPosition((int x, int y) position)
		{
			return Playfield.IsInRange(position.x, position.y) &&
				playfield[position.x, position.y] == Piece.Empty;
		}

		public bool Tick()
		{
			bool success = true;

			floatingLock.EnterUpgradeableReadLock();
			try
			{
				if (Floating.HasValue)
				{
					if (PreviewDropY() == Floating.Value.y)
					{
						PlacePiece();

						floatingLock.EnterWriteLock();
						try
						{
							Floating = null;
						}
						finally
						{
							floatingLock.ExitWriteLock();
						}
					}
					else
					{
						floatingLock.EnterWriteLock();
						try
						{
							Floating = (
								Floating.Value.piece,
								Floating.Value.x,
								Floating.Value.y + 1,
								Floating.Value.rotation
							);
						}
						finally
						{
							floatingLock.ExitWriteLock();
						}
					}
				}
				else
				{
					if (nextPieces.Count > 0)
					{
						floatingLock.EnterWriteLock();
						try
						{
							Floating = (
								nextPieces.Dequeue(),
								4,
								20,
								0
							);
						}
						finally
						{
							floatingLock.ExitWriteLock();
						}

						success = IsValidPositions(GetPositionsOfPiece());
					}
				}
			}
			finally
			{
				floatingLock.ExitUpgradeableReadLock();
			}

			int newPoints = playfield.Tick();
			Score += newPoints;

			Update?.Invoke(UpdateReason.Tick);

			return success;
		}

		public int PreviewDropY()
		{
			floatingLock.EnterReadLock();
			try
			{
				if (!Floating.HasValue)
				{
					throw new InvalidOperationException();
				}

				int lastValidY = Floating.Value.y;
				for (int y = Floating.Value.y + 1; y < Playfield.Height; y++)
				{
					if (IsValidPositions(GetPositionsOfPiece(y: y)))
					{
						lastValidY = y;
					}
					else
					{
						break;
					}
				}
				return lastValidY;
			}
			finally
			{
				floatingLock.ExitReadLock();
			}
		}

		public bool RotateCounterClockwise()
		{
			bool result = Rotate(-1);
			if (result)
			{
				Update?.Invoke(UpdateReason.RotateCounterClockwise);
			}
			return result;
		}
		public bool RotateClockwise()
		{
			bool result = Rotate(1);
			if (result)
			{
				Update?.Invoke(UpdateReason.RotateClockwise);
			}
			return result;
		}
		private bool Rotate(int dir)
		{
			floatingLock.EnterUpgradeableReadLock();
			try
			{
				if (Floating.HasValue && IsValidPositions(GetPositionsOfPiece(rotation: Floating.Value.rotation + dir)))
				{
					floatingLock.EnterWriteLock();

					try
					{
						Floating = (
							Floating.Value.piece,
							Floating.Value.x,
							Floating.Value.y,
							Floating.Value.rotation + dir
						);
						return true;
					}
					finally
					{
						floatingLock.ExitWriteLock();
					}
				}
				else
				{
					return false;
				}
			}
			finally
			{
				floatingLock.ExitUpgradeableReadLock();
			}
		}

		public bool MoveLeft()
		{
			bool result = Move(-1);
			if (result)
			{
				Update?.Invoke(UpdateReason.MoveLeft);
			}
			return result;
		}
		public bool MoveRight()
		{
			bool result = Move(1);
			if (result)
			{
				Update?.Invoke(UpdateReason.MoveRight);
			}
			return result;
		}
		private bool Move(int dir)
		{
			floatingLock.EnterUpgradeableReadLock();
			try
			{
				if (Floating.HasValue && IsValidPositions(GetPositionsOfPiece(x: Floating.Value.x + dir)))
				{
					floatingLock.EnterWriteLock();
					try
					{
						Floating = (
							Floating.Value.piece,
							Floating.Value.x + dir,
							Floating.Value.y,
							Floating.Value.rotation
						);
						return true;
					}
					finally
					{
						floatingLock.ExitWriteLock();
					}
				}
				else
				{
					return false;
				}
			}
			finally
			{
				floatingLock.ExitUpgradeableReadLock();
			}
		}

		public bool MoveDown()
		{
			floatingLock.EnterUpgradeableReadLock();
			try
			{
				if (Floating.HasValue)
				{
					int droppedY = PreviewDropY();
					if (droppedY != Floating.Value.y)
					{
						floatingLock.EnterWriteLock();
						try
						{
							Floating = (
								Floating.Value.piece,
								Floating.Value.x,
								droppedY,
								Floating.Value.rotation
							);

							Update?.Invoke(UpdateReason.MoveDown);

							return true;
						}
						finally
						{
							floatingLock.ExitWriteLock();
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			finally
			{
				floatingLock.ExitUpgradeableReadLock();
			}
		}

		public enum UpdateReason
		{
			Tick,
			RotateCounterClockwise,
			RotateClockwise,
			MoveLeft,
			MoveRight,
			MoveDown,
			NewNextPiece
		}
	}
}