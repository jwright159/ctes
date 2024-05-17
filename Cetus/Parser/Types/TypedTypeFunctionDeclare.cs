﻿using Cetus.Parser.Values;
using LLVMSharp.Interop;

namespace Cetus.Parser.Types;

public class TypedTypeFunctionDeclare() : TypedTypeFunction("Declare", Visitor.VoidType, [Visitor.TypeType, Visitor.CompilerStringType], null)
{
	public override TypedValue Call(LLVMBuilderRef builder, TypedValue function, IHasIdentifiers context, params TypedValue[] args)
	{
		TypedType type = args[0].Type;
		string name = ((TypedValueCompilerString)args[1]).StringValue;
		LLVMValueRef variable = builder.BuildAlloca(type.LLVMType, name);
		TypedValue result = new TypedValueValue(new TypedTypePointer(type), variable);
		context.Identifiers.Add(name, result);
		return Visitor.Void;
	}
}