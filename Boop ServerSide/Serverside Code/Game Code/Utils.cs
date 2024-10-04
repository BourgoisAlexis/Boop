using System;

public static class Utils {
    public static void Log(string source, string methodName, string message = "") {
        Console.WriteLine($"{source} > {methodName} > {message}");
    }

    public static void Log(object source, string methodName, string message = "") {
        Log(source.GetType().ToString(), methodName, message);
    }

    public static void LogError(string source, string methodName, string message = "") {
        Console.Error.WriteLine($"{source} > {methodName} > {message}");
    }

    public static void LogError(object source, string methodName, string message = "") {
        LogError(source.GetType().ToString(), methodName, message);
    }
}