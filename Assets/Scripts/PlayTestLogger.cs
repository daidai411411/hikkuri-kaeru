using System;
using System.IO;
using System.Text;
using UnityEngine;

public class PlayTestLogger : MonoBehaviour
{
    private static string FilePath =>
        Path.Combine(
            Application.dataPath,
            "PlayTestResults.csv"
        );

    public static void OutputLog(
        string stageName,
        int clearTime,
        int flipCount,
        int score
        )
    {
        try
        {
            // ファイルがなければ、ヘッダーを作成する
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(
                    FilePath,
                    "DateTime,Stage,ClearTimeSeconds,FlipCount,Score\n",
                    new UTF8Encoding(true)
                );
            }

            string line = string.Join(",",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                stageName,
                (clearTime / 100f).ToString("F2"),
                flipCount.ToString(),
                score.ToString()
            );

            // 既存ファイルの末尾に1行追加する
            File.AppendAllText(
                FilePath,
                line + "\n",
                new UTF8Encoding(false)
            );

            Debug.Log($"[PlayTest] CSV saved: {FilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayTest] CSV write failed: {e}");
        }
    }

}
