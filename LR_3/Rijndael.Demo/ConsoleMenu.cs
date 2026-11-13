using System;
using Rijndael.Core.Modes;
using Rijndael.Core.Padding;

namespace Rijndael.Demo
{
    internal static class ConsoleMenu
    {
        private static readonly (string Name, CipherModeType Value)[] Modes =
        {
            ("ECB", CipherModeType.ECB),
            ("CBC", CipherModeType.CBC),
            ("PCBC", CipherModeType.PCBC),
            ("CFB", CipherModeType.CFB),
            ("OFB", CipherModeType.OFB),
            ("CTR", CipherModeType.CTR),
            ("RD", CipherModeType.RandomDelta)
        };

        private static readonly (string Name, PaddingModeType Value)[] Paddings =
        {
            ("Zeros", PaddingModeType.Zeros),
            ("ANSI", PaddingModeType.AnsiX),
            ("PKCS", PaddingModeType.Pkcs),
            ("ISO", PaddingModeType.Iso)
        };

        public static int ChooseAction()
        {
            Console.WriteLine("1 encrypt/decrypt byte arrays");
            Console.WriteLine("2 encrypt/decrypt file");
            Console.WriteLine("0 exit");

            return ReadChoice(0, 2);
        }

        public static CipherModeType ChooseMode()
        {
            Console.WriteLine();

            for (int i = 0; i < Modes.Length; i++)
            {
                Console.WriteLine($"{i + 1} {Modes[i].Name}");
            }
            return Modes[ReadChoice(1, Modes.Length) - 1].Value;
        }

        public static PaddingModeType ChoosePadding()
        {
            Console.WriteLine();

            for (int i = 0; i < Paddings.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {Paddings[i].Name}");
            }
            return Paddings[ReadChoice(1, Paddings.Length) - 1].Value;
        }

        public static string AskFileName()
        {
            while (true)
            {
                Console.Write(">>");
                string name = (Console.ReadLine() ?? "").Trim().Trim('"');
                if (name.Length > 0) return name;
            }
        }
        private static int ReadChoice(int min, int max)
        {
            while (true)
            {
                Console.Write(">> ");
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max) return value;
                Console.WriteLine("invalid choice");
            }
        }
    }
}