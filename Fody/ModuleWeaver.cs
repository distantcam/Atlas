﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    public string[] DefineConstants { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        LogError = s => { };
        DefineConstants = new string[0];
    }

    public void Execute()
    {
        LoggerFactory.LogInfo = LogInfo;
        LoggerFactory.LogWarn = LogWarning;
        LoggerFactory.LogError = LogError;

        foreach (var type in ModuleDefinition.Types)
        {
            ProcessType(type);
        }

        CleanReferences();
    }

    private void ProcessType(TypeDefinition type)
    {
        foreach (var method in type.Methods.Where(m => m.HasBody))
        {
            foreach (var instruction in method.Body.Instructions)
            {
                try
                {
                    MethodReference mapMethod;
                    if (instruction.IsCall(out mapMethod) && mapMethod.DeclaringType.Namespace == "Atlas")
                    {
                        var localMethod = CreateLocalMapMethod(type, mapMethod);
                        instruction.Operand = localMethod;
                    }
                }
                catch (NonFatalException ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }
    }

    private MethodReference CreateLocalMapMethod(TypeDefinition type, MethodReference originalMethod)
    {
        if (originalMethod.Match("MapTo`1", "From"))
        {
            var destinationType = originalMethod.DeclaringType.GetGenericInstance().GenericArguments[0];
            var sourceType = originalMethod.GetGenericInstance().GenericArguments[0];

            var mapperType = FindOrCreateMapper(type, destinationType);

            return FindOrCreateMapToFromMethod(mapperType, sourceType, destinationType);
        }

        throw new NonFatalException(string.Format("Unknown method on mapper: '{0}'", originalMethod));
    }

    private TypeDefinition FindOrCreateMapper(TypeDefinition type, TypeReference destinationType)
    {
        var mapperType = type.NestedTypes.FirstOrDefault(t => t.Name == string.Format("<{0}>c__Mapper1", destinationType.Name));

        if (mapperType == null)
        {
            Log.Information("\tCreating mapping type for '{0}'.", destinationType);

            mapperType = new TypeDefinition(
                                     "",
                                     string.Format("<{0}>c__Mapper1", destinationType.Name),
                                     TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                                     ModuleDefinition.TypeSystem.Object);
            mapperType.CustomAttributes.Add(new CustomAttribute(ModuleDefinition.Import(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes))));
            type.NestedTypes.Add(mapperType);
        }

        return mapperType;
    }

    private MethodDefinition FindOrCreateMapToFromMethod(TypeDefinition mapperType, TypeReference sourceType, TypeReference destinationType)
    {
        var method = mapperType.Methods.FirstOrDefault(m => m.Name == "To" && m.IsGenericInstance && m.GetGenericInstance().GenericArguments[0] == sourceType);

        if (method == null)
        {
            Log.Information("\tCreating mapping method for '{0}' in mapping type '{1}'.", sourceType, destinationType);

            var ilHelper = new ILHelper(ModuleDefinition);

            method = new MethodDefinition("From" + sourceType.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, destinationType);
            method.Parameters.Add(new ParameterDefinition(sourceType));

            method.Body = new MethodBody(method);
            method.Body.Instructions.Add(ilHelper.New<NotImplementedException>());
            method.Body.Instructions.Add(ilHelper.Throw());

            mapperType.Methods.Add(method);
        }

        return method;
    }

    private void CleanReferences()
    {
        var referenceToRemove = ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Atlas");
        if (referenceToRemove == null)
        {
            Log.Information("\tNo reference to 'Atlas' found. References not modified.");
            return;
        }

        ModuleDefinition.AssemblyReferences.Remove(referenceToRemove);
        Log.Information("\tRemoving reference to 'Atlas'.");
    }
}