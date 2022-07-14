using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GunEffect : MonoBehaviour
{
    //�ѱ� ȭ�� -> ������ enemyManager�� �־�������� �̰� ���൵ ���� ���� �Ŷ� ���� ����� ���� -> ����� ���� ����
    //��� �ִϸ��̼� -> ��� �ִϸ��̼��� Ư�� �������� �Ǹ� �ѱ� ȭ�� ����Ʈ�� ����ϵ��� ��������
    //�ǰ� ����Ʈ
    //�̰� 3���� speed�ð��� �Ϸ�ž���
    public float speed = 0;

    VisualEffect vfWarning;
    VisualEffect vfShoot1;
    VisualEffect vfShoot2;
    VisualEffect vfHit1;
    VisualEffect vfHit2;

    const float lifeTime = 2;
    // Start is called before the first frame update
    void Start()
    {


        VisualEffect[] tmp = GetComponentsInChildren<VisualEffect>();
        Debug.Log(tmp.Length);
        vfWarning = tmp[0];
        vfShoot1 = tmp[1];
        vfShoot2 = tmp[2];
        vfHit1 = tmp[3];
        vfHit2 = tmp[4];

        vfWarning.Stop();
        vfShoot1.Stop();

        vfShoot2.Stop();
        vfHit1.Stop();
        vfHit2.Stop();

        GameManager.data.enemy.GetComponentInChildren<GunEffect2>().targetPos = transform.position;
        StartCoroutine(Animation());
    }
    public void Run()
    {
       
    }
   
    // Update is called once per frame
    IEnumerator Animation()
    {

       

        vfWarning.playRate = 1 / (1 / lifeTime * speed*0.8f);
        vfWarning.Play();

        //���⼭ ���ƺ���
        float t = 0;
        while (t < speed*0.8f)
        {
            t += Time.deltaTime;
            yield return null;

        }

        //��
        Animator Ani = GameManager.data.GetEnemyAnim();
        Ani.SetTrigger("StartShoot");
        Ani.SetBool("Shooting",true);


        t = 0;
        while (t < speed*0.2f)
        {
            t += Time.deltaTime;

          

            yield return null;
        }
        Ani.SetBool("Shooting",false);
        Destroy(gameObject);
    }
}
