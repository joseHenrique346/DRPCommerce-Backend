using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Media;

public sealed record PngLogoData(byte[] Content, int Width, int Height, string Hash);
