using System.Text.RegularExpressions;

namespace ecs_gen;


public class ParseStateData
{
  public readonly ParseState state;

  private Dictionary<ParseToken, ParseStateData> _transitions;

  public ParseStateData(ParseState state)
  {
    this.state = state;
    _transitions = new Dictionary<ParseToken, ParseStateData>();
  }

  public void AddTransition(ParseToken trigger, ParseStateData toState)
  {
    _transitions.Add(trigger, toState);
  }

  public bool TryGetTransition(ParseToken trigger, out ParseStateData toState)
  {
    return _transitions.TryGetValue(trigger, out toState);
  }

  public List<ParseToken> GetValidTransitionTokens()
  {
    return new List<ParseToken>(_transitions.Keys);
  }
}

public class ParseStateMachine
{
  private ParseStateData _curState;
  private ParseStateData _startState;

  public ParseStateMachine()
  {
    //define all states
    _startState = new ParseStateData(ParseState.ParseStart);
    ParseStateData compAttrStart = new ParseStateData(ParseState.CompOrAliasAttrStart);
    ParseStateData componentKeywordDeclared = new ParseStateData(ParseState.ComponentKeywordDeclared);
    ParseStateData componentNameDefined = new ParseStateData(ParseState.ComponentNameDefined);
    ParseStateData compAttrNameDefined = new ParseStateData(ParseState.CompOrAliasAttrNameDefined);
    
    ParseStateData attrParamStart = new ParseStateData(ParseState.AttrParamStart);
    ParseStateData attrParamDefined = new ParseStateData(ParseState.AttrParamNameDefined);
    ParseStateData attrParamEnd = new ParseStateData(ParseState.AttrParamEnd);
    
    ParseStateData componentOpen = new ParseStateData(ParseState.ComponentCurlyOpened);
    ParseStateData componentFieldNameDefined = new ParseStateData(ParseState.ComponentFieldNameDefined);
    ParseStateData componentFieldSeperatorSet = new ParseStateData(ParseState.ComponentFieldSeperatorSet);
    ParseStateData componentFieldTypeDefined = new ParseStateData(ParseState.ComponentFieldTypeDefined);

    ParseStateData compFieldAttrStart = new ParseStateData(ParseState.ComponentFieldAttrStart);
    ParseStateData compFieldAttrNameDefined = new ParseStateData(ParseState.ComponentFieldAttrNameDefined);
    
    ParseStateData aliasKeywordDeclared = new ParseStateData(ParseState.AliasKeywordDeclared);
    ParseStateData aliasNameDefined = new ParseStateData(ParseState.AliasNameDefined);
    ParseStateData aliasReadyForComponent = new ParseStateData(ParseState.AliasReadyForComponent);
    ParseStateData alieasComponentDefined = new ParseStateData(ParseState.AliasComponentDefined);
    
    ParseStateData enumKeywordDeclared = new ParseStateData(ParseState.EnumKeywordDeclared);
    ParseStateData enumNameDefined = new ParseStateData(ParseState.EnumNameDefined);
    ParseStateData enumReadyForValue = new ParseStateData(ParseState.EnumReadyForValue);
    ParseStateData enumValueDefined = new ParseStateData(ParseState.EnumValueDefined);

    //setup transitions
    _startState.AddTransition(ParseToken.ComponentKeyword, componentKeywordDeclared);
    _startState.AddTransition(ParseToken.AliasKeyword, aliasKeywordDeclared);
    _startState.AddTransition(ParseToken.EnumKeyword, enumKeywordDeclared);
    _startState.AddTransition(ParseToken.OpenBracket, compAttrStart);
    
    compAttrStart.AddTransition(ParseToken.AlphaNumericWord, compAttrNameDefined);
    compAttrNameDefined.AddTransition(ParseToken.OpenParenthesis, attrParamStart);
    attrParamStart.AddTransition(ParseToken.AlphaNumericWord, attrParamDefined);
    attrParamDefined.AddTransition(ParseToken.ClosedParenthesis, attrParamEnd);
    attrParamEnd.AddTransition(ParseToken.ClosedBracket, _startState);
    compAttrNameDefined.AddTransition(ParseToken.ClosedBracket, _startState);

    componentKeywordDeclared.AddTransition(ParseToken.AlphaNumericWord, componentNameDefined);
    componentNameDefined.AddTransition(ParseToken.OpenCurly, componentOpen);
    
    componentOpen.AddTransition(ParseToken.ClosedCurly, _startState);
    componentOpen.AddTransition(ParseToken.OpenBracket, compFieldAttrStart);
    componentOpen.AddTransition(ParseToken.AlphaNumericWord, componentFieldNameDefined);
    
    compFieldAttrStart.AddTransition(ParseToken.AlphaNumericWord, compFieldAttrNameDefined);
    compFieldAttrNameDefined.AddTransition(ParseToken.ClosedBracket, componentOpen);
    
    componentFieldNameDefined.AddTransition(ParseToken.Colon, componentFieldSeperatorSet);
    componentFieldSeperatorSet.AddTransition(ParseToken.AlphaNumericWord, componentFieldTypeDefined);
    componentFieldTypeDefined.AddTransition(ParseToken.SemiColon, componentOpen);

    aliasKeywordDeclared.AddTransition(ParseToken.AlphaNumericWord, aliasNameDefined);
    aliasNameDefined.AddTransition(ParseToken.OpenCurly, aliasReadyForComponent);
    aliasReadyForComponent.AddTransition(ParseToken.AlphaNumericWord, alieasComponentDefined);
    alieasComponentDefined.AddTransition(ParseToken.Comma, aliasReadyForComponent);
    alieasComponentDefined.AddTransition(ParseToken.ClosedCurly, _startState);
    
    enumKeywordDeclared.AddTransition(ParseToken.AlphaNumericWord, enumNameDefined);
    enumNameDefined.AddTransition(ParseToken.OpenCurly, enumReadyForValue);
    enumReadyForValue.AddTransition(ParseToken.AlphaNumericWord, enumValueDefined);
    enumValueDefined.AddTransition(ParseToken.Comma, enumReadyForValue);
    enumValueDefined.AddTransition(ParseToken.ClosedCurly, _startState);

    _curState = _startState;
  }

  public void ResetState()
  {
    _curState = _startState;
  }

  public ParseStateData GetCurState()
  {
    return _curState;
  }

  public bool TryTransition(ParseToken tokenTrigger)
  {
    if (_curState.TryGetTransition(tokenTrigger, out ParseStateData newState))
    {
      _curState = newState;
      return true;
    }

    return false;
  }
}