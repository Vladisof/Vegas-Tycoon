[System.Serializable]
public class Quirk
{
    public string requires_tag;
    public string conflicts_with_role;
    public string requires_role;
    public string boosts_if_with_tag;
    public string boosts_if_with_role;
    public string conflicts_with_tag;
    
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(requires_tag))
            return $"Requires tag: {requires_tag}";
        if (!string.IsNullOrEmpty(conflicts_with_role))
            return $"Conflicts with role: {conflicts_with_role}";
        if (!string.IsNullOrEmpty(requires_role))
            return $"Requires role: {requires_role}";
        if (!string.IsNullOrEmpty(boosts_if_with_tag))
            return $"Boosts if with tag: {boosts_if_with_tag}";
        if (!string.IsNullOrEmpty(boosts_if_with_role))
            return $"Boosts if with role: {boosts_if_with_role}";
        if (!string.IsNullOrEmpty(conflicts_with_tag))
            return $"Conflicts with tag: {conflicts_with_tag}";
        
        return "Unknown quirk";
    }
}