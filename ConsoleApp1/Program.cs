using System;
using OtpNet;                      
using ZXing;                      
using ZXing.Common;
using ZXing.Rendering;
using SixLabors.ImageSharp;      
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;        

namespace TwoFactorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Two-Factor Authentication Setup");

        byte[] secretBytes = KeyGeneration.GenerateRandomKey(20);

        String base32Secret = Base32Encoding.ToString(secretBytes);

        String uri = $"otpauth://totp/MyApp:user@example.com?secret={base32Secret}&issuer=MyApp";

        Console.WriteLine("\nGenerating QR code image...");
        SaveQrCodeToImage(uri, "qrcode.png");

        Totp totp = new Totp(secretBytes);
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            Console.Write($"\nEnter 6-digit code (attempt {attempt}/3): ");
            String input = Console.ReadLine();
            if (input != null && totp.VerifyTotp(input, out _))
            {
                Console.WriteLine("Verified successfully.");
                return;
            }
            Console.WriteLine("Incorrect code.");
        }
        Console.WriteLine("Verification failed.");
    }

    static void SaveQrCodeToImage(string content, string outputPath)
    {
        BarcodeWriterPixelData writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Height = 250,
                Width = 250,
                Margin = 1
            }
        };
        PixelData pixelData = writer.Write(content);

        using Image image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

        image.Save(outputPath, new PngEncoder());
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open QR code image: {ex.Message}");
        }
    }
}

