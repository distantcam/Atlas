using System;
using System.Linq;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

internal static class MapperImplementer
{
    internal static class Mapper
    {
        public static MethodBody Map(ModuleDefinition moduleDefinition, MethodDefinition method, TypeDefinition sourceType, TypeDefinition destinationType)
        {
            var ilHelper = new ILHelper(moduleDefinition);

            var body = new MethodBody(method);

            foreach (var sourceProperty in sourceType.Properties.Where(property => property.GetMethod.IsPublic && property.SetMethod.IsPublic))
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
                    if (sourceProperty.PropertyType != destinationProperty.PropertyType)
                    {
                        Log.Warning("Property '{0}' on type '{1}' did not match on type '{2}'.", sourceProperty.Name, sourceType, destinationType);
                        continue;
                    }

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetProperty(sourceProperty));
                    body.Instructions.Add(ilHelper.SetProperty(destinationProperty));
                }
                if (destinationField != null)
                {
                    if (sourceProperty.PropertyType != destinationField.FieldType)
                    {
                        Log.Warning("Property '{0}' on type '{1}' did not match field on type '{2}'.", sourceProperty.Name, sourceType, destinationType);
                        continue;
                    }

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetProperty(sourceProperty));
                    body.Instructions.Add(ilHelper.SetField(destinationField));
                }
            }

            foreach (var sourceField in sourceType.Fields.Where(field => field.IsPublic))
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
                    if (sourceField.FieldType != destinationField.FieldType)
                    {
                        Log.Warning("Field '{0}' on type '{1}' did not match on type '{2}'.", sourceField.Name, sourceType, destinationType);
                        continue;
                    }

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetField(sourceField));
                    body.Instructions.Add(ilHelper.SetField(destinationField));
                }
                if (destinationProperty != null)
                {
                    if (sourceField.FieldType != destinationProperty.PropertyType)
                    {
                        Log.Warning("Field '{0}' on type '{1}' did not match property on type '{2}'.", sourceField.Name, sourceType, destinationType);
                        continue;
                    }

                    body.Instructions.Add(ilHelper.Load(method.Parameters[1]));
                    body.Instructions.Add(ilHelper.Load(method.Parameters[0]));
                    body.Instructions.Add(ilHelper.GetField(sourceField));
                    body.Instructions.Add(ilHelper.SetProperty(destinationProperty));
                }
            }

            body.Instructions.Add(ilHelper.Return());

            body.OptimizeMacros();

            return body;
        }
    }
}