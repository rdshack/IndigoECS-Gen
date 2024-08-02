using System.Text;

namespace ecs_gen.Generation;

public static class ComponentCopierGen
{
  public static void Gen(string outputPath, ParsedSchemaData schemaData)
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("//Generated code\n\n");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using ecs;");
    sb.AppendLine("using System.Text;\n");
    sb.AppendLine("public class ComponentCopier : IComponentFactory\n{");
    sb.AppendLine("  private const int DEFAULT_POOL_SIZE = 5;");
    sb.AppendLine();
    sb.AppendLine("  private ComponentDefinitions _componentDefinitions;");
    sb.AppendLine("  private Dictionary<Type, Action<IComponent, IComponent>> copyMethods;\n");
    sb.AppendLine("  private Dictionary<Type, Action<IComponent>> resetMethods;\n");
    sb.AppendLine("  private List<ObjPool<IComponent>>                           _poolList = new List<ObjPool<IComponent>>();");
    sb.AppendLine("  private Dictionary<ComponentTypeIndex, ObjPool<IComponent>> _pools = new Dictionary<ComponentTypeIndex, ObjPool<IComponent>>();");
    sb.AppendLine("  private ObjPool<EntityData>     _entityDataPool = new ObjPool<EntityData>(EntityData.Create, EntityData.Reset);");
    sb.AppendLine("  private ObjPool<ComponentGroup> _compGroupPool  = new ObjPool<ComponentGroup>(ComponentGroup.Create, ComponentGroup.Reset);");
    
    sb.AppendLine();
    sb.AppendLine("  public ComponentCopier(ComponentDefinitions componentDefinitions)\n  {");
    sb.AppendLine("    _componentDefinitions = componentDefinitions;");
    sb.AppendLine();
    sb.AppendLine("    copyMethods = new Dictionary<Type, Action<IComponent, IComponent>>();\n");
    sb.AppendLine("    resetMethods = new Dictionary<Type, Action<IComponent>>();\n");

    foreach (var ct in schemaData.Components)
    {
      sb.AppendLine($"    copyMethods[typeof({ct.Name})] = Copy{ct.Name};");
    }

    sb.AppendLine();
    foreach (var ct in schemaData.Components)
    {
      sb.AppendLine($"    resetMethods[typeof({ct.Name})] = Reset{ct.Name};");
    }
    
    sb.AppendLine();
    foreach (var ct in schemaData.Components)
    {
      sb.AppendLine($"    _pools[_componentDefinitions.GetIndex<{ct.Name}>()] = new ObjPool<IComponent>(Build{ct.Name}, Reset{ct.Name});");
      sb.AppendLine($"    _poolList.Add(_pools[_componentDefinitions.GetIndex<{ct.Name}>()]);");
    }
    
    sb.AppendLine("  }\n");
    sb.AppendLine();

    sb.AppendLine("  public void ReturnAll()");
    sb.AppendLine("  {");
    sb.AppendLine("    _compGroupPool.ReturnAll();");
    sb.AppendLine("    _entityDataPool.ReturnAll();");
    sb.AppendLine();
    sb.AppendLine("    foreach (var pool in _poolList)");
    sb.AppendLine("    {");
    sb.AppendLine("      pool.ReturnAll();");
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    sb.AppendLine();

    sb.AppendLine("  public EntityData GetEntityData()");
    sb.AppendLine("  {");
    sb.AppendLine("    return _entityDataPool.Get();");
    sb.AppendLine("  }");
    sb.AppendLine();

