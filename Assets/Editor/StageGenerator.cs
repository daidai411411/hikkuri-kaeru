using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// ステージ設計シートから書き出した CSV（Assets/StageData/{シーン名}.csv）を読み、
/// 開いているステージシーンの中身を作り直すエディタ拡張。
/// 前提：シーンに Stage1 と同じ構成（Stage / Retry / Border の各 Tilemap）があること。
/// </summary>
public static class StageGenerator
{
    private const string StageDataFolder = "Assets/StageData";
    private const string UndoName = "ステージ生成";

    [MenuItem("Tools/ステージ生成")]
    private static void GenerateForActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        string csvPath = $"{StageDataFolder}/{scene.name}.csv";
        if (!File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog(UndoName, $"{csvPath} が見つかりません。\nシーン名と同じ名前の CSV を置いてください。", "OK");
            return;
        }

        StageTilemaps tilemaps = StageTilemaps.Find(scene);
        if (tilemaps == null)
        {
            EditorUtility.DisplayDialog(UndoName, "Stage Tilemap / Retry Tilemap / Border Tilemap が見つかりません。\nStage1 を複製したシーンで実行してください。", "OK");
            return;
        }

        StageLayout layout = StageLayout.Load(csvPath);
        StageTilePainter.Paint(layout, tilemaps, UndoName);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[{UndoName}] {csvPath} からタイルを塗りました（{layout.Cells.Count()} マス）");
    }
}

/// <summary>
/// CSV に書かれたステージの地図。セル座標は Unity のワールド座標と同じ（セル x,y はワールドの x〜x+1, y〜y+1）。
/// CSV の形式：1行目が x の見出し、1列目が y の見出し、それ以外のセルが記号1文字（空欄は何もない）。
/// </summary>
public class StageLayout
{
    public const char Empty = '\0';

    private readonly Dictionary<Vector2Int, char> cells = new Dictionary<Vector2Int, char>();

    public IEnumerable<KeyValuePair<Vector2Int, char>> Cells => cells;

    public char Get(Vector2Int cell) => cells.TryGetValue(cell, out char symbol) ? symbol : Empty;

    public char Get(int x, int y) => Get(new Vector2Int(x, y));

    /// <summary>
    /// CSV を読み込む。
    /// </summary>
    /// <param name="path">プロジェクトからの相対パス。</param>
    /// <exception cref="System.FormatException">見出しが整数でないとき。</exception>
    public static StageLayout Load(string path)
    {
        StageLayout layout = new StageLayout();
        string[] lines = File.ReadAllLines(path);
        int[] xs = lines[0].Split(',').Skip(1).Select(int.Parse).ToArray();

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] columns = line.Split(',');
            int y = int.Parse(columns[0]);
            for (int i = 1; i < columns.Length && i - 1 < xs.Length; i++)
            {
                string value = columns[i].Trim();
                if (value.Length > 0)
                {
                    layout.cells[new Vector2Int(xs[i - 1], y)] = value[0];
                }
            }
        }

        return layout;
    }
}

/// <summary>
/// ステージシーンにある3つのタイルマップ。
/// </summary>
public class StageTilemaps
{
    public Tilemap Stage { get; private set; }
    public Tilemap Retry { get; private set; }
    public Tilemap Border { get; private set; }

    public Tilemap[] All => new[] { Stage, Retry, Border };

    /// <summary>
    /// シーンから名前でタイルマップを探す。1つでも欠けていれば null。
    /// </summary>
    public static StageTilemaps Find(Scene scene)
    {
        Tilemap[] found = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Tilemap>(true))
            .ToArray();

        StageTilemaps tilemaps = new StageTilemaps
        {
            Stage = found.FirstOrDefault(t => t.name == "Stage Tilemap"),
            Retry = found.FirstOrDefault(t => t.name == "Retry Tilemap"),
            Border = found.FirstOrDefault(t => t.name == "Border Tilemap"),
        };
        return tilemaps.All.Any(t => t == null) ? null : tilemaps;
    }
}

