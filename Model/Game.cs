﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytet.Model
{
	public class Game
	{
		private Playfield playfield;
		private (Piece piece, int x, int y, int rotation)? floating;

		private readonly Queue<Piece> nextPieces = new Queue<Piece>();
		public Piece? NextPiece => nextPieces.Count > 0 ? (Piece?)nextPieces.Peek() : null;

		public Piece this[int x, int y]
		{
			get
			{
				Piece fromField = playfield[x, y];

				if (fromField == Piece.Empty)
				{
					if (floating.HasValue && GetPositionsOfPiece().Any(p => p.x == x && p.y == y))
					{
						return floating.Value.piece;
					}
					else
					{
						return Piece.Empty;
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
			floating = null;
		}

		public void AddCommingPiece(Piece piece)
		{
			nextPieces.Enqueue(piece);
			Update?.Invoke(UpdateReason.NewNextPiece);
		}

		private IEnumerable<(int x, int y)> GetPositionsOfPiece(int? rotation = null, int? x = null, int? y = null)
		{
			if (!floating.HasValue)
			{
				throw new InvalidOperationException();
			}

			return floating.Value.piece
				.GetOffsets(rotation ?? floating.Value.rotation)
				.Select(p => (p.x + x ?? floating.Value.x, p.y + y ?? floating.Value.y));
		}

		private void PlacePiece()
		{
			if (!floating.HasValue)
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
				playfield[x, y] = floating.Value.piece;
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

		public void Tick()
		{
			if (floating.HasValue)
			{
				if (!IsValidPositions(GetPositionsOfPiece().Select(p => (p.x, p.y + 1))))
				{
					PlacePiece();

					floating = null;
				}
				else
				{
					floating = (
						floating.Value.piece,
						floating.Value.x,
						floating.Value.y + 1,
						floating.Value.rotation
					);
				}
			}
			else
			{
				if (nextPieces.Count > 0)
				{
					floating = (
						nextPieces.Dequeue(),
						4,
						20,
						0
					);
				}
			}

			playfield.Tick();

			Update?.Invoke(UpdateReason.Tick);
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
			if (floating.HasValue && IsValidPositions(GetPositionsOfPiece(rotation: floating.Value.rotation + dir)))
			{
				floating = (
					floating.Value.piece,
					floating.Value.x,
					floating.Value.y,
					floating.Value.rotation + dir
				);
				return true;
			}
			else
			{
				return false;
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
			if (floating.HasValue && IsValidPositions(GetPositionsOfPiece(x: floating.Value.x + dir)))
			{
				floating = (
					floating.Value.piece,
					floating.Value.x + dir,
					floating.Value.y,
					floating.Value.rotation
				);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool MoveDown()
		{
			if (floating.HasValue)
			{
				int lastValidY = floating.Value.y;
				for (int y = floating.Value.y + 1; y < Playfield.Height; y++)
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

				if (lastValidY != floating.Value.y)
				{
					floating = (
						floating.Value.piece,
						floating.Value.x,
						lastValidY,
						floating.Value.rotation
					);

					Update?.Invoke(UpdateReason.MoveDown);

					return true;
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