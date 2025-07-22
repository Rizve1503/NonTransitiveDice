using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 1. Validate dice count
            if (args.Length < 3)
            {
                Console.WriteLine("Error: You must provide at least three dice. Example:");
                Console.WriteLine("  dotnet run -- \"2,2,4,4,9,9\" \"6,8,1,1,8,6\" \"7,5,3,7,5,3\"");
                return;
            }

            // 2. Parse each die and validate faces
            var dice = args.Select(arg =>
            {
                var faces = arg.Split(',')
                               .Select(s =>
                               {
                                   if (!int.TryParse(s, out int value))
                                       throw new FormatException($"Invalid face value '{s}' in die \"{arg}\"");
                                   return value;
                               })
                               .ToArray();

                if (faces.Length != 6)
                    throw new ArgumentException($"Each die must have exactly 6 faces. Die \"{arg}\" has {faces.Length}.");

                return faces;
            }).ToArray();

            Console.WriteLine($"Parsed {dice.Length} dice successfully.");

            int n = dice.Length;

            // 3. Precompute win probabilities
            double[,] winProb = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int wins = 0, total = 0;
                    foreach (var faceI in dice[i])
                        foreach (var faceJ in dice[j])
                        {
                            if (faceI > faceJ) wins++;
                            total++;
                        }
                    winProb[i, j] = total > 0 ? wins / (double)total : 0;
                }
            }

            // 4. Generate random first-move bit and HMAC commit
            using var rng = RandomNumberGenerator.Create();
            byte[] bitBuf = new byte[1];
            rng.GetBytes(bitBuf);
            int firstMoveBit = bitBuf[0] % 2;

            byte[] key = new byte[32];
            rng.GetBytes(key);

            using var hmac = new HMACSHA256(key);
            byte[] hmacBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(firstMoveBit.ToString()));
            string hmacHex = BitConverter.ToString(hmacBytes).Replace("-", "");
            Console.WriteLine($"HMAC commit: {hmacHex}");

            // Local functions for menu and help
            void ShowMenu()
            {
                Console.WriteLine("\nAvailable dice:");
                for (int i = 0; i < dice.Length; i++)
                    Console.WriteLine($"  {i + 1}. Die #{i + 1} [{string.Join(",", dice[i])}]");
                Console.WriteLine("  H. Help");
                Console.WriteLine("  X. Exit");
            }

            void ShowHelp()
            {
                Console.WriteLine("\nHelp: Select a die by its number, H for help, or X to exit.");
                Console.WriteLine("Win-probability table P(i beats j):");

                // Header
                Console.Write("     ");
                for (int j = 0; j < n; j++)
                    Console.Write($"Die{j + 1,6} ");
                Console.WriteLine();

                for (int i = 0; i < n; i++)
                {
                    Console.Write($"Die{i + 1,3} ");
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j)
                            Console.Write("   —    ");
                        else
                            Console.Write($"{winProb[i, j]:P1}, ".PadLeft(8));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            // 5. User and computer pick dice based on first-move bit
            int userChoice = -1, compChoice = -1;
            if (firstMoveBit == 0)
            {
                Console.WriteLine("Your turn to choose a die first.");
                while (userChoice < 1 || userChoice > dice.Length)
                {
                    ShowMenu();
                    Console.Write("Enter choice: ");
                    var input = Console.ReadLine()?.Trim().ToUpper();
                    if (input == "H") { ShowHelp(); continue; }
                    if (input == "X") return;
                    if (!int.TryParse(input, out userChoice))
                    {
                        Console.WriteLine("Invalid input. Try again.");
                        continue;
                    }
                }
                compChoice = RandomNumberGenerator.GetInt32(0, dice.Length) + 1;
                Console.WriteLine($"Computer selects Die #{compChoice}.");
            }
            else
            {
                Console.WriteLine("Computer chooses its die first.");
                compChoice = RandomNumberGenerator.GetInt32(0, dice.Length) + 1;
                Console.WriteLine($"Computer selects Die #{compChoice}.");

                while (userChoice < 1 || userChoice > dice.Length)
                {
                    ShowMenu();
                    Console.Write("Enter choice: ");
                    var input = Console.ReadLine()?.Trim().ToUpper();
                    if (input == "H") { ShowHelp(); continue; }
                    if (input == "X") return;
                    if (!int.TryParse(input, out userChoice))
                    {
                        Console.WriteLine("Invalid input. Try again.");
                        continue;
                    }
                }
            }

            // 6. Reveal HMAC key & bit
            string keyHex = BitConverter.ToString(key).Replace("-", "");
            Console.WriteLine($"\nHMAC key reveal: {keyHex}");
            Console.WriteLine($"First-move bit: {firstMoveBit}\n");

            // 7. Roll dice and declare the winner
            int RollDie(int[] faces) => faces[RandomNumberGenerator.GetInt32(0, faces.Length)];
            int userRoll = RollDie(dice[userChoice - 1]);
            int compRoll = RollDie(dice[compChoice - 1]);

            Console.WriteLine($"You rolled: {userRoll}");
            Console.WriteLine($"Computer rolled: {compRoll}");

            if (userRoll > compRoll)
                Console.WriteLine("🎉 You win!");
            else if (userRoll < compRoll)
                Console.WriteLine("😞 Computer wins.");
            else
                Console.WriteLine("🤝 It’s a tie.");
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
