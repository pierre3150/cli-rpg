namespace RpgGame.Core.Models;

public class Spell
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SpellType Type { get; set; }
    public int ManaCost { get; set; }
    public int Power { get; set; }

    /// <summary>Which classes can learn/cast this spell. Empty means all classes.</summary>
    public List<ClassType> AllowedClasses { get; set; } = new();

    public bool IsUsableBy(ClassType classType) =>
        AllowedClasses.Count == 0 || AllowedClasses.Contains(classType);
}
