namespace RSA.Demo
{
    class Program
    {
        public static void Main()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1 MathService");
                Console.WriteLine("2 PrimalityTest");
                Console.WriteLine("3 RSA");
                Console.WriteLine("4 Wiener");
                Console.WriteLine("0 exit");
                Console.Write("\n>> ");

                var key = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (key)
                {
                    case '1':
                        MathTheoryDemo.Run();
                        break;
                    case '2':
                        PrimalityDemo.Run();
                        break;
                    case '3':
                        RsaDemo.Run();
                        break;
                    case '4':
                        WienerAttackDemo.Run();
                        break;
                    case '0':
                        return;
                    default:
                        Console.WriteLine("???>>");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}