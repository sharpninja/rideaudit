using System.Security.Cryptography;
using System.Text;
using RideAudit.Contracts;
using RideAudit.Seal;

namespace RideAudit.Seal.Tests;

public class AuthorizedDecryptorRaesTests
{
    [Fact]
    public void Open_decrypts_raes_and_still_opens_rideseal1()
    {
        var dek = RandomNumberGenerator.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes("raes-decrypt-unit");
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(dek, 16))
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var envelope = new byte[RaesEnvelopeFormat.PrefixLength + ciphertext.Length];
        RaesEnvelopeFormat.Magic.CopyTo(envelope);
        envelope[4] = RaesEnvelopeFormat.Version;
        nonce.CopyTo(envelope.AsSpan(5));
        tag.CopyTo(envelope.AsSpan(17));
        ciphertext.CopyTo(envelope.AsSpan(RaesEnvelopeFormat.PrefixLength));

        var opened = AuthorizedDecryptor.Open(envelope, dek);
        Assert.Equal(plaintext, opened);

        var unknown = Assert.Throws<RideAuditException>(() => AuthorizedDecryptor.Open(new byte[] { 1, 2, 3, 4, 5 }, dek));
        Assert.Equal(ErrorCodes.PlaintextRejected, unknown.Code);
    }
}
