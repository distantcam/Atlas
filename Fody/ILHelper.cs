using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class ILHelper
{
    private readonly ModuleDefinition moduleDefinition;

    public ILHelper(ModuleDefinition moduleDefinition)
    {
        if (moduleDefinition == null)
            throw new ArgumentNullException("moduleDefinition", "moduleDefinition is null.");
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
        if (type == null)
            throw new ArgumentNullException("type", "type is null.");
        return Instruction.Create(OpCodes.Newobj, moduleDefinition.Import(type.GetConstructor(Type.EmptyTypes)));
    }

    public Instruction New(TypeReference type)
    {
        if (type == null)
            throw new ArgumentNullException("type", "type is null.");
        return Instruction.Create(OpCodes.Newobj, type.GetConstructor());
    }

    public Instruction Load(VariableDefinition variable)
    {
        if (variable == null)
            throw new ArgumentNullException("variable", "variable is null.");
        return Instruction.Create(OpCodes.Ldloc, variable);
    }

    public Instruction Load(ParameterDefinition parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException("parameter", "parameter is null.");
        return Instruction.Create(OpCodes.Ldarg, parameter);
    }

    public Instruction Store(VariableDefinition variable)
    {
        if (variable == null)
            throw new ArgumentNullException("variable", "variable is null.");
        return Instruction.Create(OpCodes.Stloc, variable);
    }

    public Instruction GetProperty(PropertyDefinition property)
    {
        if (property == null)
            throw new ArgumentNullException("property", "property is null.");
        return Instruction.Create(OpCodes.Callvirt, property.GetMethod);
    }

    public Instruction SetProperty(PropertyDefinition property)
    {
        if (property == null)
            throw new ArgumentNullException("property", "property is null.");
        return Instruction.Create(OpCodes.Callvirt, property.SetMethod);
    }
}