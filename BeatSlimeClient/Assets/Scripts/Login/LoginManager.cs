using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
public class LoginManager : MonoBehaviour
{
    public Animator self;

    public TMPro.TMP_Text ID;
    public TMPro.TMP_Text Password;

    public void Start()
    {
        Network.CreateAndConnect();
    }
    // Start is called before the first frame update
    public void scrollToLogin()
    {
        ID.text = "";
        Password.text = "";
        self.SetTrigger("scrollToLogin");
        print("scrollToLogin");
    }

    public void scrollToCenter()
    {
        self.SetTrigger("scrollToCenter");

    }

    public void SendLogin()
    {
        //print("DEBUG LOGIN");
        string id = ID.text;//.Remove(ID.text.Length-1,1);

        string idChecker = Regex.Replace(id, @"[^a-zA-Z0-9]{1,20}", "", RegexOptions.Singleline);
        id = id.Remove(ID.text.Length - 1, 1);

        if (id == "_MAPMAKER")
        {
            SceneManager.LoadScene("MapMakingScene");
            return;
        }

        if (id.Equals(idChecker) == false) { 
            print("�߸��� ���̵� �����Դϴ� ���Ŀ� ���� �ٽ� �ۼ��� �ּ���(Ư�� ���� ��� �Ұ���, ���� �� 20����)"); 
            print(id.Length);
            print(id);
            print(idChecker.Length);
            print(idChecker);
            return; 
        }

        
       
        SceneManager.LoadScene("FieldScene");
        
        Network.SendLogIn(id);
    }
}
