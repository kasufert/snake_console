using static System.Console;
using static System.Threading.Thread;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Media;
using System.Diagnostics;
namespace Game;
enum Direction
{
	Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight, Stopped
}
static class snek
{
	const int height = 40;
	const int width = 100;
	const int growEvery = 10;
	static void Main(string[] args)
	{
		WindowHeight = height+2;
		WindowWidth = width+2;
		var direction = Direction.Up;
		var length = 1;
		var (foodX, foodY) = (0, 0);
		var songThread = new Thread(Song);
		songThread.Start();
		var clearRowString = "";
		for (int i = 0; i < width-2; i+=1)
		{
			clearRowString += " ";
		}
		while(true)
		{
			direction = Direction.Up;
			length = 1;
			Clear();
			List<BodyPart> snake = new List<BodyPart>();
			snake.Add(new BodyPart(width/2, height/2));
			CursorVisible = false;
			DrawBorder();
			(foodX, foodY) = PlaceFood(snake, clearRowString);
			SetCursorPosition(11, height - 1);
			Write(snake.Count);
			while (true)
			{
				DrawBoard(snake, direction);
				Sleep(60);
				if (KeyAvailable) GetDirection();
				if (snake[0].IsLocatedAt(foodX - 1, foodY) || snake[0].IsLocatedAt(foodX+1, foodY) || snake[0].IsLocatedAt(foodX+3, foodY))
				{
					length++;
					if (direction is Direction.Up or Direction.Down or Direction.Left or Direction.Right)
						snake.PropagateMovement(direction, true, false);
					else snake.PropagateMovement(direction, true, true);
					SetCursorPosition(foodX, foodY);
					(foodX, foodY) = PlaceFood(snake, clearRowString);
				}
				else
				{
					snake.PropagateMovement(direction, false, false);
				}
				if (snake.Where(s => s.IsLocatedAt(snake[0].xPos, snake[0].yPos)).Count() > 1) break;
				if (snake[0].xPos is (0 or 1 or width or width-1)) break;
				if (snake[0].yPos is (0 or height)) break;
			}
			WriteLine("YOU DIED, Your Length: " + length);
			ReadLine();

		}
		void GetDirection()
		{
			switch(ReadKey(true).Key)
			{
				case ConsoleKey.NumPad5:
				case ConsoleKey.Spacebar:
					direction = Direction.Stopped;
					break;
				case ConsoleKey.NumPad7:
					if (direction != Direction.DownRight)
						direction = Direction.UpLeft;
					break;
				case ConsoleKey.NumPad9:
					if (direction != Direction.DownLeft)
						direction = Direction.UpRight;
					break;
				case ConsoleKey.NumPad1:
					if (direction != Direction.UpRight)
						direction = Direction.DownLeft;
					break;
				case ConsoleKey.NumPad3:
					if (direction != Direction.UpLeft)
						direction = Direction.DownRight;
					break;
				case ConsoleKey.NumPad8:
				case ConsoleKey.UpArrow:
					if (direction != Direction.Down)
						direction = Direction.Up;
					break;
				case ConsoleKey.NumPad2:
				case ConsoleKey.DownArrow:
					if (direction != Direction.Up)
						direction = Direction.Down;
					break;
				case ConsoleKey.NumPad4:
				case ConsoleKey.LeftArrow:
					if (direction != Direction.Right)
						direction = Direction.Left;
					break;
				case ConsoleKey.NumPad6:
				case ConsoleKey.RightArrow:
					if (direction != Direction.Left)
						direction = Direction.Right;
					break;
			}
		}
	}
	static (int,int) PlaceFood(List<BodyPart> snake, string clearRow)
	{
		for (int y = 1; y < height; y++)
		{
			SetCursorPosition(2, y);
			Write(clearRow);
		}
		SetCursorPosition(2, height - 2);
		Write("Crewmates Killed: " + (snake.Count-1));
		SetCursorPosition(2, height - 1);
		Write("by Kasufert");
		Random rand = new Random();
		var xPos = 0;
		var yPos = 0;
		do
		{
			xPos = rand.Next(10, width-10);
			if (xPos % 2 == 0) xPos--;
			yPos = rand.Next(10, height - 10);
		} while (snake.Any(s => s.IsLocatedAt(xPos, yPos)));
		SetCursorPosition(xPos - 1, yPos - 1);
		Write(".------.");
		SetCursorPosition(xPos - 1, yPos + 0);
		Write("| <==> |");
		SetCursorPosition(xPos - 1, yPos + 1);
		Write("|  __  |");
		SetCursorPosition(xPos - 1, yPos + 2);
		Write("| |  | |");
		SetCursorPosition(xPos - 1, yPos + 3);
		Write("--   --");

		return (xPos, yPos);
	}
	static void PropagateMovement(this List<BodyPart> snake, Direction headDir, bool grow, bool tripleGrow)
	{ 
		foreach (var segment in snake) // Updating previous positions
		{
			segment.prevX = segment.xPos;
			segment.prevY = segment.yPos;
		}
		switch (headDir) // Moving the head
		{
			case Direction.Stopped:
				return;
			case Direction.UpLeft:
				snake[0].yPos--;
				snake[0].xPos-=2;
				break;
			case Direction.UpRight:
				snake[0].yPos--;
				snake[0].xPos+=2;
				break;
			case Direction.DownLeft:
				snake[0].xPos -= 2;
				snake[0].yPos++;
				break;
			case Direction.DownRight:
				snake[0].xPos += 2;
				snake[0].yPos++;
				break;
			case Direction.Up:
				snake[0].yPos--;
				break;
			case Direction.Down:
				snake[0].yPos++;
				break;
			case Direction.Left:
				snake[0].xPos-=2;
				break;
			case Direction.Right:
				snake[0].xPos+=2;
				break;
		}
		for (int i = 1; i < snake.Count; i++)
		{
			snake[i].xPos = snake[i - 1].prevX;
			snake[i].yPos = snake[i - 1].prevY;
		}
		if (grow)
		{
			snake.Add(new(snake[snake.Count - 1].prevX, snake[snake.Count - 1].prevY));
			SetCursorPosition(11, height - 1);
			Write(snake.Count);
		}
		if (tripleGrow)
		{
			for (int j = 0; j < 2; j++)
			{
				foreach (var segment in snake) // Updating previous positions
				{
					segment.prevX = segment.xPos;
					segment.prevY = segment.yPos;
				}
				switch (headDir) // Moving the head
				{
					case Direction.Stopped:
						return;
					case Direction.UpLeft:
						snake[0].yPos--;
						snake[0].xPos -= 2;
						break;
					case Direction.UpRight:
						snake[0].yPos--;
						snake[0].xPos += 2;
						break;
					case Direction.DownLeft:
						snake[0].xPos -= 2;
						snake[0].yPos++;
						break;
					case Direction.DownRight:
						snake[0].xPos += 2;
						snake[0].yPos++;
						break;
					case Direction.Up:
						snake[0].yPos--;
						break;
					case Direction.Down:
						snake[0].yPos++;
						break;
					case Direction.Left:
						snake[0].xPos -= 2;
						break;
					case Direction.Right:
						snake[0].xPos += 2;
						break;
				}
				for (int i = 1; i < snake.Count; i++)
				{
					snake[i].xPos = snake[i - 1].prevX;
					snake[i].yPos = snake[i - 1].prevY;
				}
				snake.Add(new(snake[snake.Count - 1].prevX, snake[snake.Count - 1].prevY));
				SetCursorPosition(11, height - 1);
				Write(snake.Count);

			}
		}
	}

