using UnityEngine;

public enum ColorVariant {
    Default,
    Light,
    Dark,
    Tint,
    Shade,
    Tone,
    SuperTone
}

public static class AppConst {
    public static string squareKey = "BoardSquare";
    public static string pieceKey = "Boardpiece";
    public static string popKey = "VFXPop";
    public const float globalAnimDuration = 0.12f;
    public static Color yellow { get { ColorUtility.TryParseHtmlString("#FDEE78", out Color result); return result; } }
    public static Color green { get { ColorUtility.TryParseHtmlString("#78FDA7", out Color result); return result; } }

    public static Color GetColor(ColorVariant variant, int value = -1) {
        Color result = default(Color);

        switch (variant) {
            case ColorVariant.Default:
                ColorUtility.TryParseHtmlString(value < 0 ? "#FD4556" : "#45b2fd", out result);
                break;

            case ColorVariant.Light:
                ColorUtility.TryParseHtmlString(value < 0 ? "#FFFBF5" : "#f6f5ff", out result);
                break;

            case ColorVariant.Dark:
                ColorUtility.TryParseHtmlString(value < 0 ? "#2C1F20" : "#1f262c", out result);
                break;

            case ColorVariant.Tint:
                ColorUtility.TryParseHtmlString(value < 0 ? "#FD7884" : "#78c6fd", out result);
                break;

            case ColorVariant.Shade:
                ColorUtility.TryParseHtmlString(value < 0 ? "#C93744" : "#378dc9", out result);
                break;

            case ColorVariant.Tone:
                ColorUtility.TryParseHtmlString(value < 0 ? "#CD5f69" : "#5f9fcd", out result);
                break;

            case ColorVariant.SuperTone:
                ColorUtility.TryParseHtmlString(value < 0 ? "#7F6265" : "#62737f", out result);
                break;
        }

        return result;
    }
}