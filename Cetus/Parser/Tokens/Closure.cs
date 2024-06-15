﻿using Cetus.Parser.Types;
using Cetus.Parser.Types.Function;
using Cetus.Parser.Values;
using LLVMSharp.Interop;

namespace Cetus.Parser.Tokens;

public class Closure() : TokenSplit(new LiteralToken("{"), new LiteralToken(";"), new LiteralToken("}"), new ParameterExpressionToken("statements")), TypedValue, IHasIdentifiers
{
	public List<FunctionCallContext> Statements;
	public ProgramContext Program { get; set; }
	public IDictionary<string, TypedValue> Identifiers { get; set; }
	public ICollection<TypedTypeFunction> Functions { get; set; }
	public ICollection<TypedType> Types { get; set; }
	public List<TypedTypeFunction>? FinalizedFunctions { get; set; }
	public LLVMBasicBlockRef Block;
	public TypedValue? ReturnValue;
	
	public TypedType Type { get; }
	public LLVMValueRef LLVMValue { get; }
	
	public void Parse(IHasIdentifiers context)
	{
		
	}
	
	public void Transform(IHasIdentifiers context, TypedType? typeHint)
	{
		
	}
	
	public void Visit(IHasIdentifiers context, TypedType? typeHint, Visitor visitor)
	{
		if (typeHint is TypedTypeCompilerClosure)
		{
			LLVMBasicBlockRef originalBlock = visitor.Builder.InsertBlock;
			LLVMBasicBlockRef block = originalBlock.Parent.AppendBasicBlock("closureBlock");
			visitor.Builder.PositionAtEnd(block);
			
			foreach (FunctionCallContext statement in Statements)
				statement.Visit(this, null, visitor);
			
			visitor.Builder.PositionAtEnd(originalBlock);
		}
		else if (typeHint is TypedTypeClosurePointer pointer)
		{
			Dictionary<string, TypedValue> uniqueClosureIdentifiers = ((NestedDictionary<string, TypedValue>)context.Identifiers).ThisDict;
			TypedTypeStruct closureEnvType = new(LLVMTypeRef.CreateStruct(uniqueClosureIdentifiers.Values.Select(type => type.Type.LLVMType).ToArray(), false));
			
			TypedTypeFunction functionType = pointer.BlockType;
			LLVMValueRef function = visitor.Module.AddFunction("closure_block", functionType.LLVMType);
			function.Linkage = LLVMLinkage.LLVMInternalLinkage;
			
			LLVMBasicBlockRef originalBlock = visitor.Builder.InsertBlock;
			visitor.Builder.PositionAtEnd(function.AppendBasicBlock("entry"));
			
			// Unpack the closure environment in the function
			{
				LLVMValueRef closureEnvPtr = function.GetParam(0);
				int paramIndex = 0;
				foreach ((string name, TypedValue value) in uniqueClosureIdentifiers)
				{
					LLVMValueRef elementPtr = visitor.Builder.BuildStructGEP2(closureEnvType.LLVMType, closureEnvPtr, (uint)paramIndex++, name);
					LLVMValueRef element = visitor.Builder.BuildLoad2(value.Type.LLVMType, elementPtr, name);
					Identifiers.Add(name, new TypedValueValue(value.Type, element));
				}
				
				foreach (FunctionCallContext statement in Statements)
					statement.Visit(this, null, visitor);
			}
			
			TypedTypeStruct closureStructType = new(LLVMTypeRef.CreateStruct([LLVMTypeRef.CreatePointer(functionType.LLVMType, 0), LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)], false));
			TypedTypeClosurePointer closureType = new(closureStructType, functionType);
			
			visitor.Builder.PositionAtEnd(originalBlock);
			LLVMValueRef closurePtr = visitor.Builder.BuildAlloca(closureType.Type.LLVMType, "closure");
			
			// Pack the closure for the function
			{
				LLVMValueRef functionPtr = visitor.Builder.BuildStructGEP2(closureType.Type.LLVMType, closurePtr, 0, "function");
				visitor.Builder.BuildStore(function, functionPtr);
				
				LLVMValueRef closureEnvPtr = visitor.Builder.BuildStructGEP2(closureType.Type.LLVMType, closurePtr, 1, "closure_env_ptr");
				LLVMValueRef closureEnv = visitor.Builder.BuildAlloca(closureEnvType.LLVMType, "closure_env");
				int paramIndex = 0;
				foreach ((string name, TypedValue value) in uniqueClosureIdentifiers)
				{
					LLVMValueRef elementPtr = visitor.Builder.BuildStructGEP2(closureEnvType.LLVMType, closureEnv, (uint)paramIndex++, name);
					visitor.Builder.BuildStore(value.LLVMValue, elementPtr);
				}
				LLVMValueRef closureEnvCasted = visitor.Builder.BuildBitCast(closureEnv, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "closure_env_casted");
				visitor.Builder.BuildStore(closureEnvCasted, closureEnvPtr);
			}
		}
		else
			throw new Exception("Expected closure");
	}
}