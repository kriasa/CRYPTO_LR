├── 📁 Core                               // Ядро. То, от чего зависят все остальные.
│   ├── 📁 Enums                          // Перечисления (Требования 1 и 2.4)
│   │   ├── BitOrder.cs                   // LSbToMSb, MSbToLSb (От младшего к старшему и наоборот)
│   │   ├── BitIndexBase.cs               // ZeroBased, OneBased (Начальный бит 0 или 1)
│   │   ├── CipherModeType.cs             // ECB, CBC, PCBC, CFB, OFB, CTR, RandomDelta
│   │   └── PaddingModeType.cs            // Zeros, AnsiX923, Pkcs7, Iso10126
│   │
│   ├── 📁 Interfaces                     // Абстракции (Требования 2.1, 2.2, 2.3)
│   │   ├── IKeyExpansion.cs              // 2.1: byte[][] ExpandKey(byte[] key)
│   │   ├── IRoundFunction.cs             // 2.2: byte[] Execute(byte[] block, byte[] roundKey)
│   │   └── ISymmetricCipher.cs           // 2.3: EncryptBlock, DecryptBlock, SetKey
│   │
│   └── 📁 Utilities                      // Утилиты
│       └── BitPermutator.cs              // Задание 1: Функция перестановки битов с учетом Enums
│
├── 📁 Engine                             // Движок: режимы, набивки, сети
│   ├── 📁 Padding                        // Паттерн "Стратегия" для набивки (Требование 2.4)
│   │   ├── IPaddingStrategy.cs           // Интерфейс набивки
│   │   ├── ZerosPadding.cs
│   │   ├── AnsiX923Padding.cs
│   │   ├── Pkcs7Padding.cs
│   │   └── Iso10126Padding.cs
│   │
│   ├── 📁 CipherModes                    // Паттерн "Стратегия" для режимов шифрования (Требование 2.4)
│   │   ├── ICipherModeStrategy.cs        // Интерфейс: Encrypt(data, cipher, iv), Decrypt(...)
│   │   ├── EcbMode.cs                    // Внутри распараллелить блоки через Parallel.For!
│   │   ├── CbcMode.cs                    
│   │   ├── PcbcMode.cs                   
│   │   ├── CfbMode.cs                    
│   │   ├── OfbMode.cs                    
│   │   ├── CtrMode.cs                    // Внутри распараллелить блоки через Parallel.For!
│   │   └── RandomDeltaMode.cs            
│   │
│   ├── FeistelNetwork.cs                 // Задание 3: Реализует ISymmetricCipher, принимает IKeyExpansion и IRoundFunction
│   ├── StrategyFactory.cs                // Фабрика: превращает CipherModeType(Enum) в ICipherModeStrategy(Класс)
│   └── SymmetricCryptoContext.cs         // Задание 2.4: Тот самый главный класс. (Async/Await методы работы с файлами)
│
├── 📁 Algorithms                         // Конкретные алгоритмы (Связывают Core и Engine)
│   ├── 📁 DES                            // Задание 4
│   │   ├── DesConstants.cs               // S-блоки, P-блоки, сдвиги
│   │   ├── DesKeyExpansion.cs            // Реализация IKeyExpansion (с использованием BitPermutator)
│   │   ├── DesRoundFunction.cs           // Реализация IRoundFunction (с использованием BitPermutator)
│   │   └── DesCipher.cs                  // Собирает DES: передает свои классы в FeistelNetwork
│   │
│   └── 📁 DEAL                           // Задание 6
│       ├── DealConstants.cs              // Константы DEAL
│       ├── DealKeyExpansion.cs           // Реализация IKeyExpansion для DEAL
│       ├── DesToRoundFunctionAdapter.cs  // ПАТТЕРН АДАПТЕР: Оборачивает DesCipher в интерфейс IRoundFunction
│       └── DealCipher.cs                 // Собирает DEAL: передает адаптер и ключи в FeistelNetwork
│
└── 📁 Demo                               // Демонстрация (Задания 5 и 7)
    ├── 📁 TestData                       // Папка, куда ты положишь картинки, mp3, txt, исходники .cs
    ├── DemoRunner.cs                     // Класс с красивым выводом в консоль (запуск шифрования)
    └── Program.cs                        // Точка входа (Main)