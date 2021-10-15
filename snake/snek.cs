using static System.Console;
using System.Linq;
namespace Game;
enum Direction
{
	Up, Down, Left, Right
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
		while(true)
		{
			direction = Direction.Up;
			length = 1;
			Clear();
			List<BodyPart> snake = new List<BodyPart>();
			snake.Add(new BodyPart(width/2, height/2));
			CursorVisible = false;
			DrawBorder();
			(foodX, foodY) = PlaceFood(snake);
			SetCursorPosition(11, height - 1);
			Write(snake.Count);
			while (true)
			{
				DrawBoard(snake);
				Thread.Sleep(30);
				if (KeyAvailable) GetDirection();
				if (snake[0].IsLocatedAt(foodX, foodY) || snake[0].IsLocatedAt(foodX+1, foodY))
				{
					length++;
					snake.PropagateMovement(direction, true);
					SetCursorPosition(foodX, foodY);
					(foodX, foodY) = PlaceFood(snake);
				}
				else
				{
					snake.PropagateMovement(direction, false);
				}
				if (snake.Where(s => s.IsLocatedAt(snake[0].xPos, snake[0].yPos)).Count() > 1) break;
				if (snake[0].xPos is (0 or 1 or width or width-1)) break;
				if (snake[0].yPos is (0 or height)) break;
			}

			WriteLine("You Died\nGet Fucked!");
			WriteLine("Your Length: " + length);
			ReadLine();

		}
		void GetDirection()
		{
			switch (ReadKey().Key)
			{
				case ConsoleKey.UpArrow:
					if (direction != Direction.Down)
						direction = Direction.Up;
					break;
				case ConsoleKey.DownArrow:
					if (direction != Direction.Up)
						direction = Direction.Down;
					break;
				case ConsoleKey.LeftArrow:
					if (direction != Direction.Right)
						direction = Direction.Left;
					break;
				case ConsoleKey.RightArrow:
					if (direction != Direction.Left)
						direction = Direction.Right;
					break;
			}
		}
	}
	static (int,int) PlaceFood(List<BodyPart> snake)
	{
		Random rand = new Random();
		var xPos = 0;
		var yPos = 0;
		do
		{
			xPos = rand.Next(2, width-2);
			if (xPos % 2 == 0) xPos--;
			yPos = rand.Next(1, height - 1);
		} while (snake.Any(s => s.IsLocatedAt(xPos, yPos)));
		SetCursorPosition(xPos+1, yPos);
		Write("()");
		return (xPos, yPos);
	}
	static void PropagateMovement(this List<BodyPart> snake, Direction headDir, bool grow)
	{
		foreach (var segment in snake) // Updating previous positions
		{
			segment.prevX = segment.xPos;
			segment.prevY = segment.yPos;
		}
		switch (headDir) // Moving the head
		{
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
	static void DrawBoard(List<BodyPart> snake)
	{
		SetCursorPosition(snake[snake.Count - 1].prevX, snake[snake.Count - 1].prevY); // Updates tail
		Write("  ");

		SetCursorPosition(snake[0].xPos, snake[0].yPos); // Updates head
		Write("[]");
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