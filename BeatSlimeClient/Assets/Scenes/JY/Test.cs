using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    

   
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(OneTileWaveEffect(0,0,1));
    }
    IEnumerator OneTileWaveEffect(int x, int z, int power)
    {
        float t = 0.0f;
        // power�� ������ �ĵ�ũ��� �ֱⰡ Ŀ����? �̰� �ʹ� �ɽ��ϴ�
        //�ϴ� �׳� ���ड
        while (t <= 1.0f)
        {

            t += Time.deltaTime;


            GameManager.data.grid.cellMaps.SetW(x, z, Mathf.Sin(t));

            yield return null;
        }

    }
}
