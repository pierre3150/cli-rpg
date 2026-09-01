using RpgGame.Core.Services;

namespace RpgGame.Core.Tests;

/// <summary>Always returns a fixed value, making crit rolls and other randomness
/// fully deterministic and repeatable in tests.</summary>
public class FakeRandomProvider : IRandomProvider
{
    private readonly int _fixedValue;
    public FakeRandomProvider(int fixedValue) => _fixedValue = fixedValue;
    public int Next(int minInclusive, int maxExclusive) => _fixedValue;
}
