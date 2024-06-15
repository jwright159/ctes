﻿using Cetus.Parser.Tokens;
using LLVMSharp.Interop;

namespace Cetus.Parser.Types.Function;

public class Assign : TypedTypeFunctionSimple
{
	public override string Name => "Assign";
	public override IToken Pattern => new TokenString([new ParameterExpressionToken("target"), new LiteralToken("="), new ParameterExpressionToken("value")]);
	public override TypeIdentifier ReturnType => new(Visitor.VoidType);
	public override FunctionParameters Parameters => new([(Visitor.IntType.Pointer(), "target"), (Visitor.IntType, "value")], null);
	public override float Priority => 100;
	
	public override LLVMValueRef Visit(IHasIdentifiers context, TypedType? typeHint, Visitor visitor, FunctionArgs args)
	{
		return visitor.Builder.BuildStore(args["value"].LLVMValue, args["target"].LLVMValue);
	}
}