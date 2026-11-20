using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Media;

public sealed record PngLogoData(byte[] Content, int Width, int Height, string Hash);

/// <summary>
/// PNG estático: estrutura/CRC, dimensões, fluxo zlib e linhas limitados; remove metadados opcionais.
/// </summary>
public static class PngLogoValidator
{
    public const int MaximumBytes = 1024 * 1024;
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly uint[] CrcTable = CreateCrcTable();

    public static PngLogoData Validate(byte[] bytes)
    {
        if (bytes.Length > MaximumBytes)
            throw new SecurityFailure(413, "Media.TooLarge", "O logo deve ter no máximo 1 MiB.");

        if (bytes.Length < 57 || !bytes.AsSpan(0, 8).SequenceEqual(Signature))
            throw Invalid();

        using var output = new MemoryStream();
        using var compressed = new MemoryStream();
        output.Write(Signature);
        var offset = 8;
        var width = 0;
        var height = 0;
        var depth = 0;
        var color = 0;
        var palette = 0;
        var transparency = false;
        var dataStarted = false;
        var dataClosed = false;
        var ended = false;

        while (offset < bytes.Length)
        {
            if (bytes.Length - offset < 12)
                throw Invalid();

            var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));

            if (size > bytes.Length - offset - 12)
                throw Invalid();

            var length = (int)size;
            var type = Encoding.ASCII.GetString(bytes, offset + 4, 4);
            var payload = bytes.AsSpan(offset + 8, length);
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset + 8 + length, 4));

            if (Crc(bytes.AsSpan(offset + 4, length + 4)) != expectedCrc ||
                type.Any(Character => !char.IsAsciiLetter(Character)) || type[2] is >= 'a' and <= 'z')
                throw Invalid();

            var keep = false;

            if (type == "IHDR")
            {
                if (offset != 8 || length != 13)
                    throw Invalid();

                var rawWidth = BinaryPrimitives.ReadUInt32BigEndian(payload[..4]);
                var rawHeight = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));

                if (rawWidth is < 1 or > 2048 || rawHeight is < 1 or > 2048 ||
                    (long)rawWidth * rawHeight > 1048576)
                    throw new SecurityFailure(400, "Media.DimensionsExceeded", "Use até 2048 pixels por lado e 1 megapixel no total.");

                width = (int)rawWidth;
                height = (int)rawHeight;
                depth = payload[8];
                color = payload[9];
                var validDepth = color switch
                {
                    0 => depth is 1 or 2 or 4 or 8 or 16,
                    3 => depth is 1 or 2 or 4 or 8,
                    2 or 4 or 6 => depth is 8 or 16,
                    _ => false
                };

                if (!validDepth || payload[10] != 0 || payload[11] != 0 || payload[12] != 0)
                    throw new SecurityFailure(400, "Media.UnsupportedPng", "Envie um PNG estático sem entrelaçamento.");

                keep = true;
            }
            else if (width == 0)
                throw Invalid();
            else if (type == "PLTE")
            {
                if (palette != 0 || dataStarted || color is 0 or 4 || length is < 3 or > 768 || length % 3 != 0 ||
                    color == 3 && length / 3 > 1 << depth)
                    throw Invalid();

                palette = length / 3;
                keep = true;
            }
            else if (type == "tRNS")
            {
                if (transparency || dataStarted || color == 0 && length != 2 || color == 2 && length != 6 ||
                    color == 3 && (palette == 0 || length < 1 || length > palette) || color is 4 or 6)
                    throw Invalid();

                transparency = true;
                keep = true;
            }
            else if (type == "IDAT")
            {
                if (dataClosed || color == 3 && palette == 0)
                    throw Invalid();

                dataStarted = true;
                compressed.Write(payload);
                keep = true;
            }
            else if (type == "IEND")
            {
                if (length != 0 || !dataStarted || offset + 12 != bytes.Length)
                    throw Invalid();

                ended = true;
                keep = true;
            }
            else if (type is "acTL" or "fcTL" or "fdAT" || (bytes[offset + 4] & 32) == 0)
                throw Invalid();

            if (dataStarted && type != "IDAT")
                dataClosed = true;

            if (keep)
                output.Write(bytes, offset, length + 12);

            offset += length + 12;
        }

        if (!ended)
            throw Invalid();

        var channels = color switch { 2 => 3, 4 => 2, 6 => 4, _ => 1 };
        var rowBytes = (width * channels * depth + 7) / 8 + 1;
        var row = new byte[rowBytes];
        compressed.Position = 0;

        try
        {
            using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);

            for (var index = 0; index < height; index++)
            {
                inflater.ReadExactly(row);

                if (row[0] > 4)
                    throw Invalid();
            }

            if (inflater.ReadByte() != -1)
                throw Invalid();
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
            throw Invalid();
        }

        var content = output.ToArray();

        return new PngLogoData(content, width, height, Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant());
    }

    private static SecurityFailure Invalid()
    {
        return new SecurityFailure(400, "Media.InvalidPng", "O arquivo não é um PNG estático válido.");
    }

    private static uint Crc(ReadOnlySpan<byte> bytes)
    {
        var value = uint.MaxValue;

        foreach (var item in bytes)
            value = CrcTable[(value ^ item) & 255] ^ value >> 8;

        return ~value;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];

        for (var index = 0; index < table.Length; index++)
        {
            var value = (uint)index;

            for (var bit = 0; bit < 8; bit++)
                value = (value & 1) != 0 ? 0xedb88320U ^ value >> 1 : value >> 1;

            table[index] = value;
        }

        return table;
    }
}
