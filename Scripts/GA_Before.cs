using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GA_Before : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject positionObj;
    [SerializeField]
    private int height = 10;
    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int roop = 1000;
    [SerializeField]
    private Vector2Int playerPosition = new Vector2Int(5,5);

    private int no1 = 0;
    private int no2 = 1;
    private bool isCalc = false;
    private Vector2Int position = new Vector2Int();
    private List<Vector2Int> points = new List<Vector2Int>();
    private List<Vector2Int> pointsCopy = new List<Vector2Int>();
    private List<byte[]> genes = new List<byte[]>();
    private byte[] geneScores = new byte[4];

    private List<GameObject> objList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0;i < 10;i++)
        {
            int randX = UnityEngine.Random.Range(0,10);
            int randY = UnityEngine.Random.Range(0,10);
            Vector2Int randPos = new Vector2Int(randX,randY);

            if (points.IndexOf(randPos) < 0)
            {
                points.Add(randPos);
                objList.Add(Instantiate(positionObj, new Vector3(randX, randY), Quaternion.identity));
            }
            else
            {
                i--;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isCalc) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            isCalc = true;
            StartCoroutine(GA_Waiting());
        }
    }

    private IEnumerator GA_Waiting()
    {
        #region 初期要素作成

        for (int i = 0; i < 4; i++)
        {
            // ランダムシードリセット
            UnityEngine.Random.InitState(DateTime.Now.Millisecond + i);

            pointsCopy = new List<Vector2Int>(points);  // アイテム座標をコピー
            genes.Add(new byte[100]);                   // 各要素を初期化

            for (int j = 0; j < 100; j++)
            {
                // 要素に乱数値を作成する
                genes[i][j] = (byte)UnityEngine.Random.Range(0, 4);
            }
        }

        #endregion

        for (int i = 0; i < roop; i++)
        {
            #region 適応度評価

            for (int j = 0;j < 4;j++)
            {
                position = playerPosition;                  // 座標を更新
                pointsCopy = new List<Vector2Int>(points);  // アイテム座標をコピー
                geneScores[j] = 0;                          // 要素のスコアを初期化

                for (int k = 0;k < 100;k++)
                {
                    Vector2Int moveValue = new Vector2Int();    // 移動量管理用変数

                    // 乱数値によって移動量を設定
                    switch (genes[j][k])
                    {
                        case 0: moveValue = Vector2Int.left; break;
                        case 1: moveValue = Vector2Int.right; break;
                        case 2: moveValue = Vector2Int.down; break;
                        case 3: moveValue = Vector2Int.up; break;
                    }

                    // 座標を範囲内に収める
                    position = new Vector2Int(Mathf.Clamp(position.x + moveValue.x,0,9),Mathf.Clamp(position.y + moveValue.y,0,9));

                    int searchIndex = pointsCopy.IndexOf(position); // 現在の座標がアイテム座標に存在するかを確認
                    if (searchIndex < 0) continue;                  // 要素が0未満であれば次要素へ

                    pointsCopy.RemoveAt(searchIndex);       // アイテム座標から対象要素を削除
                    geneScores[j]++;                        // スコアを加算

                    if (pointsCopy.Count >= 1) continue;    // アイテム座標に要素が残っているなら次要素へ
                    geneScores[j] += (byte)(100 - k);       // スコアに残り座標の数を加算(残り座標が多いほどスコアを高く設定するため)
                    break;
                }
            }
            #endregion

            #region 選択

            for (int j = 0;j < 4;j++)
            {
                if (geneScores[no1] < geneScores[j])        // 現在の最上位スコアよりスコアが高いなら
                {
                    no2 = no1;                              // 2位スコアを過去の最上位スコアに設定
                    no1 = j;                                // 最上位スコアを再設定
                }
                else if (geneScores[no2] < geneScores[j])   // 2位スコアよりスコアが高いなら
                {
                    no2 = j;                                // 2位スコアを再設定
                }
            }

            #endregion

            #region 交叉(2点交叉)

            bool swap = false; // 入れ替え済みかを管理する変数

            for (int j = 0;j < 4;j++)
            {
                // 2点交叉の対象数値を乱数値から設定
                int randA = UnityEngine.Random.Range(0, 98);
                int randB = UnityEngine.Random.Range(randA + 1, 100);

                for (int k = 0; k < 4; k++)
                {
                    if (no1 == k || no2 == k) continue; // 最上位スコアと2位スコアは何も行わない

                    byte swapValue = 0;                 // 入れ替えを行う要素を保存する変数

                    if (swap)
                    {
                        // ベース：2位スコア/交叉範囲：最上位スコア
                        for (int l = 0; l < 100; l++)
                        {
                            if (l >= randA && l <= randB)
                            {
                                {
                                    swapValue = genes[no1][l];
                                }
                            }
                            else
                            {
                                swapValue = genes[no2][l];
                            }

                            genes[k][l] = swapValue;
                        }
                    }
                    else
                    {
                        // ベース：最上位スコア/交叉範囲：2位スコア
                        swap = true;

                        for (int l = 0; l < 100; l++)
                        {
                            if (l >= randA && l <= randB)
                            {
                                {
                                    swapValue = genes[no2][l];
                                }
                            }
                            else
                            {
                                swapValue = genes[no1][l];
                            }

                            genes[k][l] = swapValue;
                        }
                    }
                }
            }

            #endregion

            #region 突然変異

            if (i % 5 == 0)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j == no1 || j == no2) continue;

                    UnityEngine.Random.InitState(DateTime.Now.Millisecond + j);

                    // ランダムに5つの要素を再計算
                    for (int k = 0;k < 5;k++)
                    {
                        int randIndex = UnityEngine.Random.Range(0,100);
                        genes[j][randIndex] = (byte)UnityEngine.Random.Range(0,4);
                    }
                }
            }

            #endregion

            if (i % 2 == 0) yield return null;

        }

        isCalc = false;

        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        position = new Vector2Int(5, 5);
        pointsCopy = new List<Vector2Int>(points);
        geneScores[no1] = 0;

        for (int k = 0; k < 100; k++)
        {
            Vector2Int moveValue = new Vector2Int();

            switch (genes[no1][k])
            {
                case 0: moveValue = Vector2Int.left; break;
                case 1: moveValue = Vector2Int.right; break;
                case 2: moveValue = Vector2Int.down; break;
                case 3: moveValue = Vector2Int.up; break;
            }

            position = new Vector2Int(Mathf.Clamp(position.x + moveValue.x, 0, 9), Mathf.Clamp(position.y + moveValue.y, 0, 9));

            int searchIndex = pointsCopy.IndexOf(position);
            if (searchIndex < 0) continue;
            
            pointsCopy.RemoveAt(searchIndex);
            geneScores[no1]++;

            if (pointsCopy.Count >= 1) continue;
            geneScores[no1] += (byte)(100 - k);
            break;
        }

        position = new Vector2Int(5,5);

        for (int i = 0;i < 100;i++)
        {
            Vector2Int moveValue = Vector2Int.zero;

            switch (genes[no1][i])
            {
                case 0: moveValue = Vector2Int.left; break;
                case 1: moveValue = Vector2Int.right; break;
                case 2: moveValue = Vector2Int.down; break;
                case 3: moveValue = Vector2Int.up; break;
            }

            position += moveValue;
            position = new Vector2Int(Mathf.Clamp(position.x,0,9),Mathf.Clamp(position.y,0,9));

            int index = points.IndexOf(position);

            if (index >= 0)
            {
                points.RemoveAt(index);
                Destroy(objList[index]);
                objList.RemoveAt(index);
            }

            player.transform.position = new Vector3(position.x,position.y);

            if (points.Count <= 0) break;

            yield return new WaitForSeconds(0.3f);
        }
    }
}
