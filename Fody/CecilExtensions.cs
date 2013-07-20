using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CecilExtensions
{
    public static bool IsCall(this Instruction instruction, out MethodReference method)
    {
        if (instruction.OpCode == OpCodes.Call)
        {
            method = (MethodReference)instruction.Operand;
            return true;
        }
        else
        {
            method = null;
            return false;
        }
    }

    public static GenericInstanceMethod GetGenericInstance(this MethodReference method)
    {
        return (GenericInstanceMethod)method;
    }

    public static GenericInstanceType GetGenericInstance(this TypeReference type)
    {
        return (GenericInstanceType)type;
    }

    public static bool Match(this MemberReference method, string typeName, string methodName)
    {
        return method.DeclaringType.Name == typeName && method.Name == methodName;
    }

    public static MethodDefinition GetConstructor(this TypeReference type)
    {
        return type.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 0);
    }

    public static MethodDefinition GetMethod(this TypeReference type, string name)
    {
        return type.Resolve().Methods.First(m => m.Name == name && m.Parameters.Count == 0);
    }

    public static MethodDefinition GetMethod(this TypeReference type, string name, params TypeReference[] parameters)
    {
        return type.Resolve().Methods.First(m => m.Name == name && m.Parameters.Select(p => p.ParameterType.Resolve()).SequenceEqual(parameters.Select(p => p.Resolve())));
    }

    public static bool HasAttribute(this PropertyDefinition property, string name)
    {
        return property.CustomAttributes.Any(attr => attr.AttributeType.FullName == name);
    }

    public static bool HasAttribute(this FieldDefinition field, string name)
    {
        return field.CustomAttributes.Any(attr => attr.AttributeType.FullName == name);
    }

    public static bool IsPublicInstance(this MethodDefinition method)
    {
        return method.IsPublic && !method.IsStatic;
    }

    public static bool IsPublicInstance(this FieldDefinition field)
    {
        return field.IsPublic && !field.IsStatic;
    }
}