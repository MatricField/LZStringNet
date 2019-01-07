namespace LZStringNet.IO
{
    public interface IEncoder
    {
        void Flush();
        void WriteBits(int data, int numBits);
    }
}