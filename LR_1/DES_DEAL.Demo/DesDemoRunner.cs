using System.Security.Cryptography;
using System.Threading.Tasks;
using DES_DEAL.Algorithms.DES;

namespace DES_DEAL.Demo
{
    public sealed class DesDemoRunner : DemoRunner<DesCipher>
    {
        public DesDemoRunner(string testDataDirectory, string outputDirectory): base("DES", testDataDirectory, outputDirectory)
        {
        }
        public override async Task RunAsync()
        {
            Console.WriteLine("DES");

            byte[] key = RandomNumberGenerator.GetBytes(BlockSizeBytes);

            await DemoRandomBytesAsync(key);
            // await DemoPaddingModesAsync(key);
            // await DemoFilesAsync(key);

            PrintSummary();
        }
    }
}