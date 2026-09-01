using RpgGame.Core.Models;

namespace RpgGame;

internal static class ConsoleUi
{
    public static void Header(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', title.Length + 4));
        Console.WriteLine($"= {title} =");
        Console.WriteLine(new string('=', title.Length + 4));
    }

    public static int Menu(string title, IReadOnlyList<string> options)
    {
        Header(title);
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  {i + 1}. {options[i]}");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out var choice) && choice >= 1 && choice <= options.Count)
                return choice - 1;

            Console.WriteLine("Choix invalide.");
        }
    }

    public static void ShowStatus(Player player)
    {
        Console.WriteLine($"{player.Name} [{player.Class}] Nv.{player.Level}  " +
            $"PV {player.Hp}/{player.MaxHp}  MP {player.Mp}/{player.MaxMp}  Or: {player.Gold}");
    }

    public static void Log(string message) => Console.WriteLine($"  > {message}");

    public static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("(Entree pour continuer)");
        Console.ReadLine();
    }
}
