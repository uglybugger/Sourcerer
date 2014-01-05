﻿using System;

namespace Sourcerer.Extensions
{
    public static class ClassExtensions
    {
        public static TResult Coalesce<T, TResult>(this T o, Func<T, TResult> extractValue, TResult defaultValue) where T : class
        {
            return o == null
                       ? defaultValue
                       : extractValue(o);
        }
    }
}