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
}