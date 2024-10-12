using PlayerIO.GameLibrary;
using System.Collections.Generic;
using System.Text;

public static class CommonUtils {
    public static void ErrorOnParams(string source, string methodName) {
        Utils.LogError(source, methodName, "not the expected parameters");
    }

    public static void ErrorOnResult(string source, string methodName) {
        Utils.LogError(source, methodName, "not the expected result");
    }

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

    public static string BoardState(int[,] board) {
        StringBuilder sb = new StringBuilder();
        int boardSize = board.GetLength(0);

        for (int x = 0; x < boardSize; x++)
            for (int y = 0; y < boardSize; y++) {
                sb.Append(board[x, y]);
                if (x < boardSize - 1 || y < boardSize - 1)
                    sb.Append(";");
            }

        return sb.ToString();
    }

    public static int[,] BoardState(string board, int boardSize) {
        string[] squares = board.Split(';');
        int[,] result = new int[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
            for (int y = 0; y < boardSize; y++)
                result[x, y] = int.Parse(squares[(x * boardSize) + y]);

        return result;
    }
}
