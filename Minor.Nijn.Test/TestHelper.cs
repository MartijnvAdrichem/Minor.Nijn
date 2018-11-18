using System;
using System.Reflection;

namespace Minor.Nijn.Test
{
    internal class TestHelper
    {
        /// <summary>
        /// Gets the value from a private property
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetPrivateProperty<T>(object instance, string propertyName)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(propertyName,
                                                     BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty);
            MethodInfo getter = property.GetGetMethod(nonPublic: true);
            return (T)getter.Invoke(instance, null);
        }

        /// <summary>
        /// Gets the value from a private field
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName,
                                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField);
            return (T)field.GetValue(instance);
        }

        /// <summary>
        /// Invokes a private method and returns the return value
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T InvokeMethod<T>(object instance, string methodName, params object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(methodName,
                                               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return (T)method.Invoke(instance, parameters);
        }
    }
}
