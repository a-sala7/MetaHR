namespace MetaHR_API.Utility
{
    public static class FileSignatureVerifier
    {
        private static byte[][] _jpegSignatures = new byte[][]
        {
            new byte[]{ 0xFF, 0xD8, 0xFF, 0xE0 },
            new byte[]{ 0xFF, 0xD8, 0xFF, 0xE1 },
            new byte[]{ 0xFF, 0xD8, 0xFF, 0xE2 },
            new byte[]{ 0xFF, 0xD8, 0xFF, 0xE3 },
            new byte[]{ 0xFF, 0xD8, 0xFF, 0xE8 }
        };

        private static byte[][] _pngSignatures = new byte[][]
        {
            new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
        };

        private static byte[][] _pdfSignatures = new byte[][]
        {
            new byte[]{ 0x25, 0x50, 0x44, 0x46 }
        };

        private static bool IsValid(byte[][] signatures, Stream stream, long contentLength)
        {
            if (contentLength < signatures.Min(s => s.Length))
            {
                return false;
            }
            stream.Position = 0;
            var reader = new BinaryReader(stream);
            var headerBytes = reader.ReadBytes(signatures.Max(s => s.Length));
            if (signatures.Any(s => s.SequenceEqual(headerBytes.Take(s.Length))))
            {
                stream.Position = 0;
                return true;
            }
            stream.Position = 0;
            return false;
        }

        public static bool IsJpeg(Stream stream, long contentLength)
        {
            return IsValid(_jpegSignatures, stream, contentLength);
        }

        public static bool IsPng(Stream stream, long contentLength)
        {
            return IsValid(_pngSignatures, stream, contentLength);
        }

        public static bool IsPdf(Stream stream, long contentLength)
        {
            return IsValid(_pdfSignatures, stream, contentLength);
        }
    }
}
