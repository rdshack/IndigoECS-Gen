using System.Text;

namespace ecs_gen.Generation;

public static class ComponentTypeIndexGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    Dictionary<string, int> _compNameToIndex = new Dictionary<string, int>();

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("//Generated code\n\n");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using ecs;\n");
    sb.AppendLine("public class ComponentDefinitions : IComponentDefinitions\n{");
    sb.AppendLine("  private List<ComponentTypeIndex>             _indices   = new List<ComponentTypeIndex>();");
    sb.AppendLine("  private Dictionary<Type, ComponentTypeIndex> _typeToIdx = new Dictionary<Type, ComponentTypeIndex>();");
    sb.AppendLine("  private Dictionary<ComponentTypeIndex, Type> _idxToType = new Dictionary<ComponentTypeIndex, Type>();\n");
    
    sb.AppendLine("  private HashSet<Type>               _singletonComponentTypes         = new HashSet<Type>();");
    sb.AppendLine("  private HashSet<ComponentTypeIndex> _singletonComponentTypeIndices   = new HashSet<ComponentTypeIndex>();");
    sb.AppendLine("  private List<ComponentTypeIndex>    _singletonComponentList          = new List<ComponentTypeIndex>();");
    sb.AppendLine("  private List<ComponentTypeIndex> _singleFrameComponentTypeIndices = new List<ComponentTypeIndex>();\n");
    
    sb.AppendLine("  public ComponentDefinitions()\n  {");

    for (int i = 0; i < schemaData.Components.Count; i++)
    {
      _compNameToIndex.Add(schemaData.Components[i].Name, i);
      sb.AppendLine($"    var idx{i} = new ComponentTypeIndex({i});");
      sb.AppendLine($"    _indices.Add(idx{i});");
      sb.AppendLine($"    _typeToIdx[typeof({schemaData.Components[i].Name})] = idx{i};");
      sb.AppendLine($"    _idxToType[idx{i}] = typeof({schemaData.Components[i].Name});");
      sb.AppendLine();
    }
    
    sb.AppendLine("    foreach(var idx in _indices)");
    sb.AppendLine("    {");
    sb.AppendLine("      var componentType = _idxToType[idx];");
    sb.AppendLine("      bool isSingleton = Attribute.GetCustomAttribute(componentType, typeof(SingletonComponent)) != null;");
    sb.AppendLine("      if(isSingleton)");
    sb.AppendLine("      {");
    sb.AppendLine("        _singletonComponentTypes.Add(componentType);");
    sb.AppendLine("        _singletonComponentTypeIndices.Add(idx);");
    sb.AppendLine("        _singletonComponentList.Add(idx);");
    sb.AppendLine("      }");
    sb.AppendLine("      bool isSingleFrame = Attribute.GetCustomAttribute(componentType, typeof(SingleFrameComponent)) != null;");
    sb.AppendLine("      if(isSingleFrame)");
    sb.AppendLine("      {");
    sb.AppendLine("        _singleFrameComponentTypeIndices.Add(idx);");
    sb.AppendLine("      }");
    sb.AppendLine("    }");
    sb.AppendLine("  }\n");

    sb.AppendLine("  public Type GetComponentType(ComponentTypeIndex idx)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _idxToType[idx];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public ComponentTypeIndex GetIndex(IComponent c)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _typeToIdx[c.GetType()];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public ComponentTypeIndex GetIndex(Type cType)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _typeToIdx[cType];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public IEnumerable<ComponentTypeIndex> GetTypeIndices()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _indices;");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public ComponentTypeIndex GetIndex<T>() where T : IComponent, new()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _typeToIdx[typeof(T)];");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public bool IsSingletonComponent(ComponentTypeIndex idx)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _singletonComponentTypeIndices.Contains(idx);");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public List<ComponentTypeIndex> GetAllSingletonComponents()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _singletonComponentList;");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public List<ComponentTypeIndex> GetAllSingleFrameComponents()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _singleFrameComponentTypeIndices;");
    sb.AppendLine("  }");
    sb.AppendLine();
    sb.AppendLine("  public bool MatchesComponentFieldKey(IComponent c, object val)");
    sb.AppendLine("  {");
    sb.AppendLine("    switch (GetIndex(c).Index)");
    sb.AppendLine("    {");

    foreach (var componentData in schemaData.Components)
    {
      ParsedComponentField keyField = null;
      foreach (var field in componentData.Fields)
      {
        if (field.IsKey)
        {
          keyField = field;
          break;
        }
      }

      if (keyField != null)
      {
        sb.AppendLine($"      case {_compNameToIndex[componentData.Name]}:");
        sb.AppendLine($"        if (val is not {ComponentGen.PrintType(keyField)} {MakeFirstLower(componentData.Name)}KeyVal)");
        sb.AppendLine("        {");
        sb.AppendLine("          return false;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        {componentData.Name} {MakeFirstLower(componentData.Name)} = ({componentData.Name}) c;");
        sb.AppendLine($"        return {MakeFirstLower(componentData.Name)}.{keyField.Name} == {MakeFirstLower(componentData.Name)}KeyVal;");
      }
    }
    
    sb.AppendLine("      default:");
    sb.AppendLine("        return false;");
    sb.AppendLine("    }");
    sb.AppendLine("  }");

    sb.AppendLine();
    sb.AppendLine("  public int CompareComponentFieldKeys(IComponent a, IComponent b)");
    sb.AppendLine("    {");
    sb.AppendLine("    if (a.GetType() != b.GetType())");
    sb.AppendLine("    {");
    sb.AppendLine("      throw new Exception();");
    sb.AppendLine("    }");
    sb.AppendLine();
    sb.AppendLine("    switch (GetIndex(a).Index)");
    sb.AppendLine("    {");

    foreach (var componentData in schemaData.Components)
    {
      ParsedComponentField keyField = null;
      foreach (var field in componentData.Fields)
      {
        if (field.IsKey)
        {
          keyField = field;
          break;
        }
      }

      if (keyField != null)
      {
        sb.AppendLine($"      case {_compNameToIndex[componentData.Name]}:");
        sb.AppendLine($"        {componentData.Name} {MakeFirstLower(componentData.Name)}A = ({componentData.Name}) a;");
        sb.AppendLine($"        {componentData.Name} {MakeFirstLower(componentData.Name)}B = ({componentData.Name}) b;");
        sb.AppendLine($"        return {MakeFirstLower(componentData.Name)}A.{keyField.Name}.CompareTo({MakeFirstLower(componentData.Name)}B.{keyField.Name});");
      }
    }
    
    sb.AppendLine("      default:");
    sb.AppendLine("        throw new Exception();");
    sb.AppendLine("    }");
    sb.AppendLine("  }");

    /*
public int CompareComponentFieldKeys(IComponent a, IComponent b)
  {
    if (a.GetType() != b.GetType())
    {
      throw new Exception();
    }
    switch (GetIndex(a).Index)
    {
      case 6:
        PlayerOwnedComponent playerOwnedComponentA = (PlayerOwnedComponent) a;
        PlayerOwnedComponent playerOwnedComponentB = (PlayerOwnedComponent) b;
        return playerOwnedComponentA.playerId.CompareTo(playerOwnedComponentB.playerId);
      case 7:
        EntityOwnedComponent entityOwnedComponentA = (EntityOwnedComponent) a;
        EntityOwnedComponent entityOwnedComponentB = (EntityOwnedComponent) b;
        return entityOwnedComponentA.ownerEntityId.CompareTo(entityOwnedComponentB.ownerEntityId);
      default:
        throw new Exception();
    }
  }
     */
    
    sb.AppendLine("}");
    
    string path = Path.Combine(outputPath, "ComponentDefinitions.cs");
    File.WriteAllText(path, sb.ToString());
  }
  
  private static string MakeFirstLower(string input)
  {
    return input[0].ToString().ToLower() + input.Substring(1);
  }
}
