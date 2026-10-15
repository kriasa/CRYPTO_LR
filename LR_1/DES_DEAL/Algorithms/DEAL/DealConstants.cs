using System;

namespace DES_DEAL.Algorithms.DEAL
{
    public static class DealConstants
    {
        public const int DesBlockSizeBytes = 8;
        public const int BlockSizeBytes = 16;

        public static readonly byte[] ScheduleConstantKey =
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF
        };

        public static int GetKeyPartsCount(int keyLengthBytes) => keyLengthBytes switch
        {
            16 => 2,
            24 => 3,
            32 => 4,
            _ => throw new ArgumentException("error: len = 16, 24 32",nameof(keyLengthBytes))
        };

        public static int GetRoundCount(int keyLengthBytes) => keyLengthBytes switch
        {
            16 => 6,
            24 => 6,
            32 => 8,
            _ => throw new ArgumentException("error: len = 16, 24 32", nameof(keyLengthBytes))
        };
    }
}