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

    public static bool Match(this TypeReference type, Type matchType)
    {
        return type.Namespace == matchType.Namespace && type.Name == matchType.Name;
    }

    public static MethodDefinition GetConstructor(this TypeReference type)
    {
        return type.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Count == 0);
    }

    public static MethodDefinition GetConstructor(this TypeReference type, params TypeReference[] parameters)
    {
        return type.Resolve().Methods.First(m => m.IsConstructor && m.Parameters.Select(p => p.ParameterType.Resolve()).SequenceEqual(parameters.Select(p => p.Resolve())));
    }

    public static MethodDefinition GetMethod(this TypeReference type, string name)
    {
        return type.Resolve().Methods.First(m => m.Name == name && m.Parameters.Count == 0);
    }

    public static MethodDefinition GetMethod(this TypeReference type, string name, params TypeReference[] parameters)
    {
        return type.Resolve().Methods.First(m => m.Name == name && m.Parameters.Select(p => p.ParameterType.Resolve()).SequenceEqual(parameters.Select(p => p.Resolve())));
    }

    public static PropertyDefinition GetProperty(this TypeReference type, string name)
    {
        return type.Resolve().Properties.First(p => p.Name == name);
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

    public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] args)
    {
        var reference = new MethodReference(
            self.Name,
            self.ReturnType,
            self.DeclaringType.MakeGenericInstanceType(args))
        {
            HasThis = self.HasThis,
            ExplicitThis = self.ExplicitThis,
            CallingConvention = self.CallingConvention
        };

        foreach (var parameter in self.Parameters)
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

        foreach (var genericParam in self.GenericParameters)
            reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));

        return reference;
    }

    public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
    {
        if (self == null)
            throw new ArgumentNullException("self", "self is null.");
        if (arguments == null || arguments.Length == 0)
            throw new ArgumentException("arguments is null or empty.", "arguments");

        GenericInstanceType genericInstanceType = new GenericInstanceType(self);

        foreach (TypeReference typeReference in arguments)
            genericInstanceType.GenericArguments.Add(typeReference);

        return genericInstanceType;
    }
}