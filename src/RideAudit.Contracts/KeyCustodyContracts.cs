namespace RideAudit.Contracts;

public sealed class CollectionKeyMaterial : IDisposable
{
    public required string KeyId { get; init; }
    public required byte[] PublicKeySpki { get; init; }
    public required byte[] Dek { get; init; }
    public required byte[] PrivateKeyPkcs8 { get; init; }
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;
        CryptographicOperationsZero(Dek);
        CryptographicOperationsZero(PrivateKeyPkcs8);
        _disposed = true;
    }

    private static void CryptographicOperationsZero(byte[] buffer)
    {
        if (buffer.Length > 0)
            System.Security.Cryptography.CryptographicOperations.ZeroMemory(buffer);
    }
}

public interface ICollectionKeySource
{
    int GenerateCount { get; }
    CollectionKeyMaterial Create(string keyScope, string scopeBinding);
}

public sealed class EscrowSecret
{
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }
    public required string SealedRecordId { get; init; }
    public required byte[] PrivateKeyPkcs8 { get; init; }
    public required byte[] Dek { get; init; }
    public required IReadOnlyList<string> CustodianIds { get; init; }
    public int ThresholdM { get; init; } = 2;
    public int TotalN { get; init; } = 3;
}

public interface IEscrowSink
{
    void EscrowCollectionSecret(EscrowSecret secret);
}

public interface IEscrowDirectory
{
    bool IsEscrowed(string keyId, string tenantId);
}
