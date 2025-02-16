[System.Serializable]
public class StatModifier : Stat
{
    public bool _isPermanent;
    public int _remainingTurns;
    public int _modifierValue = 0;
    public int _modifierPercent = 0;
    public int _minumumTurns = 2;
    public int _maximumTurns = 6;
}
