using System.Text;

namespace ecs_gen.Generation;

public static class AliasLookupGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("//Generated code\n\n");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using ecs;\n");
    sb.AppendLine("public class AliasLookup : IAliasLookup\n{\n");

    int aliasNum = 1;
    foreach (var aliasDef in schemaData.Aliases)
    {
      sb.AppendLine($"  public static AliasId {aliasDef.Name} = new AliasId({aliasNum++});");
    }

    sb.AppendLine("  private       List<AliasId> _inputAlias = new List<AliasId>();");
    sb.AppendLine("  private Dictionary<AliasId, HashSet<ComponentTypeIndex>> _associatedComponents =");
    sb.AppendLine("    new Dictionary<AliasId, HashSet<ComponentTypeIndex>>();");
    sb.AppendLine();

    sb.AppendLine("  private Dictionary<Archetype, AliasId> _archetypeToAliasLookup = new Dictionary<Archetype, AliasId>();");
    sb.AppendLine();
    sb.AppendLine("  private Dictionary<AliasId, ComponentTypeIndex> _inputAliasKeyComponent =");
    sb.AppendLine("    new Dictionary<AliasId, ComponentTypeIndex>();");
    sb.AppendLine();

    sb.AppendLine("  public AliasLookup()\n  {\n");
    
    sb.AppendLine("    var index = new ComponentDefinitions();");
    sb.AppendLine("    var graph = new ArchetypeGraph(index, this);");
    foreach (var aliasDef in schemaData.Aliases)
    {
      if (aliasDef.IsInput)
      {
        sb.AppendLine($"    _inputAlias.Add({aliasDef.Name});");
        sb.AppendLine($"    _inputAliasKeyComponent.Add({aliasDef.Name}, index.GetIndex<{aliasDef.InputComponentKey}>());");
      }

      sb.AppendLine($"    var {aliasDef.Name}Indices = new HashSet<ComponentTypeIndex>()");
      sb.AppendLine("                                          {");

      foreach (var componentName in aliasDef.ComponentNames)
      {
        sb.AppendLine($"                                            index.GetIndex<{componentName}>(),  ");
      }

      sb.AppendLine("                                          };");
      
      sb.AppendLine($"    _associatedComponents.Add({aliasDef.Name}, {aliasDef.Name}Indices);");
      sb.AppendLine($"    _archetypeToAliasLookup.Add(graph.GetArchetypeFor({aliasDef.Name}Indices), {aliasDef.Name});");
    }

    sb.AppendLine("  }");
    
    sb.AppendLine("  public IEnumerable<ComponentTypeIndex> GetAssociatedComponents(AliasId id)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _associatedComponents[id];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public IEnumerable<AliasId> GetInputAlias()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _inputAlias;");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public ComponentTypeIndex GetInputAliasKeyComponent(AliasId aliasId)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _inputAliasKeyComponent[aliasId];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public AliasId GetAliasForArchetype(Archetype a)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _archetypeToAliasLookup[a];");
    sb.AppendLine("  }");

    sb.AppendLine("}");
    
    string path = Path.Combine(outputPath, "AliasLookup.cs");
    File.WriteAllText(path, sb.ToString());
  }
}