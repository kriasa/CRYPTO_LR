namespace Rijndael.Algorithms.Substitution
{
    public interface ISBoxProvider
    {
        byte Sub(byte value);
        byte InvSub(byte value);
    }
}