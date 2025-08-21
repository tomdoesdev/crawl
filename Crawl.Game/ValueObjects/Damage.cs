namespace Crawl.Game.ValueObjects;

public enum ApplicationType : byte
{
    Flat,
    CurrentHealthPercent,
    MaxHealthPercent
}

public enum DamageType : byte
{
    Physical,
    Poison,
    Fire
}

public readonly record struct Damage(float Amount, ApplicationType ApplicationType, DamageType Type)
{
}