﻿using System;

namespace Sourcerer.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsInstantiable(this Type type)
        {
            if (type.IsInterface) return false;
            if (type.IsAbstract) return false;
            if (type.ContainsGenericParameters) return false;
            return true;
        }

        public static bool IsAssignableTo<TTarget>(this Type type)
        {
            return typeof (TTarget).IsAssignableFrom(type);
        }
    }
}