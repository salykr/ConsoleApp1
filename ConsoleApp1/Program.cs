using System;
using OtpNet;                      // For TOTP key generation and validation
using ZXing;                      // QR code generation
using ZXing.Common;
using ZXing.Rendering;
using SixLabors.ImageSharp;       // Image processing
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;         // For opening the image file

namespace TwoFactorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Two-Factor Authentication Setup");

        //generated key: class OTP
        byte[] secretBytes = KeyGeneration.GenerateRandomKey(20);
        //encode the secret to the format of TOTP secret

        //because the secret needs to be shared with the user or an authenticator app
        //and raw bytes (byte[]) can't be easily displayed, copied, or scanned.
        String base32Secret = Base32Encoding.ToString(secretBytes);
        //Console.WriteLine($"Secret key (save this securely): {base32Secret}");

                    // otpauth://totp/{issuer}:{account}?secret={key}&issuer={issuer}
        String uri = $"otpauth://totp/MyApp:user@example.com?secret={base32Secret}&issuer=MyApp";

        Console.WriteLine("\nGenerating QR code image...");
        SaveQrCodeToImage(uri, "qrcode.png");
        //Console.WriteLine("QR code saved and opened.");

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
        //creating QR code
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

        //pixelData.Pixels: byte[] containing raw RGBA values( for each pixel).
        //Converts the input string (content) into raw pixel data representing the QR code
        // QR code as raw pixel data
        PixelData pixelData = writer.Write(content);

        //interpret each set of 4 bytes as an RGBA pixel, and build an image of size
        //Create an image from raw pixels
        using Image image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

        image.Save(outputPath, new PngEncoder());

        //open the image automatically
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

