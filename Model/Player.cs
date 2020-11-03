using System;
using System.Timers;

namespace Polytet.Model
{
	public class Player : IDisposable
	{
		private readonly Game game;
		private readonly Random random;
		private readonly Timer timer;

		public event Action? GameOver;

		public Player(Game game, int millisecondInterval, Random? random = null)
		{
			this.game = game ?? throw new ArgumentNullException(nameof(game));
			this.random = random ?? new Random();

			timer = new Timer(millisecondInterval);
			timer.Elapsed += Timer_Elapsed;
		}

		public void Start()
		{
			if (!timer.Enabled)
			{
				EnsureHasNextPiece();

				timer.Start();
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			bool success = game.Tick();

			if (success)
			{
				EnsureHasNextPiece();
			}
			else
			{
				Stop();

				GameOver?.Invoke();
			}
		}

		private void EnsureHasNextPiece()
		{
			if (!game.NextPiece.HasValue)
			{
				game.AddCommingPiece((Piece)random.Next((int)Piece.I, (int)Piece.Z + 1));
			}
		}

		public void Stop()
		{
			if (timer.Enabled)
			{
				timer.Stop();
			}
		}

		public void Dispose()
		{
			Stop();
			timer.Dispose();
		}
	}
}