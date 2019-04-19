using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace Reusable
{
    public static class ObjectFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> TypeCache = new ConcurrentDictionary<Type, Type>();

        public static T CreateInstance<T>(Action<T> initializeAction = null)
        {
            if (!typeof(T).IsInterface) throw new ArgumentException($"Type {typeof(T).Name} must be an interface.");
            var newType = TypeCache.GetOrAdd(typeof(T), t => BuildType(typeof(T)));
            var instance = (T)Activator.CreateInstance(newType);
            initializeAction?.Invoke(instance);
            return instance;
        }

        private static Type BuildType(Type interfaceType)
        {
            var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
#if NET47
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
#if NETCOREAPP2_2
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var typeName = $"{RemoveInterfacePrefix(interfaceType.Name)}_{Guid.NewGuid():N}";
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            typeBuilder.AddInterfaceImplementation(interfaceType);

            var properties = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            BuildProperties(typeBuilder, properties);

            return typeBuilder.CreateType();

            string RemoveInterfacePrefix(string name) => Regex.Replace(name, "^I", string.Empty);
        }

        private static void BuildProperties(TypeBuilder typeBuilder, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                BuildProperty(typeBuilder, property);
            }
        }

        private static PropertyBuilder BuildProperty(TypeBuilder typeBuilder, PropertyInfo property)
        {
            var fieldName = $"<{property.Name}>k__BackingField";

            var propertyBuilder = typeBuilder.DefineProperty(property.Name, System.Reflection.PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);

            // Build backing-field.
            var fieldBuilder = typeBuilder.DefineField(fieldName, property.PropertyType, FieldAttributes.Private);

            var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            var getterBuilder = BuildGetter(typeBuilder, property, fieldBuilder, getSetAttributes);
            var setterBuilder = BuildSetter(typeBuilder, property, fieldBuilder, getSetAttributes);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);

            return propertyBuilder;
        }

        private static MethodBuilder BuildGetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
        {
            var getterBuilder = typeBuilder.DefineMethod($"get_{property.Name}", attributes, property.PropertyType, Type.EmptyTypes);
            var ilGenerator = getterBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);

            if (property.IsDefined(typeof(RequiredAttribute)))
            {
                // Build null check
                ilGenerator.Emit(OpCodes.Dup);

                var isFieldNull = ilGenerator.DefineLabel();
                ilGenerator.Emit(OpCodes.Brtrue_S, isFieldNull);
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ldstr, $"{property.Name} isn't set.");

                var invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });
                ilGenerator.Emit(OpCodes.Newobj, invalidOperationExceptionConstructor);
                ilGenerator.Emit(OpCodes.Throw);

                ilGenerator.MarkLabel(isFieldNull);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return getterBuilder;
        }

        private static MethodBuilder BuildSetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
        {
            var setterBuilder = typeBuilder.DefineMethod($"set_{property.Name}", attributes, null, new Type[] { property.PropertyType });
            var ilGenerator = setterBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);

            // Build null check

            if (property.IsDefined(typeof(RequiredAttribute)))
            {
                var isValueNull = ilGenerator.DefineLabel();

                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.Emit(OpCodes.Brtrue_S, isValueNull);
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ldstr, property.Name);

                var argumentNullExceptionConstructor = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) });
                ilGenerator.Emit(OpCodes.Newobj, argumentNullExceptionConstructor);
                ilGenerator.Emit(OpCodes.Throw);

                ilGenerator.MarkLabel(isValueNull);
            }

            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            return setterBuilder;
        }
    }
}