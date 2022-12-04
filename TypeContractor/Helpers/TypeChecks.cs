namespace TypeContractor.Helpers;

internal static class TypeChecks
{
    public static bool IsNullable(Type sourceType) => Nullable.GetUnderlyingType(sourceType) != null;

    public static bool ImplementsIEnumerable(Type sourceType)
    {
        if (sourceType.IsInterface && sourceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return true;

        // We can apparently treat a string as IEnumerable, which we don't want
        if (sourceType == typeof(string))
            return false;

        foreach (Type interfaceUsed in sourceType.GetInterfaces())
            if (interfaceUsed.IsGenericType && interfaceUsed.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return true;

        return false;
    }
}
