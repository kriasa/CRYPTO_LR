using System;
using System.IO;
using System.Threading.Tasks;
using Rijndael.Core.Modes;
using Rijndael.Core.Padding;

namespace Rijndael.Demo
{
    public static class Program
    {
        public static async Task Main()
        {
            string baseDirectory = AppContext.BaseDirectory;

            string testDataDirectory = Path.Combine(baseDirectory, "TestData");
            string outputDirectory = Path.Combine(baseDirectory, "Output");

            Directory.CreateDirectory(outputDirectory);

            var demo = new RijndaelDemo(testDataDirectory, outputDirectory);

            while (true)
            {
                int action = ConsoleMenu.ChooseAction();
                if (action == 0) return;

                CipherModeType mode = ConsoleMenu.ChooseMode();
                PaddingModeType padding = ConsoleMenu.ChoosePadding();
                try
                {
                    if (action == 1)
                    {
                        demo.RunArrayDemo(mode, padding);
                    }
                    else
                    {
                        string fileName = ConsoleMenu.AskFileName();
                        await demo.RunFileDemoAsync(mode, padding, fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}