﻿using Cetus.Parser.Tokens;
using Cetus.Parser.Values;
using LLVMSharp.Interop;

namespace Cetus.Parser.Types.Function;

public class Define : TypedTypeFunctionSimple
{
	public override string Name => "Define";
	public override IToken Pattern => new TokenString([new ParameterValueToken("type"), new ParameterValueToken("name"), new LiteralToken("="), new ParameterExpressionToken("value")]);
	public override TypeIdentifier ReturnType => new(Visitor.VoidType);
	public override FunctionParameters Parameters => new([(Visitor.TypeType, "type"), (Visitor.CompilerStringType, "name"), (Visitor.IntType, "value")], null);
	public override float Priority => 100;
	
	public override LLVMValueRef Visit(IHasIdentifiers context, TypedType? typeHint, Visitor visitor, FunctionArgs args)
	{
		TypedType type = args["type"].Type;
		string name = ((Tokens.String)args["name"]).Value;
		TypedValue value = args["value"];
		if (!value.IsOfType(type))
			throw new Exception($"Type mismatch in assignment to '{name}', expected {type.LLVMType} but got {value.Type.LLVMType}");
		LLVMValueRef variable = visitor.Builder.BuildAlloca(type.LLVMType, name);
		visitor.Builder.BuildStore(value.LLVMValue, variable);
		TypedValue result = new TypedValueValue(new TypedTypePointer(type), variable);
		context.Identifiers.Add(name, result);
		return variable;
	}
}