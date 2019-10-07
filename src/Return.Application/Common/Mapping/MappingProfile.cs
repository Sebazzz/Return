// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MappingProfile.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            List<Type> types = assembly.GetExportedTypes().
                Where(predicate: t => t.GetInterfaces().
                    Any(predicate: i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>))).
                ToList();

            foreach (Type type in types)
            {
                object? instance = Activator.CreateInstance(type: type);
                MethodInfo? methodInfo = type.GetMethod(name: "Mapping");
                methodInfo?.Invoke(obj: instance, new object[] {this});
            }
        }
    }
}
