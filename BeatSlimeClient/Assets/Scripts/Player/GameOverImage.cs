using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameEndTraits {
    None,
    Win,
    Lose
}

public class GameOverImage : MonoBehaviour
{
    public GameEndTraits gameEnder;
    public Text t;
    public GameObject Center;


    void Start()
    {
        gameEnder = GameEndTraits.None;
        Center.SetActive(false);
    }

    void Update()
    {
        // ���� Ŭ������ �����ִ� ���� : PlayerManager���� �ϸ� �� �÷��̾�� ������ ȭ�鿡 ���� ������ �߱� ����.
        if (gameEnder == GameEndTraits.Lose)
        {
            t.text = "Game Over";
            Center.SetActive(true);
        }
        else if (gameEnder == GameEndTraits.Win)
        {
            t.text = "Game Clear!";
            Center.SetActive(true);
        }
    }

    public void SetGameEnd(GameEndTraits end)
    {
        gameEnder = end;
    }
}
