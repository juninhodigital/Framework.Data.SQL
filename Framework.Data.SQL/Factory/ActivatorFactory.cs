using System;

namespace Framework.Data.SQL
{
    public static class ActivatorFactory
    {
        #region| Methods |

        /// <summary>
        /// Creates an instance of the type designated by the specified generic type parameter, using the parameterless constructor
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <returns>A reference to the newly created object</returns>
        public static T CreateInstance<T>() where T : new()
        {
            return ActivatorImpl<T>.Create();
        }
        
        /// <summary>
        /// Implments the object instance creation using Lambda compiler expression faster the the Activator.CreateInstance<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class ActivatorImpl<T> where T : new()
        {
            public static readonly Func<T> Create = DynamicModuleLambdaCompiler.GenerateFactory<T>();
        } 

        #endregion
    }
}
