using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // 1. Check dice count
        if (args.Length < 3)
        {
            Console.WriteLine("Error: You must provide at least three dice. Example:");
            Console.WriteLine("  NonTransitiveDice \"2,2,4,4,9,9\" \"6,8,1,1,8,6\" \"7,5,3,7,5,3\"");
            return;
        }

        // 2. Parse each die
        var dice = args.Select(arg =>
        {
            var faces = arg.Split(',')
                           .Select(s =>
                           {
                               if (!int.TryParse(s, out int v))
                                   throw new FormatException($"Invalid face value '{s}' in die \"{arg}\"");
                               return v;
                           })
                           .ToArray();
            return faces;
        }).ToArray();

        Console.WriteLine($"Parsed {dice.Length} dice successfully.");

        // 3. Generate random first‑move bit
        var rng = RandomNumberGenerator.Create();
        byte[] bitBuf = new byte[1];
        rng.GetBytes(bitBuf);
        int firstMoveBit = bitBuf[0] % 2;  // 0 or 1

        // 4. Generate HMAC key
        byte[] key = new byte[32];
        rng.GetBytes(key);

        // 5. Compute HMAC‑SHA256 over the bit (as string)
        using var hmac = new HMACSHA256(key);
        byte[] hmacBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(firstMoveBit.ToString()));
        string hmacHex = BitConverter.ToString(hmacBytes).Replace("-", "");

        Console.WriteLine($"HMAC commit: {hmacHex}");

        // 6. Build menu text
        void ShowMenu()
        {
            Console.WriteLine("Available dice:");
            for (int i = 0; i < dice.Length; i++)
                Console.WriteLine($"  {i + 1}. Die #{i + 1} [{string.Join(",", dice[i])}]");
            Console.WriteLine("  H. Help");
            Console.WriteLine("  X. Exit");
        }

        // 7. Handle Help & Exit
        void ShowHelp()
        {
            Console.WriteLine("Select a die by entering its number (e.g., 1).");
            Console.WriteLine("H → display this help.");
            Console.WriteLine("X → quit the game.");
        }

        // 8. Let picks happen
        int userChoice = -1, compChoice = -1;

        // If bit == 0, user picks first:
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

            // Computer picks randomly
            compChoice = RandomNumberGenerator
                            .GetInt32(0, dice.Length) + 1;
            Console.WriteLine($"Computer selects Die #{compChoice}.");
        }
        else
        {
            Console.WriteLine("Computer chooses its die first.");
            compChoice = RandomNumberGenerator
                            .GetInt32(0, dice.Length) + 1;
            Console.WriteLine($"Computer selects Die #{compChoice}.");

            // Then user picks
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
        // 9. Reveal HMAC key & bit
        string keyHex = BitConverter.ToString(key).Replace("-", "");
        Console.WriteLine($"\nHMAC key reveal: {keyHex}");
        Console.WriteLine($"First‑move bit: {firstMoveBit}\n");

        // 10. Roll both dice once
        int RollDie(int[] faces) =>
            faces[RandomNumberGenerator.GetInt32(0, faces.Length)];

        int userRoll = RollDie(dice[userChoice - 1]);
        int compRoll = RollDie(dice[compChoice - 1]);

        Console.WriteLine($"You rolled: {userRoll}");
        Console.WriteLine($"Computer rolled: {compRoll}");

        // 11. Declare result
        if (userRoll > compRoll)
            Console.WriteLine("🎉 You win!");
        else if (userRoll < compRoll)
            Console.WriteLine("😞 Computer wins.");
        else
            Console.WriteLine("🤝 It’s a tie.");
    }
}
