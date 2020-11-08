using Buffer = ConsoleElmish.Buffer;
using ConsoleElmish;
using System;
using System.Collections.Immutable;
using System.Numerics;
using System.Linq;
using ConsoleElmish.Common;

namespace Polytet.Player
{
	class ChatComponent : Component<ChatState>
	{
		public event Action<string?>? SendMessage;
		public bool ShouldAcceptInput { get; }
		public IImmutableList<(BigInteger id, string message)> Messages { get; }

		public ChatComponent(bool shouldAcceptInput, IImmutableList<(BigInteger, string)> messages) : base(new ChatState(null))
		{
			ShouldAcceptInput = shouldAcceptInput;
			Messages = messages;

			Input.KeyDown += Input_KeyDown;
			Input.Start();
		}

		private void Input_KeyDown(ConsoleKeyInfo obj)
		{
			if (ShouldAcceptInput)
			{
				if (obj.Key == ConsoleKey.Enter)
				{
					if (!(State.Text is null))
					{
						SendMessage?.Invoke(State.Text.Length == 0 ? null : State.Text);
						State = new ChatState(null);
					}
					else
					{
						SendMessage?.Invoke(null);
					}
				}
				else if (obj.Key == ConsoleKey.Backspace)
				{
					if (!(State.Text is null) && State.Text.Length > 0)
					{
						State = new ChatState(State.Text.Substring(0, State.Text.Length - 1));
					}
				}
				else if (obj.KeyChar is char c)
				{
					if (obj.Modifiers.HasFlag(ConsoleModifiers.Shift))
					{
						c = Char.ToUpper(c);
					}

					State = new ChatState(State.Text is null ? c.ToString() : State.Text + c);
				}
			}
		}

		public override Buffer Render(uint height, uint width)
		{
			Buffer buffer = new Buffer();

			uint historyHeight = height - 2;
			(BigInteger, string)[] history = Messages.TakeLast((int)historyHeight).ToArray();
			uint r = historyHeight - (uint)history.Length;
			foreach (var (id, message) in history)
			{
				buffer.Add(new Area(r, 0, 1, width), new TextComponent(message).WithColors(foreground: (ConsoleColor)((int)id % 15 + 1)));
				r++;
			}

			buffer.Add(new Area(r, 0, 1, width), '─');
			r++;

			if (State.Text is null)
			{
				if (ShouldAcceptInput)
				{
					buffer.Add(new Area(r, 0, 1, width), new TextComponent("Write something").WithColors(foreground: ConsoleColor.DarkGray));
				}
				else
				{
					buffer.Add(new Area(r, 0, 1, width), new TextComponent("Press enter to chat").WithColors(foreground: ConsoleColor.DarkGray));
				}
			}
			else
			{
				buffer.Add(new Area(r, 0, 1, width), new TextComponent(State.Text));
			}

			return buffer;
		}

		public override void Dispose()
		{
			Input.KeyDown -= Input_KeyDown;
		}
	}

	readonly struct ChatState
	{
		public string? Text { get; }

		public ChatState(string? text)
		{
			Text = text;
		}
	}
}