﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Collections.Generic;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    public string[] DefineConstants { get; set; }

    private MapperImplementer.Mapper mapper;

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

        mapper = new MapperImplementer.Mapper(ModuleDefinition);

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
            try
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
                    catch (NonFatalException nonFatal)
                    {
                        Log.Error(nonFatal.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error processing method '{0}' in type '{1}'.{2}{3}", method, type, Environment.NewLine, ex);
                continue;
            }
        }
    }

    private MethodReference CreateLocalMapMethod(TypeDefinition type, MethodReference originalMethod)
    {
        if (originalMethod.Match("Mapper", "Map"))
        {
            var sourceType = originalMethod.GetGenericInstance().GenericArguments[0];
            var destinationType = originalMethod.GetGenericInstance().GenericArguments[1];

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
            Log.Information("Creating mapping type for '{0}'.", destinationType);

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
        var method = mapperType.Methods.FirstOrDefault(m => m.Name == "From" + sourceType.Name);

        if (method == null)
        {
            Log.Information("Creating mapping method for '{0}' in mapping type '{1}'.", sourceType, destinationType);

            method = new MethodDefinition("From" + sourceType.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, ModuleDefinition.TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition(sourceType));
            method.Parameters.Add(new ParameterDefinition(destinationType));

            method.Body = mapper.Map(method, sourceType.Resolve(), destinationType.Resolve());

            mapperType.Methods.Add(method);
        }

        return method;
    }

    private void CleanReferences()
    {
        foreach (var typeDefinition in ModuleDefinition.Types)
        {
            RemoveAtlasAttributes(typeDefinition.CustomAttributes);
            foreach (var field in typeDefinition.Fields)
            {
                RemoveAtlasAttributes(field.CustomAttributes);
            }
            foreach (var property in typeDefinition.Properties)
            {
                RemoveAtlasAttributes(property.CustomAttributes);
            }
        }

        var referenceToRemove = ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Atlas");
        if (referenceToRemove == null)
        {
            Log.Information("No reference to 'Atlas' found. References not modified.");
            return;
        }

        ModuleDefinition.AssemblyReferences.Remove(referenceToRemove);
        Log.Information("Removing reference to 'Atlas'.");
    }

    private void RemoveAtlasAttributes(Collection<CustomAttribute> attributes)
    {
        var atlasAttributes = attributes.Where(x => x.AttributeType.Namespace == "Atlas").ToList();
        foreach (var attribute in atlasAttributes)
        {
            attributes.Remove(attribute);
        }
    }
}