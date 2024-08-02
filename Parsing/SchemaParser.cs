namespace ecs_gen;

public class SchemaParser
{
  private TokenizerDef      _tokenizerDef;
  private ParseStateMachine _parseStateMachine;

  public SchemaParser()
  {
    _tokenizerDef = new TokenizerDef();
    _parseStateMachine = new ParseStateMachine();
  }

  public ParsedSchemaData Parse(string sourceFolder)
  {
    ParsedSchemaData parsedSchemaData = new ParsedSchemaData();

    var schemaFiles = Directory.GetFiles(sourceFolder).Where(file => file.EndsWith(".schema")).ToList();
    List<MatchResult> tokenResults = new List<MatchResult>();
    foreach (var schema in schemaFiles)
    {
      string txt = File.ReadAllText(schema);
      TokenPass(txt, tokenResults);
    }
    
    SchemaEnumPass(new List<MatchResult>(tokenResults), parsedSchemaData);
    SchemaPass(new List<MatchResult>(tokenResults), parsedSchemaData);

    return parsedSchemaData;
  }

  private bool TokenPass(string sampleSchema, List<MatchResult> results)
  {
    _parseStateMachine.ResetState();
    var curState = _parseStateMachine.GetCurState();

    int cursorPosition = 0;
    int selectionLength = 1;
    while (cursorPosition + selectionLength <= sampleSchema.Length)
    {
      bool isFinalChar = ((cursorPosition + selectionLength) == sampleSchema.Length);
      string schema = isFinalChar ? sampleSchema + " " : sampleSchema;
      
      string subString = schema.Substring(cursorPosition, selectionLength);

      if (_tokenizerDef.TryGetFirstMatch(subString, curState.GetValidTransitionTokens(), out MatchResult result))
      {
        results.Add(result);
        
        cursorPosition += selectionLength;
        cursorPosition -= result.cursorFallback;
        selectionLength = 1;

        if (_parseStateMachine.TryTransition(result.tokenType))
        {
          curState = _parseStateMachine.GetCurState();
        }
        else
        {
          return false;
        }
      }
      else
      {
        selectionLength++;
      }
    }

    return true;
  }
  
  private bool SchemaEnumPass(List<MatchResult> tokenResults, ParsedSchemaData parsedSchemaData)
  {
    ParsedEnumData buildingENum = null;

    _parseStateMachine.ResetState();
    while (tokenResults.Count > 0)
    {
      MatchResult curMatch = tokenResults[0];
      tokenResults.RemoveAt(0);

      if (!_parseStateMachine.TryTransition(curMatch.tokenType))
      {
        return false;
      }

      ParseStateData stateData = _parseStateMachine.GetCurState();
      switch (stateData.state)
      {
        case ParseState.ParseStart:
          if (buildingENum != null)
          {
            parsedSchemaData.Enums.Add(buildingENum);
            buildingENum = null;
          }

          break;
        case ParseState.EnumNameDefined:
          buildingENum = new ParsedEnumData();
          buildingENum.Name = curMatch.capturedString;
          break;
        case ParseState.EnumValueDefined:
          buildingENum.ValueNames.Add(curMatch.capturedString);
          break;
      }
    }
    
    return true;
  }
  
