using System.Text.RegularExpressions;

namespace ecs_gen;

public enum ParseToken
{
  ComponentKeyword,
  AliasKeyword,
  EnumKeyword,
  OpenCurly,
  ClosedCurly,
  OpenBracket,
  ClosedBracket,
  OpenParenthesis,
  ClosedParenthesis,
  AlphaNumericWord,
  Colon,
  SemiColon,
  Comma
}

public class MatchResult
{
  public readonly ParseToken tokenType;
  public readonly string     capturedString;
  public readonly int        cursorFallback;

  public MatchResult(ParseToken tokenType, string capturedString, int cursorFallback)
  {
    this.tokenType = tokenType;
    this.capturedString = capturedString;
    this.cursorFallback = cursorFallback;
  }
}

public class TokenizerDef
{
  private Dictionary<ParseToken, Regex> _tokenDefinitions = new Dictionary<ParseToken, Regex>();

  public TokenizerDef()
  {
    _tokenDefinitions.Add(ParseToken.ComponentKeyword, new Regex(@"(?i)\b(component)\b", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.AliasKeyword, new Regex(@"(?i)\b(alias)\b", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.EnumKeyword, new Regex(@"(?i)\b(enum)\b", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.OpenCurly, new Regex(@"(^|\w|\s)\{($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.ClosedCurly, new Regex(@"(^|\w|\s)\}($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.OpenBracket, new Regex(@"(^|\w|\s)\[($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.ClosedBracket, new Regex(@"(^|\w|\s)\]($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.OpenParenthesis, new Regex(@"(^|\w|\s)\(($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.ClosedParenthesis, new Regex(@"(^|\w|\s)\)($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.AlphaNumericWord, new Regex(@"\b(.+)([^a-zA-Z\d])", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.Colon, new Regex(@"(^|\w|\s):($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.SemiColon, new Regex(@"(^|\w|\s);($|\w|\s)", RegexOptions.Compiled));
    _tokenDefinitions.Add(ParseToken.Comma, new Regex(@"(^|\w|\s),($|\w|\s)", RegexOptions.Compiled));
  }

  public bool TryGetFirstMatch(string s, List<ParseToken> allowedMatches,  out MatchResult result)
  {
    foreach (var token in allowedMatches)
    {
      Match m = _tokenDefinitions[token].Match(s);
      if (!string.IsNullOrEmpty(m.Value))
      {
        int fallback = 0;
        if (m.Groups.Count > 2)
        {
          fallback = m.Groups[2].Length;
        }
        
        result = new MatchResult(token, m.Groups[1].Value, fallback);
        return true;
      }
    }

    result = null;
    return false;
  }
}