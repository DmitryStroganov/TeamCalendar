namespace TeamCalendar.Data
{
    public interface ICryptoProvider
    {
        byte[] Encode(byte[] block, byte[] key);
        byte[] Decode(byte[] block, byte[] key);
        byte[] GetPublicKey();
    }
}