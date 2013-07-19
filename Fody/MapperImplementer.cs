using System;
using System.Collections.Generic;
using System.Linq;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

internal class MapperImplementer
{
    internal class Mapper
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ILHelper ilHelper;

        public Mapper(ModuleDefinition moduleDefinition)
        {
            this.moduleDefinition = moduleDefinition;
            ilHelper = new ILHelper(moduleDefinition);
        }

        public MethodBody Map(MethodDefinition method, TypeDefinition sourceType, TypeDefinition destinationType)
        {
            var body = new MethodBody(method);

            foreach (var sourceProperty in sourceType.Properties.Where(property => property.GetMethod.IsPublicInstance() && property.SetMethod.IsPublicInstance()))
            {
                if (sourceProperty.HasAttribute("Atlas.IgnoreMapAttribute"))
                    continue;

                var destinationProperty = destinationType.Properties.FirstOrDefault(p => p.Name == sourceProperty.Name);
                var destinationField = destinationType.Fields.FirstOrDefault(p => p.Name == sourceProperty.Name);
                if (destinationProperty == null && destinationField == null)
                {
                    Log.Warning("No matching property or field '{0}.{1}' on type '{2}'.", sourceType, sourceProperty.Name, destinationType);
                    continue;
                }
                if (destinationProperty != null)
                {
                    VariableDefinition tempVar;
                    var conversionInstructions = GetConversion(sourceProperty.PropertyType, destinationProperty.PropertyType, out tempVar);
                    if (conversionInstructions == null)
                    {
                        Log.Warning("Cannot convert from type '{0}' to type '{1}' for property '{2}'.", sourceType, destinationType, sourceProperty);
                        continue;
                    }
                    if (tempVar != null)
                        body.Variables.Add(tempVar);

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetProperty(sourceProperty));
                    foreach (var instruction in conversionInstructions)
                        body.Instructions.Add(instruction);
                    body.Instructions.Add(ilHelper.SetProperty(destinationProperty));
                }
                if (destinationField != null)
                {
                    VariableDefinition tempVar;
                    var conversionInstructions = GetConversion(sourceProperty.PropertyType, destinationField.FieldType, out tempVar);
                    if (conversionInstructions == null)
                    {
                        Log.Warning("Cannot convert from type '{0}' to type '{1}' for property '{2}'.", sourceType, destinationType, sourceProperty);
                        continue;
                    }
                    if (tempVar != null)
                        body.Variables.Add(tempVar);

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetProperty(sourceProperty));
                    foreach (var instruction in conversionInstructions)
                        body.Instructions.Add(instruction);
                    body.Instructions.Add(ilHelper.SetField(destinationField));
                }
            }

            foreach (var sourceField in sourceType.Fields.Where(field => field.IsPublicInstance()))
            {
                if (sourceField.HasAttribute("Atlas.IgnoreMapAttribute"))
                    continue;

                var destinationProperty = destinationType.Properties.FirstOrDefault(p => p.Name == sourceField.Name);
                var destinationField = destinationType.Fields.FirstOrDefault(p => p.Name == sourceField.Name);
                if (destinationProperty == null && destinationField == null)
                {
                    Log.Warning("No matching property or field '{0}.{1}' on type '{2}'.", sourceType, sourceField.Name, destinationType);
                    continue;
                }
                if (destinationField != null)
                {
                    VariableDefinition tempVar;
                    var conversionInstructions = GetConversion(sourceField.FieldType, destinationField.FieldType, out tempVar);
                    if (conversionInstructions == null)
                    {
                        Log.Warning("Cannot convert from type '{0}' to type '{1}' for field '{2}'.", sourceType, destinationType, sourceField);
                        continue;
                    }
                    if (tempVar != null)
                        body.Variables.Add(tempVar);

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetField(sourceField));
                    foreach (var instruction in conversionInstructions)
                        body.Instructions.Add(instruction);
                    body.Instructions.Add(ilHelper.SetField(destinationField));
                }
                if (destinationProperty != null)
                {
                    VariableDefinition tempVar;
                    var conversionInstructions = GetConversion(sourceField.FieldType, destinationProperty.PropertyType, out tempVar);
                    if (conversionInstructions == null)
                    {
                        Log.Warning("Cannot convert from type '{0}' to type '{1}' for field '{2}'.", sourceType, destinationType, sourceField);
                        continue;
                    }
                    if (tempVar != null)
                        body.Variables.Add(tempVar);

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetField(sourceField));
                    foreach (var instruction in conversionInstructions)
                        body.Instructions.Add(instruction);
                    body.Instructions.Add(ilHelper.SetProperty(destinationProperty));
                }
            }

            body.Instructions.Add(ilHelper.Return());

            if (body.Variables.Any())
                body.InitLocals = true;

            body.OptimizeMacros();

            return body;
        }

        public IEnumerable<Instruction> GetConversion(TypeReference sourceType, TypeReference destinationType, out VariableDefinition tempVar)
        {
            tempVar = null;

            if (sourceType == destinationType)
                return new Instruction[0];

            if (destinationType == moduleDefinition.TypeSystem.String)
            {
                var toStringMethod = moduleDefinition.Import(sourceType.GetMethod("ToString"));

                var result = new List<Instruction>();

                if (sourceType.IsValueType)
                {
                    tempVar = new VariableDefinition(sourceType);
                    result.Add(ilHelper.Store(tempVar));
                    result.Add(ilHelper.LoadAddress(tempVar));
                }

                result.Add(ilHelper.Call(toStringMethod));
                return result;
            }

            return null;
        }
    }
}