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
        public static MethodBody Map(ModuleDefinition moduleDefinition, MethodDefinition method, TypeReference sourceType, TypeReference destinationType)
        {
            var ilHelper = new ILHelper(moduleDefinition);

            var body = new MethodBody(method);

            var sourceProperties = sourceType.Resolve().Properties;
            var destinationProperties = destinationType.Resolve().Properties;

            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);
                if (destinationProperty == null)
                {
                    Log.Warning("No matching property '{0}.{1}' on type '{2}'.", sourceType, sourceProperty.Name, destinationType);
                    continue;
                }
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

            body.Instructions.Add(ilHelper.Return());

            body.OptimizeMacros();

            return body;
        }
    }
}