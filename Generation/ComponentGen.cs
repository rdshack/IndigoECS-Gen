using System.Text;

namespace ecs_gen.Generation;

public static class ComponentGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    Directory.CreateDirectory(Path.Combine(outputPath, "Components"));
    
    foreach (var componentData in schemaData.Components)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("//Generated code\n\n");
      stringBuilder.AppendLine("using System;");
      stringBuilder.AppendLine("using System.Collections.Generic;");
      stringBuilder.AppendLine("using ecs;");
      stringBuilder.AppendLine("using FixMath.NET;");
      stringBuilder.AppendLine("using SimMath;\n");

      if (componentData.IsSingleton)
      {
        stringBuilder.AppendLine($"[SingletonComponent]");
      }

      if (componentData.IsSingleFrame)
      {
        stringBuilder.AppendLine($"[SingleFrameComponent]");
      }
      stringBuilder.AppendLine($"public class {componentData.Name} : Component");
      stringBuilder.AppendLine("{");

      foreach (var f in componentData.Fields)
      {
        if (f.IsKey)
        {
          stringBuilder.AppendLine("    [KeyComponentField]");
        }
        stringBuilder.AppendLine($"    public {PrintType(f)} {f.Name};");
      }
      
      stringBuilder.AppendLine("}");
      
      string path = Path.Combine(outputPath, "Components", $"{componentData.Name}.cs");
      File.WriteAllText(path, stringBuilder.ToString()); 
    }
  }

  public static string PrintType(ParsedComponentField componentField)
  {
    switch (componentField.FieldType)
    {
      case ComponentFieldType.Bool: return "bool";
      case ComponentFieldType.String: return "string";
      case ComponentFieldType.Int: return "int";
      case ComponentFieldType.Int2: return "int2";
      case ComponentFieldType.Int3: return "int3";
      case ComponentFieldType.Float: return "float";
      case ComponentFieldType.Float2: return "float2";
      case ComponentFieldType.Float3: return "float3";
      case ComponentFieldType.Enum: return componentField.EnumName;
      case ComponentFieldType.EntityId: return "EntityId";
      case ComponentFieldType.Fix64Vec3: return "Fix64Vec3";
      case ComponentFieldType.Fix64Vec2: return "Fix64Vec2";
      case ComponentFieldType.Fix64: return "Fix64";
    }

    return "";
  }
}