using System.Diagnostics;
using System.Text;

namespace ecs_gen.Generation;

public class FlatBuffersGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    StringBuilder stringBuilder = new StringBuilder();
    
    stringBuilder.AppendLine($"namespace FlatComponents;");

    /*foreach (var e in schemaData.Enums)
    {
        stringBuilder.AppendLine($"enum {e.Name} : byte");
        stringBuilder.AppendLine("{");
        
        for(int i = 0; i < e.ValueNames.Count; i++)
        {
            stringBuilder.Append($"{e.ValueNames[i]}");
            if (i < e.ValueNames.Count - 1)
            {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.AppendLine("}");  
    }*/

    stringBuilder.AppendLine($"struct Float2");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : float;");
    stringBuilder.AppendLine("	y : float;");  
    stringBuilder.AppendLine("}");  
    
    stringBuilder.AppendLine($"struct Float3");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : float;");
    stringBuilder.AppendLine("	y : float;");  
    stringBuilder.AppendLine("	z : float;");  
    stringBuilder.AppendLine("}");  
    
    stringBuilder.AppendLine($"struct Fix64Vec2");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : long;");
    stringBuilder.AppendLine("	y : long;");  
    stringBuilder.AppendLine("}");  
    
    stringBuilder.AppendLine($"struct Fix64Vec3");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : long;");
    stringBuilder.AppendLine("	y : long;");  
    stringBuilder.AppendLine("	z : long;");  
    stringBuilder.AppendLine("}");  
    
    stringBuilder.AppendLine($"struct Int2");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : int;");
    stringBuilder.AppendLine("	y : int;");  
    stringBuilder.AppendLine("}");  
    
    stringBuilder.AppendLine($"struct Int3");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	x : int;");
    stringBuilder.AppendLine("	y : int;");  
    stringBuilder.AppendLine("	z : int;");  
    stringBuilder.AppendLine("}");  

    foreach (var c in schemaData.Components)
    {
        stringBuilder.AppendLine($"table {c.Name}");
        stringBuilder.AppendLine("{");
        
        foreach (var f in c.Fields)
        {
            stringBuilder.Append($"	{f.Name} : {ConvertType(f)};");   
        }
        stringBuilder.AppendLine("}");   
    }
    
    /*
     *table ComponentSet
     *{
     *  componentName : ComponentName;
     *  ...
     *
     *}
     * 
     */

    stringBuilder.AppendLine($"table ComponentSet");
    stringBuilder.AppendLine("{");
    foreach (var c in schemaData.Components)
    {
        string compName = $"{c.Name.Substring(0, 1).ToLower()}{c.Name.Substring(1, c.Name.Length-1)}";
        stringBuilder.AppendLine($"	{compName} : {c.Name};");   
    }
    stringBuilder.AppendLine("}");
    
    stringBuilder.AppendLine($"table NewEntityData");
    stringBuilder.AppendLine("{");
    stringBuilder.AppendLine("	entityId : ulong;");
    stringBuilder.AppendLine("	stateHash : int;");  
    stringBuilder.AppendLine("}");  

    stringBuilder.Append("table FrameData");
    stringBuilder.Append("{");
    stringBuilder.Append("  nextEntityId : ulong;"); 
    stringBuilder.Append("  frameNum : int;");
    stringBuilder.Append("  newEntities : [NewEntityData];");
    stringBuilder.Append("  entityIds : [ulong];");
    stringBuilder.Append("  componentState : [ComponentSet];");
    stringBuilder.Append("}");
    
    stringBuilder.Append("table InputData");
    stringBuilder.Append("{");
    stringBuilder.Append("  frameNum : int;");
    stringBuilder.Append("  componentState : [ComponentSet];");
    stringBuilder.Append("}");
    
    stringBuilder.Append("table FrameSyncData");
    stringBuilder.Append("{");
    stringBuilder.Append("  frameNum : int;");
    stringBuilder.Append("  fullStateHash : int;");
    stringBuilder.Append("  inputState : [ComponentSet];");
    stringBuilder.Append("}");

    string flatPath = Path.Combine(outputPath, "FlatBuffers");
    Directory.CreateDirectory(flatPath);
    
    string path = Path.Combine(flatPath, "ecs_flat.fbs");
    File.WriteAllText(path, stringBuilder.ToString());

    RunFlatC(path, "FlatBuffers");
  }

  private static string ConvertType(ParsedComponentField field)
  {
      switch (field.FieldType)
      {
          case ComponentFieldType.Bool: return "bool";
          case ComponentFieldType.Float: return "float";
          case ComponentFieldType.Float2: return "Float2";
          case ComponentFieldType.Float3: return "Float3";
          case ComponentFieldType.Int: return "int";
          case ComponentFieldType.Int2: return "Int2";
          case ComponentFieldType.Int3: return "Int3";
          case ComponentFieldType.String: return "string";
          case ComponentFieldType.Enum: return "byte";
          case ComponentFieldType.EntityId: return "ulong";
          case ComponentFieldType.Fix64Vec2: return "Fix64Vec2";
          case ComponentFieldType.Fix64Vec3: return "Fix64Vec3";
          case ComponentFieldType.Fix64: return "long";
      }

      return "";
  }

  private static void RunFlatC(string fileName, string outputPath)
  { 
    FileInfo f = new FileInfo(fileName);
    DirectoryInfo di = new DirectoryInfo(outputPath);

    ProcessStartInfo procInfo = new ProcessStartInfo();

    procInfo.FileName = "flatc";
    procInfo.Arguments = $" -I {di.FullName} --csharp " + f.FullName;

    procInfo.UseShellExecute = false;
    procInfo.RedirectStandardError = false;
    procInfo.RedirectStandardInput = true;
    procInfo.RedirectStandardOutput = false;
    procInfo.CreateNoWindow = false;

    procInfo.WorkingDirectory = f.DirectoryName;
    procInfo.WindowStyle = ProcessWindowStyle.Normal;

    Process process = new Process();
    process.StartInfo = procInfo;
    process.Start();
  }
}