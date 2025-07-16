[System.Serializable]
public class PerformerData
{
    public string name;
    public string role;
    public int skill;
    public Quirk[] quirks;
    public string[] tags;
    public int cost;
    public int localLevel = 1;
    [System.NonSerialized]
    public bool isPurchased = false;
    public int GetSkill() => skill + (localLevel-1);
    public int GetUpgradeCost() => cost * localLevel;
    public int GetPurchaseCost()
    {
        // Define purchase cost logic here
        return cost * 10; // Example: 10x base price to purchase
    }
}