	static void DrawBorder()
	{
		var xPos = 0;
		var yPos = 0;
		while (xPos < width)
		{
			SetCursorPosition(xPos, yPos);
			Write("-");
			xPos++;
		}
		while (yPos < height)
		{
			SetCursorPosition(xPos, yPos);
			Write("|");
			yPos++;
		}
		while (xPos > 1)
		{
			SetCursorPosition(xPos, yPos);
			Write("-");
			xPos--;
		}
		while(yPos > 0)
		{
			SetCursorPosition(xPos, yPos);
			Write("|");
			yPos--;
		}
		SetCursorPosition(2, height - 1);
		Write("Length: ");
		SetCursorPosition(width - 12, height - 1);
		Write("By Kasufert");
	}
	static void DrawBoard(List<BodyPart> snake, Direction direction)
	{
		SetCursorPosition(snake[snake.Count - 1].prevX, snake[snake.Count - 1].prevY);
		Write("   ");
		foreach (var seg in snake)
		{
			SetCursorPosition(seg.xPos, seg.yPos); // Updates head
			Write("SUS");
		}
		SetCursorPosition(snake[0].xPos, snake[0].yPos);
		switch (direction)
		{
			case Direction.Up:
				Write("SUS");
				break;
			case Direction.Down:
				Write("SUS");
				break;
			case Direction.Left:
				Write("SUS");
				break;
			case Direction.Right:
				Write("SUS");
				break;
			default:
				break;
		}
	}