  private bool SchemaPass(List<MatchResult> tokenResults, ParsedSchemaData parsedSchemaData)
  {
    ParsedComponentData buildingComponent = null;
    ParsedComponentField buildingComponentField = null;
    ParsedAliasData buildingAlias = null;
    RawAttribute buildingAttribute = null;
    List<RawAttribute> activeAttributes = new List<RawAttribute>();
    
    _parseStateMachine.ResetState();
    while (tokenResults.Count > 0)
    {
      MatchResult curMatch = tokenResults[0];
      tokenResults.RemoveAt(0);

      if (!_parseStateMachine.TryTransition(curMatch.tokenType))
      {
        return false;
      }

      ParseStateData stateData = _parseStateMachine.GetCurState();
      switch (stateData.state)
      {
        case ParseState.ComponentNameDefined:
          buildingComponent = new ParsedComponentData();
          buildingComponent.Name = curMatch.capturedString;
          FillAttrData(buildingComponent, activeAttributes);
          activeAttributes.Clear();
          break;
        case ParseState.ComponentFieldNameDefined:
          buildingComponentField = new ParsedComponentField();
          buildingComponentField.Name = curMatch.capturedString;

          if (buildingAttribute != null)
          {
            activeAttributes.Add(buildingAttribute);
            buildingAttribute = null;
          }
          
          FillAttrData(buildingComponentField, activeAttributes);
          activeAttributes.Clear();
          break;
        case ParseState.ComponentFieldTypeDefined:
          buildingComponentField.FieldType = ParseCompFieldType(curMatch.capturedString, parsedSchemaData);
          buildingComponent.Fields.Add(buildingComponentField);
          if (buildingComponentField.FieldType == ComponentFieldType.Enum)
          {
            buildingComponentField.EnumName = curMatch.capturedString;
          }
          buildingComponentField = null;
          break;
        case ParseState.ParseStart:

          if (buildingComponent != null)
          {
            parsedSchemaData.Components.Add(buildingComponent);
            buildingComponent = null;
          }

          if (buildingAlias != null)
          {
            parsedSchemaData.Aliases.Add(buildingAlias);
            buildingAlias = null;
          }

          if (buildingAttribute != null)
          {
            activeAttributes.Add(buildingAttribute);
            buildingAttribute = null;
          }

          break;
        case ParseState.CompOrAliasAttrNameDefined:
          buildingAttribute = new RawAttribute();
          buildingAttribute.Name = curMatch.capturedString;
          break;
        case ParseState.AttrParamNameDefined:
          buildingAttribute.Params.Add(curMatch.capturedString);
          break;
        case ParseState.AliasNameDefined:
          buildingAlias = new ParsedAliasData();
          buildingAlias.Name = curMatch.capturedString;
          FillAttrData(buildingAlias, activeAttributes);
          activeAttributes.Clear();
          break;
        case ParseState.AliasComponentDefined:
          buildingAlias.ComponentNames.Add(curMatch.capturedString);
          break;
        case ParseState.ComponentFieldAttrNameDefined:
          buildingAttribute = new RawAttribute();
          buildingAttribute.Name = curMatch.capturedString;
          break;
      }
    }
    
    return true;
  }

  private void FillAttrData(ParsedComponentData buildingComponent, List<RawAttribute> attributes)
  {
    foreach (var a in attributes)
    {
      string attrString = a.Name.ToLower();
      switch (attrString)
      {
        case "singleton":
          buildingComponent.IsSingleton = true;
          break;
        case "singleframe":
          buildingComponent.IsSingleFrame = true;
          break;
      } 
    }
  }
  
  private void FillAttrData(ParsedAliasData buildingAlias, List<RawAttribute> attributes)
  {
    foreach (var a in attributes)
    {
      string attrString = a.Name.ToLower();
      switch (attrString)
      {
        case "input":
          buildingAlias.IsInput = true;
          buildingAlias.InputComponentKey = a.Params[0];
          break;
      } 
    }
  }
  
  private void FillAttrData(ParsedComponentField buildingField, List<RawAttribute> attributes)
  {
    foreach (var a in attributes)
    {
      string attrString = a.Name.ToLower();
      switch (attrString)
      {
        case "key":
          buildingField.IsKey = true;
          break;
      } 
    }
  }

  private static ComponentFieldType ParseCompFieldType(string curMatchCapturedString, ParsedSchemaData parsedSchemaData)
  {
    string s = curMatchCapturedString.ToLower();
    switch (s)
    {
      case "int": return ComponentFieldType.Int;
      case "int2": return ComponentFieldType.Int2;
      case "int3": return ComponentFieldType.Int3;
      case "float": return ComponentFieldType.Float;
      case "float2": return ComponentFieldType.Float2;
      case "float3": return ComponentFieldType.Float3;
      case "string": return ComponentFieldType.String;
      case "entityid" : return ComponentFieldType.EntityId;
      case "bool" : return ComponentFieldType.Bool;
      case "fix64vec2": return ComponentFieldType.Fix64Vec2;
      case "fix64vec3": return ComponentFieldType.Fix64Vec3;
      case "fix64": return ComponentFieldType.Fix64;
      
      default:
        if (parsedSchemaData.Enums.Count(data => data.Name == curMatchCapturedString) > 0)
        {
          return ComponentFieldType.Enum;
        }

        break;
    }

    throw new Exception();
  }
}