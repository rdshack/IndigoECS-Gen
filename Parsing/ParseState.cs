namespace ecs_gen;

public enum ParseState
{
  ParseStart,
  CompOrAliasAttrStart,
  CompOrAliasAttrNameDefined,
  ComponentKeywordDeclared,
  ComponentNameDefined,
  ComponentCurlyOpened,
  ComponentFieldNameDefined,
  ComponentFieldSeperatorSet,
  ComponentFieldTypeDefined,
  ComponentFieldAttrStart,
  ComponentFieldAttrNameDefined,
  AliasKeywordDeclared,
  AliasNameDefined,
  AliasReadyForComponent,
  AliasComponentDefined,
  EnumKeywordDeclared,
  EnumNameDefined,
  EnumReadyForValue,
  EnumValueDefined,
  AttrParamStart,
  AttrParamNameDefined,
  AttrParamEnd
}