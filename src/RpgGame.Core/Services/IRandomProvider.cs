namespace RpgGame.Core.Services;

/// <summary>Abstraction over randomness so combat outcomes are deterministic in tests.</summary>
public interface IRandomProvider
{
    int Next(int minInclusive, int maxExclusive);
}

public class SystemRandomProvider : IRandomProvider
{
    private readonly Random _random = new();
    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
}