/// <summary>
/// 地形の記号（# = ~ |）をタイルマップに塗る。塗る前に3つのタイルマップを全部消す。
/// </summary>
public static class StageTilePainter
{
    private const char Trunk = '#';
    private const char Leaf = '=';
    private const char Pond = '~';
    private const char Border = '|';

    private const string TrunkPlainPath = "Assets/Pictures/Tiles/枝3.asset";
    private const string TrunkKnotPath = "Assets/Pictures/Tiles/枝1.asset";
    private const string TrunkVinePath = "Assets/Pictures/Tiles/枝2.asset";
    private const string LeafLeftPath = "Assets/Tilemap/grass_tile/main_15.asset";
    private const string LeafMiddlePath = "Assets/Tilemap/grass_tile/main_5.asset";
    private const string LeafRightPath = "Assets/Tilemap/grass_tile/main_11.asset";
    private const string PondPath = "Assets/Pictures/Tiles/池3.asset";
    private const string BorderPath = "Assets/Tilemap/crosshatch-1000_0.asset";

    // Stage1 の柱と同じ向き（時計回りに90度）
    private static readonly Matrix4x4 VerticalTrunk = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -90f));

    public static void Paint(StageLayout layout, StageTilemaps tilemaps, string undoName)
    {
        Undo.RegisterCompleteObjectUndo(tilemaps.All, undoName);
        foreach (Tilemap tilemap in tilemaps.All)
        {
            tilemap.ClearAllTiles();
        }

        TileBase pond = Load(PondPath);
        TileBase border = Load(BorderPath);

        foreach (KeyValuePair<Vector2Int, char> pair in layout.Cells)
        {
            Vector3Int position = (Vector3Int)pair.Key;
            switch (pair.Value)
            {
                case Trunk:
                    tilemaps.Stage.SetTile(position, TrunkTile(pair.Key));
                    if (IsVertical(layout, pair.Key, Trunk))
                    {
                        tilemaps.Stage.SetTransformMatrix(position, VerticalTrunk);
                    }
                    break;
                case Leaf:
                    tilemaps.Stage.SetTile(position, LeafTile(layout, pair.Key));
                    break;
                case Pond:
                    tilemaps.Retry.SetTile(position, pond);
                    break;
                case Border:
                    tilemaps.Border.SetTile(position, border);
                    break;
            }
        }
    }

    // 縦に続いていて横に続いていないセルだけを縦向きにする（床の端や段の角は横向きのまま）
    private static bool IsVertical(StageLayout layout, Vector2Int cell, char symbol)
    {
        bool horizontal = layout.Get(cell + Vector2Int.left) == symbol || layout.Get(cell + Vector2Int.right) == symbol;
        bool vertical = layout.Get(cell + Vector2Int.up) == symbol || layout.Get(cell + Vector2Int.down) == symbol;
        return vertical && !horizontal;
    }

    // Stage1 の比率（節・つるが1割ずつ）に合わせ、座標から決めて作り直しても同じ見た目にする
    private static TileBase TrunkTile(Vector2Int cell)
    {
        int hash = (cell.x * 73856093) ^ (cell.y * 19349663);
        int bucket = ((hash % 10) + 10) % 10;
        return bucket switch
        {
            0 => Load(TrunkKnotPath),
            1 => Load(TrunkVinePath),
            _ => Load(TrunkPlainPath),
        };
    }

    private static TileBase LeafTile(StageLayout layout, Vector2Int cell)
    {
        bool hasLeft = layout.Get(cell + Vector2Int.left) == Leaf;
        bool hasRight = layout.Get(cell + Vector2Int.right) == Leaf;
        if (hasLeft && !hasRight)
        {
            return Load(LeafRightPath);
        }
        if (!hasLeft && hasRight)
        {
            return Load(LeafLeftPath);
        }
        return Load(LeafMiddlePath);
    }

    private static TileBase Load(string path)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        if (tile == null)
        {
            throw new FileNotFoundException($"タイルが見つかりません：{path}");
        }
        return tile;
    }
}
