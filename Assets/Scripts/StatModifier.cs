[System.Serializable]
public class StatModifier : Stat
{
    public bool _isPermanent;
    public int _remainingTurns;
    public int _stage = 0;
    public int _modifierFactor = 0;
    public int _minumumTurns = 2;
    public int _maximumTurns = 6;
}
