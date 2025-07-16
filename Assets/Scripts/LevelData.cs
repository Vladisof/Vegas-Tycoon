using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int requiredStars;
    public int requiredMoney;
    public List<LevelRoleRequirement> requiredRoles; // наприклад, 1 Singer, 1 Dancer
    public List<string> recommendedTags;
    public List<LevelRestriction> restrictions; // наприклад, "No Magician"
}