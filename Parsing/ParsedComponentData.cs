namespace ecs_gen;

public class ParsedSchemaData
{
  public List<ParsedComponentData> Components = new List<ParsedComponentData>();
  public List<ParsedAliasData>     Aliases    = new List<ParsedAliasData>();
  public List<ParsedEnumData>      Enums      = new List<ParsedEnumData>();
}

public class ParsedComponentData
{
  public string                     Name;
  public bool                       IsSingleton;
  public bool                       IsSingleFrame;
  public List<ParsedComponentField> Fields = new List<ParsedComponentField>();
}

public enum ComponentFieldType
{
  Bool,
  Int,
  Int2,
  Int3,
  Int4,
  Float,
  Float2,
  Float3,
  Float4,
  String,
  Enum,
  EntityId,
  Fix64Vec3,
  Fix64Vec2,
  Fix64
}

public class ParsedComponentField
{
  public ComponentFieldType FieldType;
  public string             EnumName;
  public string             Name;
  public bool               IsKey;
}