	static void Song()
	{
		while(true)
		{
			Beep(659, 125); Beep(659, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(523, 125); Beep(659, 125); Sleep(125); Beep(784, 125); Sleep(375); Beep(392, 125); Sleep(375); Beep(523, 125); Sleep(250); Beep(392, 125); Sleep(250); Beep(330, 125); Sleep(250); Beep(440, 125); Sleep(125); Beep(494, 125); Sleep(125); Beep(466, 125); Sleep(42); Beep(440, 125); Sleep(125); Beep(392, 125); Sleep(125); Beep(659, 125); Sleep(125); Beep(784, 125); Sleep(125); Beep(880, 125); Sleep(125); Beep(698, 125); Beep(784, 125); Sleep(125); Beep(659, 125); Sleep(125); Beep(523, 125); Sleep(125); Beep(587, 125); Beep(494, 125); Sleep(125); Beep(523, 125); Sleep(250); Beep(392, 125); Sleep(250); Beep(330, 125); Sleep(250); Beep(440, 125); Sleep(125); Beep(494, 125); Sleep(125); Beep(466, 125); Sleep(42); Beep(440, 125); Sleep(125); Beep(392, 125); Sleep(125); Beep(659, 125); Sleep(125); Beep(784, 125); Sleep(125); Beep(880, 125); Sleep(125); Beep(698, 125); Beep(784, 125); Sleep(125); Beep(659, 125); Sleep(125); Beep(523, 125); Sleep(125); Beep(587, 125); Beep(494, 125); Sleep(375); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(415, 125); Beep(440, 125); Beep(523, 125); Sleep(125); Beep(440, 125); Beep(523, 125); Beep(587, 125); Sleep(250); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(698, 125); Sleep(125); Beep(698, 125); Beep(698, 125); Sleep(625); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(415, 125); Beep(440, 125); Beep(523, 125); Sleep(125); Beep(440, 125); Beep(523, 125); Beep(587, 125); Sleep(250); Beep(622, 125); Sleep(250); Beep(587, 125); Sleep(250); Beep(523, 125); Sleep(1125); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(415, 125); Beep(440, 125); Beep(523, 125); Sleep(125); Beep(440, 125); Beep(523, 125); Beep(587, 125); Sleep(250); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(698, 125); Sleep(125); Beep(698, 125); Beep(698, 125); Sleep(625); Beep(784, 125); Beep(740, 125); Beep(698, 125); Sleep(42); Beep(622, 125); Sleep(125); Beep(659, 125); Sleep(167); Beep(415, 125); Beep(440, 125); Beep(523, 125); Sleep(125); Beep(440, 125); Beep(523, 125); Beep(587, 125); Sleep(250); Beep(622, 125); Sleep(250); Beep(587, 125); Sleep(250); Beep(523, 125); Sleep(1000);
		}
	}
}
class BodyPart
{
	public int xPos;
	public int yPos;
	public int prevX;
	public int prevY;
	public BodyPart(int x, int y)
	{
		this.xPos = x;
		prevX = x;
		this.yPos = y;
		prevY = y;
	}

	public bool IsLocatedAt(int x, int y) => (x == xPos) && (y == yPos);	
}

