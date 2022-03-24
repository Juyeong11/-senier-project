using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FieldInputManager : MonoBehaviour
{
    //-----------------------------------
    public List<GameObject> Menus;
    private int menuDuplicate;
    private int preHittedObject;
    private int HittedObject;

    //-----------------------------------

    public CinemachineVirtualCamera CCO;
    CinemachineTransposer CT;

    void Awake()
    {
        CT = CCO.GetCinemachineComponent<CinemachineTransposer>();
        preHittedObject = 0;
        menuDuplicate = 0;
    }
 
    void Update ()
    {
        //�޴�
        if(Input.GetMouseButtonDown(1))  // ���콺�� Ŭ�� �Ǹ�
        {
            //�ִϸ��̼����� �ٲܰ�
            Menus[menuDuplicate].GetComponent<BillboardUI>().GetOff();
            menuDuplicate ^= 1;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                HittedObject = hit.transform.GetHashCode();
                //print("something!");
                if (hit.collider.tag == "Slimes" && (preHittedObject != HittedObject || (Menus[0].activeSelf == false && Menus[1].activeSelf == false)))
                {
                    Menus[menuDuplicate].transform.position = hit.transform.position;
                    Menus[menuDuplicate].GetComponent<BillboardUI>().GetOn(hit.transform);
                    //print("hit!");
                }
                else if (hit.collider.tag == "Cells")
                {
                    //A* �̵�
                    hit.transform.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    //FieldGameManager.data.player.GetComponent<FieldPlayerManager>().Destination = hit.transform.GetComponent<HexCellPosition>().coordinates;
                    //FieldGameManager.data.player.GetComponent<FieldPlayerManager>().ASTAR();

                }

                preHittedObject = HittedObject;
            }
        }

        //ī�޶�
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (CT.m_FollowOffset.y < 10f)
            {
                CT.m_FollowOffset.y += 0.3f;
                CT.m_FollowOffset.z -= 0.2f;
            }
            
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            if (CT.m_FollowOffset.y > 0.5f)
            {
                CT.m_FollowOffset.y -= 0.3f;
                CT.m_FollowOffset.z += 0.2f;
            }
        }
    }
}
