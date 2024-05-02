﻿using System.Diagnostics.CodeAnalysis;

namespace Cetus.Tokens;

public class Double : IToken
{
	public static bool Split(string contents, ref int index, [NotNullWhen(true)] out string? token)
	{
		if (char.IsDigit(contents[index]))
		{
			int i = index;
			bool dot = false;
			while (i < contents.Length && (char.IsDigit(contents[i]) || (!dot ^ (dot |= contents[i] == '.')))) i++;
			token = contents[index..i];
			index = i;
			return true;
		}
		else
		{
			token = null;
			return false;
		}
	}
		
	public string TokenText { get; init; } = null!;
}