namespace LZStringNet.IO
{
    public static class PredefinedEncodings
    {
        public static DataEncoding Base64Encoding { get; }
            = new DataEncoding("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=", 6);
    }
}
