using System.Text;

namespace ecs_gen.Generation;

public static class EnumGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("//Generated code\n\n");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using ecs;\n");

    foreach (var e in schemaData.Enums)
    {
      sb.AppendLine($"public enum {e.Name}");
      sb.AppendLine("{");

      for(int i = 0; i < e.ValueNames.Count; i++)
      {
        sb.Append($"  {e.ValueNames[i]}");
        if (i < e.ValueNames.Count - 1)
        {
          sb.AppendLine(",");
        }
        else
        {
          sb.AppendLine();
        }
      }

      sb.AppendLine("}"); 
    }

    string path = Path.Combine(outputPath, "GameEnums.cs");
    File.WriteAllText(path, sb.ToString());
  }
}