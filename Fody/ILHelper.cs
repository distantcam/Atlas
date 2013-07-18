using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class ILHelper
{
    private readonly ModuleDefinition moduleDefinition;

    public ILHelper(ModuleDefinition moduleDefinition)
    {
        this.moduleDefinition = moduleDefinition;
    }

    public Instruction Throw()
    {
        return Instruction.Create(OpCodes.Throw);
    }

    public Instruction Return()
    {
        return Instruction.Create(OpCodes.Ret);
    }

    public Instruction New<T>()
    {
        return New(typeof(T));
    }

    public Instruction New(Type type)
    {
        return Instruction.Create(OpCodes.Newobj, moduleDefinition.Import(type.GetConstructor(Type.EmptyTypes)));
    }
}