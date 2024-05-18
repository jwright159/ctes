﻿namespace Cetus.Parser.Tokens;

public class LiteralToken(string token) : IToken
{
	public bool Eat(string contents, ref int index)
	{
		if (contents[index..].StartsWith(TokenText!))
		{
			index += TokenText!.Length;
			return true;
		}
		
		return false;
	}
	
	public string? TokenText { get; set; } = token;
}

public class LeftBrace() : LiteralToken("{");

public class RightBrace() : LiteralToken("}");

public class LeftParenthesis() : LiteralToken("(");

public class RightParenthesis() : LiteralToken(")");

public class LeftTriangle() : LiteralToken("<");

public class RightTriangle() : LiteralToken(">");

public class Comma() : LiteralToken(",");

public class Delegate() : LiteralToken("delegate");

public class Null() : LiteralToken("null");

public class Include() : LiteralToken("include");

public class Extern() : LiteralToken("extern");

public class Ellipsis() : LiteralToken("...");

public class Pointer() : LiteralToken("*");

public class Semicolon() : LiteralToken(";");