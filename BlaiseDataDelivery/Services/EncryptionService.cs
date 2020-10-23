using System;
using System.IO;
using BlaiseDataDelivery.Helpers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace BlaiseDataDelivery.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfigurationProvider _configuration;

        public EncryptionService(IConfigurationProvider configuration)
        {
            _configuration = configuration;
        }

        public void EncryptFile(string inputFilePath, string outputFilePath)
        {
            inputFilePath.ThrowExceptionIfNullOrEmpty("inputFilePath");
            outputFilePath.ThrowExceptionIfNullOrEmpty("outputFilePath");

            var publicKeyPath = Path.GetFullPath(_configuration.EncryptionKey);
            var publicKey = ReadPublicKey(publicKeyPath);

            EncryptFile(inputFilePath, outputFilePath, publicKey);
        }

        private static void EncryptFile(string inputFilePath, string outputFilePath, PgpPublicKey publicKey)
        {
            outputFilePath.ThrowExceptionIfNullOrEmpty("outputFilePath");

            // ReSharper disable once AssignNullToNotNullAttribute
            //create any folders that may not exist
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (var fileStream = new FileStream(outputFilePath, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                EncryptFile(streamWriter.BaseStream, inputFilePath, publicKey);
            }
        }

        private static void EncryptFile(Stream outputStream, string filePath, PgpPublicKey publicKey)
        {
            using (var outputMemoryStream = new MemoryStream())
            {
                var compressedData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                PgpUtilities.WriteFileToLiteralData(compressedData.Open(outputMemoryStream), PgpLiteralData.Binary, new FileInfo(filePath));
                compressedData.Close();

                var bytes = outputMemoryStream.ToArray();

                var encryptedData = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, true, new SecureRandom());
                encryptedData.AddMethod(publicKey);

                var encryptedStream = encryptedData.Open(outputStream, bytes.Length);
                encryptedStream.Write(bytes, 0, bytes.Length);
                encryptedStream.Close();
            }
        }

        private static PgpPublicKey ReadPublicKey(string publicKeyFilePath)
        {
            Stream keyFileStream = File.OpenRead(publicKeyFilePath);
            keyFileStream = PgpUtilities.GetDecoderStream(keyFileStream);

            var pgpPub = new PgpPublicKeyRingBundle(keyFileStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        keyFileStream.Close();
                        return key;
                    }
                }

            throw new ArgumentException("Can't find encryption key in key ring of the public key held at '{publicKeyFilePath}'");
        }
    }
}
