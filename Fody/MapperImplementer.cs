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

            foreach (var sourceProperty in sourceType.Properties.Where(property => property.GetMethod.IsPublicInstance()))
            {
                if (sourceProperty.HasAttribute("Atlas.IgnoreMapAttribute"))
                    continue;

                MapMember(method, body, sourceType, destinationType, sourceProperty, sourceProperty.PropertyType, ilHelper.GetProperty(sourceProperty), "property");
            }

            foreach (var sourceField in sourceType.Fields.Where(field => field.IsPublicInstance()))
            {
                if (sourceField.HasAttribute("Atlas.IgnoreMapAttribute"))
                    continue;

                MapMember(method, body, sourceType, destinationType, sourceField, sourceField.FieldType, ilHelper.GetField(sourceField), "field");
            }

            body.Instructions.Add(ilHelper.Return());

            if (body.Variables.Any())
                body.InitLocals = true;

            body.OptimizeMacros();

            return body;
        }

        private void MapMember(
            MethodDefinition method,
            MethodBody body,
            TypeDefinition sourceType,
            TypeDefinition destinationType,
            MemberReference sourceMember,
            TypeReference sourceMemberType,
            Instruction getSourceMemberInstruction,
            string sourceMemberLogName)
        {
            var destinationProperty = destinationType.Properties.FirstOrDefault(p => p.Name == sourceMember.Name && p.SetMethod.IsPublicInstance());
            var destinationField = destinationType.Fields.FirstOrDefault(f => f.Name == sourceMember.Name && f.IsPublicInstance());
            if (destinationProperty == null && destinationField == null)
            {
                Log.Warning("No matching property or field '{0}.{1}' on type '{2}'.", sourceType, sourceMember.Name, destinationType);
                return;
            }

            VariableDefinition tempVar = null;
            IEnumerable<Instruction> conversionInstructions = null;

            if (destinationProperty != null)
                conversionInstructions = GetConversion(sourceMemberType, destinationProperty.PropertyType, out tempVar);
            if (destinationField != null)
                conversionInstructions = GetConversion(sourceMemberType, destinationField.FieldType, out tempVar);

            if (conversionInstructions == null)
            {
                Log.Warning("Cannot convert from type '{0}' to type '{1}' for {3} '{2}'.", sourceType, destinationType, sourceMember.Name, sourceMemberLogName);
                return;
            }
            if (tempVar != null)
                body.Variables.Add(tempVar);

            body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
            body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
            body.Instructions.Add(getSourceMemberInstruction);
            foreach (var instruction in conversionInstructions)
                body.Instructions.Add(instruction);

            if (destinationProperty != null)
                body.Instructions.Add(ilHelper.SetProperty(destinationProperty));
            if (destinationField != null)
                body.Instructions.Add(ilHelper.SetField(destinationField));
        }

        private IEnumerable<Instruction> GetConversion(TypeReference sourceType, TypeReference destinationType, out VariableDefinition tempVar)
        {
            tempVar = null;

            if (sourceType == destinationType)
                return new Instruction[0];

            if (destinationType == moduleDefinition.TypeSystem.String)
            {
                var result = new List<Instruction>();

                if (sourceType.IsValueType)
                {
                    tempVar = new VariableDefinition(sourceType);
                    result.Add(ilHelper.Store(tempVar));
                    result.Add(ilHelper.LoadAddress(tempVar));
                }

                result.Add(ilHelper.Call(sourceType.GetMethod("ToString")));
                return result;
            }

            var convertType = moduleDefinition.Import(typeof(Convert));

            if (sourceType == moduleDefinition.TypeSystem.Boolean ||
                sourceType == moduleDefinition.TypeSystem.Byte ||
                sourceType == moduleDefinition.TypeSystem.Char ||
                sourceType == moduleDefinition.TypeSystem.Double ||
                sourceType == moduleDefinition.TypeSystem.Int16 ||
                sourceType == moduleDefinition.TypeSystem.Int32 ||
                sourceType == moduleDefinition.TypeSystem.Int64 ||
                sourceType == moduleDefinition.TypeSystem.SByte ||
                sourceType == moduleDefinition.TypeSystem.Single ||
                sourceType == moduleDefinition.TypeSystem.UInt16 ||
                sourceType == moduleDefinition.TypeSystem.UInt32 ||
                sourceType == moduleDefinition.TypeSystem.UInt64)
            {
                if (destinationType == moduleDefinition.TypeSystem.Boolean)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToBoolean", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Byte)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToByte", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Char)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToChar", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Double)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToDouble", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Int16)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToInt16", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Int32)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToInt32", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Int64)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToInt64", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.SByte)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToSByte", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.Single)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToSingle", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.UInt16)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToUInt16", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.UInt32)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToUInt32", sourceType)) };

                if (destinationType == moduleDefinition.TypeSystem.UInt64)
                    return new Instruction[] { ilHelper.Call(convertType.GetMethod("ToUInt64", sourceType)) };
            }

            if (destinationType.Match(typeof(Nullable<>)))
            {
                var genericDestinationType = destinationType.GetGenericInstance();
                var nullType = genericDestinationType.GenericArguments[0];
                if (sourceType == nullType)
                {
                    var ctor = destinationType.Resolve().Methods.First(m => m.IsConstructor);

                    return new Instruction[] { ilHelper.New(ctor.MakeHostInstanceGeneric(sourceType)) };
                }
            }

            return null;
        }
    }
}