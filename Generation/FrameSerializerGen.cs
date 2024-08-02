using System.Text;

namespace ecs_gen.Generation;

public static class FrameSerializerGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("//Generated code\n\n");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using ecs;");
    sb.AppendLine("using FlatBuffers;");
    sb.AppendLine("using FlatComponents;");
    sb.AppendLine("using SimMath;");
    sb.AppendLine("using FixMath.NET;");
    sb.AppendLine("using System.Text;");
    sb.AppendLine();

    sb.AppendLine("public class FlatBufferFrameSerializer : IFrameSerializer\n{");
    sb.AppendLine("  private FlatBufferBuilder         _fbb = new FlatBufferBuilder(100);");
    sb.AppendLine("  private List<Offset<ComponentSet>>  _compSets    = new List<Offset<ComponentSet>>();");
    sb.AppendLine("  private byte[]                      _tempBytes   = new byte[100];");
    sb.AppendLine("  private List<ulong>                 _entityIds   = new List<ulong>();");
    sb.AppendLine("  private List<Offset<NewEntityData>> _newEntities = new List<Offset<NewEntityData>>();");
    sb.AppendLine();

    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"  private Offset<FlatComponents.{c.Name}> _temp{c.Name}Offset;");
    }
    
    sb.AppendLine();

    sb.AppendLine("  private Action<FlatBufferBuilder, int> _startComponentStateVector = FlatComponents.InputData.StartComponentStateVector;");
    sb.AppendLine("  private Action<FlatBufferBuilder, int> _startEntityIdsVector = FlatComponents.FrameData.StartEntityIdsVector;");
    sb.AppendLine("  private Action<FlatBufferBuilder, int> _startNewEntitiesVector =   FlatComponents.FrameData.StartNewEntitiesVector;");
    
    sb.AppendLine();
    
    sb.AppendLine("  private IWorldLogger _logger;");
    sb.AppendLine("  private StringBuilder _stringBuilder;");
    sb.AppendLine();
    sb.AppendLine("  public FlatBufferFrameSerializer(IWorldLogger logger = null)");
    sb.AppendLine("  {");
    sb.AppendLine("    _logger = logger;");
    sb.AppendLine("    if (logger != null)");
    sb.AppendLine("    {");
    sb.AppendLine("      _stringBuilder = new StringBuilder();");
    sb.AppendLine("    }");
    sb.AppendLine("  }");

    sb.AppendLine();
    sb.AppendLine("  public int Serialize(ArchetypeGraph archetypeGraph, IByteArrayResizer resizer, IFrameSyncData data, ref byte[] resultBuffer)");
    sb.AppendLine("  {");
    sb.AppendLine("    _fbb.Clear();");
    sb.AppendLine("    _compSets.Clear();");

    sb.AppendLine();
    
    sb.AppendLine("    foreach (var ed in data.GetClientInputData())");
    sb.AppendLine("    {");
    sb.AppendLine("      var componentTypeIndices = archetypeGraph.GetComponentIndicesForArchetype(ed.GetArchetype());");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        IComponent c = ed.GetComponent(componentTypeIndex);");
    sb.AppendLine("        BuildComponent(componentTypeIndex, c);");
    sb.AppendLine("      }");
    sb.AppendLine();
    sb.AppendLine("      FlatComponents.ComponentSet.StartComponentSet(_fbb);");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        AddComponentToSet(componentTypeIndex);");
    sb.AppendLine("      }");
    sb.AppendLine("      _compSets.Add(ComponentSet.EndComponentSet(_fbb));");
    sb.AppendLine("    }");

    sb.AppendLine();
    
    sb.AppendLine("    VectorOffset compSetsOffset = FlatBufferUtil.AddVectorToBufferFromOffsetList(_fbb, FlatComponents.FrameSyncData.StartInputStateVector, _compSets);");
    sb.AppendLine("    var frameSyncData = FlatComponents.FrameSyncData.CreateFrameSyncData(_fbb, data.GetFrameNum(), data.GetFullStateHash(), compSetsOffset);");
    sb.AppendLine("    _fbb.Finish(frameSyncData.Value);");
    sb.AppendLine();

    sb.AppendLine("    int length = _fbb.DataBuffer.Length - _fbb.DataBuffer.Position;");
    sb.AppendLine("    if (length > resultBuffer.Length)");
    sb.AppendLine("    {");
    sb.AppendLine("      resizer.Resize(ref resultBuffer, length);");
    sb.AppendLine("    }");
    sb.AppendLine();

    sb.AppendLine("    var source = _fbb.DataBuffer.ToArraySegment(_fbb.DataBuffer.Position, length);");
    sb.AppendLine("    Array.Copy(source.Array, source.Offset, resultBuffer, 0, length);");
    sb.AppendLine();
    sb.AppendLine("    return length;");

    sb.AppendLine("}");
    
    sb.AppendLine();
    sb.AppendLine("  public int Serialize(ArchetypeGraph archetypeGraph, IByteArrayResizer resizer, IFrameInputData data, ref byte[] resultBuffer)");
    sb.AppendLine("  {");
    sb.AppendLine("    _fbb.Clear();");
    sb.AppendLine("    _compSets.Clear();");


    sb.AppendLine();

    sb.AppendLine("    foreach (var ed in data.GetComponentGroups())");
    sb.AppendLine("    {");
    sb.AppendLine("      var componentTypeIndices = archetypeGraph.GetComponentIndicesForArchetype(ed.GetArchetype());");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        IComponent c = ed.GetComponent(componentTypeIndex);");
    sb.AppendLine("        BuildComponent(componentTypeIndex, c);");
    sb.AppendLine("      }");
    sb.AppendLine();
    sb.AppendLine("      FlatComponents.ComponentSet.StartComponentSet(_fbb);");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        AddComponentToSet(componentTypeIndex);");
    sb.AppendLine("      }");
    sb.AppendLine("      _compSets.Add(ComponentSet.EndComponentSet(_fbb));");
    sb.AppendLine("    }");

    sb.AppendLine();

    sb.AppendLine("    VectorOffset compSetsOffset = FlatBufferUtil.AddVectorToBufferFromOffsetList(_fbb, _startComponentStateVector, _compSets);");
    sb.AppendLine("    var frameInputData = FlatComponents.InputData.CreateInputData(_fbb, data.GetFrameNum(), compSetsOffset);");
    sb.AppendLine();

    sb.AppendLine("    _fbb.Finish(frameInputData.Value);");
    sb.AppendLine();

    sb.AppendLine("    int length = _fbb.DataBuffer.Length - _fbb.DataBuffer.Position;");
    sb.AppendLine("    if (length > resultBuffer.Length)");
    sb.AppendLine("    {");
    sb.AppendLine("      resizer.Resize(ref resultBuffer, length);");
    sb.AppendLine("    }");
    sb.AppendLine();

    sb.AppendLine("    var source = _fbb.DataBuffer.ToArraySegment(_fbb.DataBuffer.Position, length);");
    sb.AppendLine("    Array.Copy(source.Array, source.Offset, resultBuffer, 0, length);");
    sb.AppendLine();
    sb.AppendLine("    return length;");

    sb.AppendLine("}");
    
    sb.AppendLine();
    
    
    sb.AppendLine();
    sb.AppendLine("  public void DeserializeFrame(byte[] data, IDeserializedFrameDataStore output, int dataStart)");
    sb.AppendLine("  {");
    sb.AppendLine("    FlatComponents.FrameData frameData = FlatComponents.FrameData.GetRootAsFrameData(new ByteBuffer(data, dataStart));");
    sb.AppendLine("    output.FrameNum = frameData.FrameNum;");
    sb.AppendLine("    output.NextEntityId = new EntityId(frameData.NextEntityId);");

    sb.AppendLine();
    sb.AppendLine("    for (int i = 0; i < frameData.NewEntitiesLength; i++)");
    sb.AppendLine("    {");
    sb.AppendLine("      var newEntData = frameData.NewEntities(i).Value;");
    sb.AppendLine("      output.SetNewEntityHash(new EntityId(newEntData.EntityId), newEntData.StateHash);");
    sb.AppendLine("    }");
    sb.AppendLine();
    
    sb.AppendLine("    for (int i = 0; i < frameData.EntityIdsLength; i++)");
    sb.AppendLine("    {");
    sb.AppendLine("      var id = frameData.EntityIds(i);");
    sb.AppendLine("      output.AddEntity(new EntityId(id), output.IsNewEntity(new EntityId(id)));");
    sb.AppendLine("    }");
    sb.AppendLine();

    sb.AppendLine("    for (int i = 0; i < frameData.ComponentStateLength; i++)");
    sb.AppendLine("    {");
    sb.AppendLine("      var compSet = frameData.ComponentState(i).Value;");

    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"      if (compSet.{c.Name}.HasValue)");
      sb.AppendLine("      {");
      sb.AppendLine($"        var flatC = compSet.{c.Name}.Value;");
      sb.AppendLine($"        var c = ({c.Name}) output.ComponentPool.Get(output.ComponentDefinitions.GetIndex<{c.Name}>());");
      
      foreach (var f in c.Fields)
      {
        sb.AppendLine($"        {BuildFieldStringFromFlat(f)}");
      }
      
      sb.AppendLine("        output.AddComponent(new EntityId(frameData.EntityIds(i)), c);");
      sb.AppendLine("      }");
    }
    
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    
    sb.AppendLine();
    sb.AppendLine("  public void DeserializeSyncFrame(byte[] data, IDeserializedFrameSyncStore output, int dataStart)");
    sb.AppendLine("  {");
    sb.AppendLine("    FlatComponents.FrameSyncData frameData = FlatComponents.FrameSyncData.GetRootAsFrameSyncData(new ByteBuffer(data, dataStart));");
    sb.AppendLine("    output.FrameNum = frameData.FrameNum;");
    sb.AppendLine("    output.FullStateHash = frameData.FullStateHash;");

    sb.AppendLine("    for (int i = 0; i < frameData.InputStateLength; i++)");
    sb.AppendLine("    {");
    sb.AppendLine("      var compSet = frameData.InputState(i).Value;");

    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"      if (compSet.{c.Name}.HasValue)");
      sb.AppendLine("      {");
      sb.AppendLine($"        var flatC = compSet.{c.Name}.Value;");
      sb.AppendLine($"        var c = ({c.Name}) output.ComponentPool.Get(output.ComponentDefinitions.GetIndex<{c.Name}>());");
      
      foreach (var f in c.Fields)
      {
        sb.AppendLine($"        {BuildFieldStringFromFlat(f)}");
      }
      
      sb.AppendLine("        output.AddComponent(new EntityId((ulong)i), c);");
      sb.AppendLine("      }");
    }
    
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine();
    sb.AppendLine("  public void DeserializeInputFrame(byte[] data, IDeserializedFrameSyncStore output, int dataStart)");
    sb.AppendLine("  {");
    sb.AppendLine("    FlatComponents.InputData frameData = FlatComponents.InputData.GetRootAsInputData(new ByteBuffer(data, dataStart));");
    sb.AppendLine("    output.FrameNum = frameData.FrameNum;");

    sb.AppendLine("    for (int i = 0; i < frameData.ComponentStateLength; i++)");
    sb.AppendLine("    {");
    sb.AppendLine("      var compSet = frameData.ComponentState(i).Value;");

    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"      if (compSet.{c.Name}.HasValue)");
      sb.AppendLine("      {");
      sb.AppendLine($"        var flatC = compSet.{c.Name}.Value;");
      sb.AppendLine($"        var c = ({c.Name}) output.ComponentPool.Get(output.ComponentDefinitions.GetIndex<{c.Name}>());");
      
      foreach (var f in c.Fields)
      {
        sb.AppendLine($"        {BuildFieldStringFromFlat(f)}");
      }
      
      sb.AppendLine("        output.AddComponent(new EntityId((ulong)i), c);");
      sb.AppendLine("      }");
    }
    
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    sb.AppendLine();

    sb.AppendLine("  public int Serialize(ArchetypeGraph archetypeGraph, IByteArrayResizer resizer, IFrameData data, ref byte[] resultBuffer)");
    sb.AppendLine("  {");
    sb.AppendLine("    _stringBuilder?.Clear();");
    sb.AppendLine("    _stringBuilder?.AppendLine($\"Serializing frame '{data.GetFrameNum()}' with next id '{data.GetEntityRepo().GetNextEntityId().Id}'\");");
    sb.AppendLine("    _fbb.Clear();");
    sb.AppendLine("    _compSets.Clear();");
    sb.AppendLine("    _newEntities.Clear();");
    sb.AppendLine("    _entityIds.Clear();");
    sb.AppendLine();
    
    sb.AppendLine();
    sb.AppendLine("    foreach (var ed in data.GetEntityRepo().GetEntitiesData())");
    sb.AppendLine("    {");
    sb.AppendLine($"      _stringBuilder?.AppendLine($\"Entity '{{ed.GetEntityId().Id}}'\");");
    sb.AppendLine("      _entityIds.Add(ed.GetEntityId().Id);");
    sb.AppendLine("      var componentTypeIndices = archetypeGraph.GetComponentIndicesForArchetype(ed.GetArchetype());");
    sb.AppendLine("      bool createdThisFrame = data.IsNewEntity(ed.GetEntityId());");
    sb.AppendLine("      int startPos = _fbb.DataBuffer.Position;");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        IComponent c = data.GetEntityRepo().GetEntityComponent(ed.GetEntityId(), componentTypeIndex);");
    sb.AppendLine("        BuildComponent(componentTypeIndex, c);");
    sb.AppendLine("      }");
    sb.AppendLine();
    sb.AppendLine("      FlatComponents.ComponentSet.StartComponentSet(_fbb);");
    sb.AppendLine("      foreach (var componentTypeIndex in componentTypeIndices)");
    sb.AppendLine("      {");
    sb.AppendLine("        AddComponentToSet(componentTypeIndex);");
    sb.AppendLine("      }");
    sb.AppendLine("      _compSets.Add(ComponentSet.EndComponentSet(_fbb));");
    sb.AppendLine();
    sb.AppendLine("      if (createdThisFrame)");
    sb.AppendLine("      {");
    sb.AppendLine("         var seg = _fbb.DataBuffer.ToArraySegment(startPos, _fbb.DataBuffer.Position - startPos);");
    sb.AppendLine("        int compHash = CreateStateHash(seg.Array, startPos, _fbb.DataBuffer.Position - startPos);");
    sb.AppendLine("      }");
    sb.AppendLine("    }");

    sb.AppendLine();

    sb.AppendLine($"    VectorOffset ids = FlatBufferUtil.AddVectorToBufferFromUlongList(_fbb, _startEntityIdsVector, _entityIds);");
    sb.AppendLine($"    VectorOffset newEntityData = FlatBufferUtil.AddVectorToBufferFromOffsetList(_fbb, _startNewEntitiesVector, _newEntities);");
    sb.AppendLine($"    VectorOffset compSetsOffset = FlatBufferUtil.AddVectorToBufferFromOffsetList(_fbb, _startComponentStateVector, _compSets);");
    
    sb.AppendLine("    var frameSyncData = FlatComponents.FrameData.CreateFrameData(_fbb, data.GetEntityRepo().GetNextEntityId().Id, data.GetFrameNum(), newEntityData, ids, compSetsOffset );");
    sb.AppendLine("    _fbb.Finish(frameSyncData.Value);");
    sb.AppendLine();

    sb.AppendLine("    int length = _fbb.DataBuffer.Length - _fbb.DataBuffer.Position;");
    sb.AppendLine("    if (length > resultBuffer.Length)");
    sb.AppendLine("    {");
    sb.AppendLine("      resizer.Resize(ref resultBuffer, length);");
    sb.AppendLine("    }");
    sb.AppendLine();
    sb.AppendLine("    var source = _fbb.DataBuffer.ToArraySegment(_fbb.DataBuffer.Position, length);");
    sb.AppendLine("    Array.Copy(source.Array, source.Offset, resultBuffer, 0, length);");
    sb.AppendLine();
    sb.AppendLine("    _logger?.Log(_stringBuilder.ToString());");
    sb.AppendLine("    return length;");

    sb.AppendLine("}");
    
    sb.AppendLine();

    sb.AppendLine("  private void BuildComponent(ComponentTypeIndex componentTypeIndex, IComponent component)");
    sb.AppendLine("  {");
    sb.AppendLine("    switch (componentTypeIndex.Index)");
    sb.AppendLine("    {");
    int i = 0;
    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"      case {i++}: Build{c.Name}(({c.Name})component); break;");
    }
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    
    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"  private void Build{c.Name}({c.Name} c)");
      sb.AppendLine("  {");
      sb.AppendLine($"    _stringBuilder?.AppendLine($\"{c.Name}:\");");
      sb.AppendLine($"    FlatComponents.{c.Name}.Start{c.Name}(_fbb);");

      foreach (var f in c.Fields)
      {
        sb.AppendLine($"    _stringBuilder?.AppendLine($\"{f.Name}: {FieldCreatePrintString(f)}\");");
        sb.AppendLine($"    FlatComponents.{c.Name}.Add{MakeFirstUpper(f.Name)}(_fbb, {FieldCreateString(f)});"); 
      }

      sb.AppendLine($"_temp{c.Name}Offset = FlatComponents.{c.Name}.End{c.Name}(_fbb);");
      sb.AppendLine("  }");
    }

    sb.AppendLine();
    
    sb.AppendLine($"  public int CreateStateHash(byte[] seg, int pos, int len)");
    sb.AppendLine("  {");
    sb.AppendLine("    unchecked");
    sb.AppendLine("    {");
    sb.AppendLine("      const int p = 16777619;");
    sb.AppendLine("      int hash = (int)2166136261;");
    sb.AppendLine("      for (int i = pos; i < len; i++)");
    sb.AppendLine("        hash = (hash ^ seg[i]) * p;;");
    sb.AppendLine("      return hash;");
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine($"  private void AddComponentToSet(ComponentTypeIndex componentTypeIndex)");
    sb.AppendLine("  {");
    sb.AppendLine("    switch (componentTypeIndex.Index)");
    sb.AppendLine("    {");
    i = 0;
    foreach (var c in schemaData.Components)
    {
      sb.AppendLine($"      case {i++}: FlatComponents.ComponentSet.Add{c.Name}(_fbb, _temp{c.Name}Offset); break;"); 
    }
    sb.AppendLine("    }");
    sb.AppendLine("  }");
        
    sb.AppendLine("}");
    
    string path = Path.Combine(outputPath, "FlatBufferFrameSerializer.cs");
    File.WriteAllText(path, sb.ToString());
  }

  private static string? BuildFieldStringFromFlat(ParsedComponentField cField)
  {
    switch (cField.FieldType)
    {
      case ComponentFieldType.Bool:
      case ComponentFieldType.Float:
      case ComponentFieldType.Int:
      case ComponentFieldType.String:
        return $"c.{cField.Name} = flatC.{cField.Name.ToFirstUpper()};";
      case ComponentFieldType.Float2:
        return $"c.{cField.Name} = new float2(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y);";
      case ComponentFieldType.Float3:
        return $"c.{cField.Name} = new float3(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y, flatC.{cField.Name.ToFirstUpper()}.Value.Z);";
      case ComponentFieldType.Float4:
        return $"c.{cField.Name} = new float4(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y, flatC.{cField.Name.ToFirstUpper()}.Value.Z,  flatC.{cField.Name.ToFirstUpper()}.Value.W);";
      case ComponentFieldType.Int2:
        return $"c.{cField.Name} = new int2(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y);";
      case ComponentFieldType.Int3:
        return $"c.{cField.Name} = new int3(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y, flatC.{cField.Name.ToFirstUpper()}.Value.Z);";
      case ComponentFieldType.Int4:
        return $"c.{cField.Name} = new int4(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y, flatC.{cField.Name.ToFirstUpper()}.Value.Z,  flatC.{cField.Name.ToFirstUpper()}.Value.W);";
      case ComponentFieldType.Enum:
        return $"c.{cField.Name} = ({cField.EnumName})flatC.{cField.Name.ToFirstUpper()};";
      case ComponentFieldType.EntityId:
        return $"c.{cField.Name} = new EntityId(flatC.{cField.Name.ToFirstUpper()});";
      case ComponentFieldType.Fix64:
        return $"c.{cField.Name} = Fix64.FromRaw(flatC.{cField.Name.ToFirstUpper()});";
      case ComponentFieldType.Fix64Vec2:
        return $"c.{cField.Name} = SimMath.Fix64Vec2.FromRaw(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y);";
      case ComponentFieldType.Fix64Vec3:
        return $"c.{cField.Name} = SimMath.Fix64Vec3.FromRaw(flatC.{cField.Name.ToFirstUpper()}.Value.X, flatC.{cField.Name.ToFirstUpper()}.Value.Y, flatC.{cField.Name.ToFirstUpper()}.Value.Z);";
    }

    throw new Exception();
  }

  private static string MakeFirstUpper(string input)
  {
    return input[0].ToString().ToUpper() + input.Substring(1);
  }
  
  private static string ToFirstUpper(this string input)
  {
    return input[0].ToString().ToUpper() + input.Substring(1);
  }

  private static string FieldCreateString(ParsedComponentField cField)
  {
    switch (cField.FieldType)
    {
      case ComponentFieldType.Bool:
      case ComponentFieldType.Float:
      case ComponentFieldType.Int:
      case ComponentFieldType.String:
        return $"c.{cField.Name}";
      case ComponentFieldType.Float2:
        return $"Float2.CreateFloat2(_fbb, c.{cField.Name}.x, c.{cField.Name}.y)";
      case ComponentFieldType.Float3:
        return $"Float3.CreateFloat3(_fbb, c.{cField.Name}.x, c.{cField.Name}.y, c.{cField.Name}.z)";
      case ComponentFieldType.Float4:
        return $"Float4.CreateFloat3(_fbb, c.{cField.Name}.x, c.{cField.Name}.y, c.{cField.Name}.z, c.{cField.Name}.w)";
      case ComponentFieldType.Int2:
        return $"Int2.CreateInt2(_fbb, c.{cField.Name}.x, c.{cField.Name}.y)";
      case ComponentFieldType.Int3:
        return $"Int3.CreateInt3(_fbb, c.{cField.Name}.x, c.{cField.Name}.y, c.{cField.Name}.z)";
      case ComponentFieldType.Int4:
        return $"Int4.CreateInt4(_fbb, c.{cField.Name}.x, c.{cField.Name}.y, c.{cField.Name}.z, c.{cField.Name}.w)";
      case ComponentFieldType.Enum:
        return $"(sbyte)c.{cField.Name}";
      case ComponentFieldType.EntityId:
        return $"c.{cField.Name}.Id";
      case ComponentFieldType.Fix64:
        return $"c.{cField.Name}.RawValue";
      case ComponentFieldType.Fix64Vec2:
        return $"FlatComponents.Fix64Vec2.CreateFix64Vec2(_fbb, c.{cField.Name}.x.RawValue, c.{cField.Name}.y.RawValue)";
      case ComponentFieldType.Fix64Vec3:
        return $"FlatComponents.Fix64Vec3.CreateFix64Vec3(_fbb, c.{cField.Name}.x.RawValue, c.{cField.Name}.y.RawValue, c.{cField.Name}.z.RawValue)";
    }

    return "";
  }
  
  private static string FieldCreatePrintString(ParsedComponentField cField)
  {
    switch (cField.FieldType)
    {
      case ComponentFieldType.Bool:
      case ComponentFieldType.Float:
      case ComponentFieldType.Int:
      case ComponentFieldType.String:
        return $"{{c.{cField.Name}}}";
      case ComponentFieldType.Float2:
      case ComponentFieldType.Float3:
      case ComponentFieldType.Float4:
      case ComponentFieldType.Int2:
        return $"x:{{c.{cField.Name}.x}}, y:{{c.{cField.Name}.y}}";
      case ComponentFieldType.Int3:
        return $"x:{{c.{cField.Name}.x}}, y:{{c.{cField.Name}.y}}, z:{{c.{cField.Name}.z}}";
      case ComponentFieldType.Int4:
        return $"";
      case ComponentFieldType.Enum:
        return $"(sbyte)c.{cField.Name}";
      case ComponentFieldType.EntityId:
        return $"c.{cField.Name}.Id";
      case ComponentFieldType.Fix64:
        return $"{{c.{cField.Name}.RawValue}}";
      case ComponentFieldType.Fix64Vec2:
        return $"x:{{c.{cField.Name}.x.RawValue}}, y:{{c.{cField.Name}.y.RawValue}}";
      case ComponentFieldType.Fix64Vec3:
        return $"x:{{c.{cField.Name}.x.RawValue}}, y:{{c.{cField.Name}.y.RawValue}}, z:{{c.{cField.Name}.z.RawValue}}";
    }

    return "";
  }
}