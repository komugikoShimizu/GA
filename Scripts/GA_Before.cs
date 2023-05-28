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
        #region �����v�f�쐬

        for (int i = 0; i < 4; i++)
        {
            // �����_���V�[�h���Z�b�g
            UnityEngine.Random.InitState(DateTime.Now.Millisecond + i);

            pointsCopy = new List<Vector2Int>(points);  // �A�C�e�����W���R�s�[
            genes.Add(new byte[100]);                   // �e�v�f��������

            for (int j = 0; j < 100; j++)
            {
                // �v�f�ɗ����l���쐬����
                genes[i][j] = (byte)UnityEngine.Random.Range(0, 4);
            }
        }

        #endregion

        for (int i = 0; i < roop; i++)
        {
            #region �K���x�]��

            for (int j = 0;j < 4;j++)
            {
                position = playerPosition;                  // ���W���X�V
                pointsCopy = new List<Vector2Int>(points);  // �A�C�e�����W���R�s�[
                geneScores[j] = 0;                          // �v�f�̃X�R�A��������

                for (int k = 0;k < 100;k++)
                {
                    Vector2Int moveValue = new Vector2Int();    // �ړ��ʊǗ��p�ϐ�

                    // �����l�ɂ���Ĉړ��ʂ�ݒ�
                    switch (genes[j][k])
                    {
                        case 0: moveValue = Vector2Int.left; break;
                        case 1: moveValue = Vector2Int.right; break;
                        case 2: moveValue = Vector2Int.down; break;
                        case 3: moveValue = Vector2Int.up; break;
                    }

                    // ���W��͈͓��Ɏ��߂�
                    position = new Vector2Int(Mathf.Clamp(position.x + moveValue.x,0,9),Mathf.Clamp(position.y + moveValue.y,0,9));

                    int searchIndex = pointsCopy.IndexOf(position); // ���݂̍��W���A�C�e�����W�ɑ��݂��邩���m�F
                    if (searchIndex < 0) continue;                  // �v�f��0�����ł���Ύ��v�f��

                    pointsCopy.RemoveAt(searchIndex);       // �A�C�e�����W����Ώۗv�f���폜
                    geneScores[j]++;                        // �X�R�A�����Z

                    if (pointsCopy.Count >= 1) continue;    // �A�C�e�����W�ɗv�f���c���Ă���Ȃ玟�v�f��
                    geneScores[j] += (byte)(100 - k);       // �X�R�A�Ɏc����W�̐������Z(�c����W�������قǃX�R�A�������ݒ肷�邽��)
                    break;
                }
            }
            #endregion

            #region �I��

            for (int j = 0;j < 4;j++)
            {
                if (geneScores[no1] < geneScores[j])        // ���݂̍ŏ�ʃX�R�A���X�R�A�������Ȃ�
                {
                    no2 = no1;                              // 2�ʃX�R�A���ߋ��̍ŏ�ʃX�R�A�ɐݒ�
                    no1 = j;                                // �ŏ�ʃX�R�A���Đݒ�
                }
                else if (geneScores[no2] < geneScores[j])   // 2�ʃX�R�A���X�R�A�������Ȃ�
                {
                    no2 = j;                                // 2�ʃX�R�A���Đݒ�
                }
            }

            #endregion

            #region ����(2�_����)

            bool swap = false; // ����ւ��ς݂����Ǘ�����ϐ�

            for (int j = 0;j < 4;j++)
            {
                // 2�_�����̑Ώې��l�𗐐��l����ݒ�
                int randA = UnityEngine.Random.Range(0, 98);
                int randB = UnityEngine.Random.Range(randA + 1, 100);

                for (int k = 0; k < 4; k++)
                {
                    if (no1 == k || no2 == k) continue; // �ŏ�ʃX�R�A��2�ʃX�R�A�͉����s��Ȃ�

                    byte swapValue = 0;                 // ����ւ����s���v�f��ۑ�����ϐ�

                    if (swap)
                    {
                        // �x�[�X�F2�ʃX�R�A/�����͈́F�ŏ�ʃX�R�A
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
                        // �x�[�X�F�ŏ�ʃX�R�A/�����͈́F2�ʃX�R�A
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

            #region �ˑR�ψ�

            if (i % 5 == 0)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j == no1 || j == no2) continue;

                    UnityEngine.Random.InitState(DateTime.Now.Millisecond + j);

                    // �����_����5�̗v�f���Čv�Z
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
