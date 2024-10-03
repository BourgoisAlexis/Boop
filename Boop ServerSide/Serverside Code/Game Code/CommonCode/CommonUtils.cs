using PlayerIO.GameLibrary;
using System.Collections.Generic;
using System.Text;

public static class CommonUtils {
    public static string[] GetMessageParams(Message m) {
        List<string> infos = new List<string>();
        for (int i = 0; i < m.Count; i++)
            infos.Add(m[(uint)i].ToString());

        return infos.ToArray();
    }

    public static void LogMessage(Message m) {
        StringBuilder b = new StringBuilder();
        b.Append($"{m.Type} > ");
        string[] infos = GetMessageParams(m);
        foreach (string info in infos)
            b.AppendLine(info);

        Utils.Log("CommonUtils", "LogMessage", b.ToString());
    }
}
