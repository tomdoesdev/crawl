using Crawl.Game.ValueObjects;

namespace Crawl.Game.Components;

public struct HealthComponent
{
    public float DamageTaken { get; private set; }
    public int MaxHealth { get; private set; }

    public int CurrentHealth =>
        MaxHealth - (int)DamageTaken; //Ignore the fraction of DamageTaken so CurrentHealth is always an int.

    public void ApplyDamage(Damage damage)
    {
        switch (damage.ApplicationType)
        {
            case ApplicationType.Flat:
                if (damage.Amount <= 0) return;
                DamageTaken += damage.Amount;
                break;
            case ApplicationType.CurrentHealthPercent:
                if (damage.Amount is 0 or > 1) return;
                DamageTaken += (MaxHealth - DamageTaken) * damage.Amount;
                break;
            case ApplicationType.MaxHealthPercent:
                if (damage.Amount is 0 or > 1) return;
                DamageTaken += MaxHealth * damage.Amount;
                break;
            default:
                return;
        }
    }
}