    sb.AppendLine("  public void ReturnEntityData(IComponentGroup e)");
    sb.AppendLine("  {");
    sb.AppendLine("    if (e is EntityData entityData)");
    sb.AppendLine("    {");
    sb.AppendLine("      _entityDataPool.Return(entityData);");
    sb.AppendLine("    }");
    sb.AppendLine("    else if (e is ComponentGroup componentGroup)");
    sb.AppendLine("    {");
    sb.AppendLine("      _compGroupPool.Return(componentGroup);");
    sb.AppendLine("    }");
    sb.AppendLine("    else");
    sb.AppendLine("    {");
    sb.AppendLine("      throw new Exception();");
    sb.AppendLine("    }");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine("  public ComponentGroup GetComponentGroup(ArchetypeGraph graph, IComponentDefinitions definitions)");
    sb.AppendLine("  {");
    sb.AppendLine("    ComponentGroup group = _compGroupPool.Get();");
    sb.AppendLine("    group.Setup(this, graph, definitions);");
    sb.AppendLine("    return group;");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine("  public IComponent Get(ComponentTypeIndex idx)");
    sb.AppendLine("  {");
    sb.AppendLine("    return _pools[idx].Get();");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine("  public void Return(IComponent component)");
    sb.AppendLine("  {");
    sb.AppendLine("    _pools[_componentDefinitions.GetIndex(component)].Return(component);");
    sb.AppendLine("  }");
    sb.AppendLine();
    
    sb.AppendLine("  public void Copy(IComponent source, IComponent target)\n  {");
    sb.AppendLine("    Type sourceType = source.GetType();");
    sb.AppendLine("    copyMethods[sourceType](source, target);\n  }\n");
    
    sb.AppendLine("  public void Reset(IComponent c)\n  {");
    sb.AppendLine("    Type sourceType = c.GetType();");
    sb.AppendLine("    resetMethods[sourceType](c);\n  }\n");

    foreach (var ct in schemaData.Components)
    {
      sb.AppendLine($"  private void Copy{ct.Name}(IComponent s, IComponent t)");
      sb.AppendLine("  {");
      sb.AppendLine($"    {ct.Name} source = ({ct.Name}) s;");
      sb.AppendLine($"    {ct.Name} target = ({ct.Name}) t;");

      foreach (var f in ct.Fields)
      {
        sb.AppendLine($"    target.{f.Name} = source.{f.Name};");
      }

      sb.AppendLine("  }");
      
      sb.AppendLine($"  private void Reset{ct.Name}(IComponent c)");
      sb.AppendLine("  {");
      sb.AppendLine($"    {ct.Name} source = ({ct.Name}) c;");

      foreach (var f in ct.Fields)
      {
        sb.AppendLine($"    source.{f.Name} = default;");
      }

      sb.AppendLine("  }");
      
      sb.AppendLine($"  private Component Build{ct.Name}()");
      sb.AppendLine("  {");
      sb.AppendLine($"    return new {ct.Name}();");
      sb.AppendLine("  }");
    }

    sb.AppendLine();
    
    sb.AppendLine($"  public string ToString(IComponent c)");
    sb.AppendLine("  {");
    sb.AppendLine($"       StringBuilder sb = new StringBuilder();");
    sb.AppendLine($"    sb.AppendLine(c.GetType().ToString());");

    foreach (var c in schemaData.Components)
    {
      string castedComponentName = MakeFirstLower(c.Name);
      sb.AppendLine($"    if (c is {c.Name} {castedComponentName})");
      sb.AppendLine("    {");
      foreach (var f in c.Fields)
      {
        sb.AppendLine($"      sb.AppendLine($\"{f.Name}: {PrintFieldContent(f, castedComponentName)}\");");
      }
      sb.AppendLine("     return sb.ToString();");
      sb.AppendLine("    }");
    }

