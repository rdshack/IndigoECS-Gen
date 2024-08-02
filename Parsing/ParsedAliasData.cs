namespace ecs_gen;

public class ParsedAliasData
{
  public string Name;
  public bool   IsInput;
  public string InputComponentKey;
  public List<string> ComponentNames = new List<string>();
}

public class RawAttribute
{
  public string       Name;
  public List<string> Params = new List<string>();
}