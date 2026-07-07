using System.Diagnostics;

namespace QCommon.Internal.Logging;

internal static class LoggerUtil
{
    public static string GetCategoryName<T>() => TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: true, includeGenericParameterNames: false/*, nestedTypeDelimiter: '.'*/);
    public static string GetCategoryName<T>(string? suffix) => suffix is null ? GetCategoryName<T>() : GetCategoryNameWithSuffix<T>(suffix);
    private static string GetCategoryNameWithSuffix<T>(string suffix) => $"{GetCategoryName<T>()}.{suffix}";
}