    sb.AppendLine("    throw new Exception();");
    sb.AppendLine("  }");
    
    
    /*
public string ToString(IComponent c)
  {
    StringBuilder sb = new StringBuilder();
    sb.AppendLine(c.GetType().ToString());
    if (c is PositionComponent positionComponent)
    {
      sb.AppendLine($"pos: ({positionComponent.pos.x} , {positionComponent.pos.y} , {positionComponent.pos.z})");
      return sb.ToString();
    }
    if (c is VelocityComponent velocityComponent)
    {
      sb.AppendLine($"velo: ({velocityComponent.velo.x} , {velocityComponent.velo.y} , {velocityComponent.velo.z})");
      return sb.ToString();
    }
    if (c is TimeComponent timeComponent)
    {
      sb.AppendLine($"deltaTimeMs: {timeComponent.deltaTimeMs}");
      return sb.ToString();
    }
    if (c is CoreUnitStateComponent coreUnitStateComponent)
    {
      sb.AppendLine($"activeStateId: {coreUnitStateComponent.activeStateId.Id}");
      return sb.ToString();
    }
    if (c is PlayerOwnedComponent playerOwnedComponent)
    {
      sb.AppendLine($"playerId: {playerOwnedComponent.playerId}");
      return sb.ToString();
    }
    if (c is EntityOwnedComponent entityOwnedComponent)
    {
      sb.AppendLine($"ownerEntityId: {entityOwnedComponent.ownerEntityId.Id}");
      return sb.ToString();
    }
    if (c is PlayerInputComponent playerInputComponent)
    {
      sb.AppendLine($"moveInput: ({playerInputComponent.moveInput.x}, {playerInputComponent.moveInput.y})");
      sb.AppendLine($"attackPressed: {playerInputComponent.attackPressed}");
      return sb.ToString();
    }
    if (c is AddPlayerInputComponent addPlayerInputComponent)
    {
      sb.AppendLine($"player: {addPlayerInputComponent.playerId}");
      return sb.ToString();
    }
    if (c is StateMachineStateComponent stateMachineStateComponent)
    {
      sb.AppendLine($"stateMachineId: {stateMachineStateComponent.stateMachineId}");
      sb.AppendLine($"curStateId: {stateMachineStateComponent.curStateId}");
      sb.AppendLine($"stateElapedTimeMs: {stateMachineStateComponent.stateElapedTimeMs}");
      sb.AppendLine($"stateMachineElapsedTimeMs: {stateMachineStateComponent.stateMachineElapsedTimeMs}");
      return sb.ToString();
    }
    if (c is RequestDestroyComponent requestDestroyComponent)
    {
      return sb.ToString();
    }
    if (c is FacingDirectionComponent facingDirectionComponent)
    {
      sb.AppendLine($"turnSpeed: {facingDirectionComponent.turnSpeed}");
      sb.AppendLine($"facingDir: ({facingDirectionComponent.facingDir.x.RawValue} , {facingDirectionComponent.facingDir.y.RawValue} , {facingDirectionComponent.facingDir.z.RawValue})");
      sb.AppendLine($"targetDir: ({facingDirectionComponent.targetDir.x.RawValue} , {facingDirectionComponent.targetDir.y.RawValue} , {facingDirectionComponent.targetDir.z.RawValue})");
      return sb.ToString();
    }

    return sb.ToString();
  }
     */
    
    sb.AppendLine("}");
    
    string path = Path.Combine(outputPath, "ComponentCopier.cs");
    File.WriteAllText(path, sb.ToString());
  }

  private static string PrintFieldContent(ParsedComponentField fieldData, string compName)
  {
    StringBuilder sb = new StringBuilder();
    switch (fieldData.FieldType)
    {
      case ComponentFieldType.Bool:
      case ComponentFieldType.Enum:
      case ComponentFieldType.Float:
      case ComponentFieldType.Int:
      case ComponentFieldType.String:
      case ComponentFieldType.Fix64:
        sb.Append($"{{{compName}.{fieldData.Name}}}");
        break;
      case ComponentFieldType.Float2:
      case ComponentFieldType.Int2:
      case ComponentFieldType.Fix64Vec2:
        sb.Append($"({{{compName}.{fieldData.Name}.x}}, {{{compName}.{fieldData.Name}.y}})");
        break;
      case ComponentFieldType.Float3:
      case ComponentFieldType.Int3:
      case ComponentFieldType.Fix64Vec3:
        sb.Append($"({{{compName}.{fieldData.Name}.x}}, {{{compName}.{fieldData.Name}.y}}, {{{compName}.{fieldData.Name}.z}})");
        break;
      case ComponentFieldType.Float4:
      case ComponentFieldType.Int4:
        sb.Append($"({{{compName}.{fieldData.Name}.x}}, {{{compName}.{fieldData.Name}.y}}, {{{compName}.{fieldData.Name}.z}}, {{{compName}.{fieldData.Name}.w}})");
        break;
      case ComponentFieldType.EntityId:
        sb.Append($"{{{compName}.{fieldData.Name}.Id}}");
        break;
    }

    return sb.ToString();
  }

  private static string MakeFirstLower(string input)
  {
    return input[0].ToString().ToLower() + input.Substring(1);
  }
}