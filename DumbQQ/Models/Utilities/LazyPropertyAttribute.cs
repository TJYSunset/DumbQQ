using System;
using DumbQQ.Helpers;

namespace DumbQQ.Models.Utilities
{
    /// <summary>
    ///     Indicates this property will be ignored by
    ///     <see
    ///         cref="ExtensionMethods.Reassemble{TKey,TValue}" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class LazyPropertyAttribute : Attribute
    {
    }
}