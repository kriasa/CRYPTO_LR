namespace DES_DEAL.Engine.Padding
{
    public sealed class PkcsPadding : PaddingStrategyBase
    {
        protected override void FillPaddingBytes(byte[] data)
        {
            byte val = (byte)data.Length;
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = val;
            }
        }
        protected override void ValidatePaddingBytes(byte[] data, int paddingStart, int paddingLen)
        {
            byte expected = (byte)paddingLen;
            for (int i = paddingStart; i < paddingStart + paddingLen - 1; ++i)
            {
                if (data[i] != expected) throw new InvalidPaddingException("Padding PKCS incorrect");
            }
        }
    }
}