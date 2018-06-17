using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgetPassword : MonoBehaviour {

    // Use this for initialization

    [SerializeField]
    InputField txtbox_username;

    string url = "http://webcity.online/email/send_emailv2.php?username={0}";



    void Start () {
	}

    // Update is called once per frame
    void Update()
    {

    }

   

    public void OnClickSubmit()
    {
        string valuetxtbox = txtbox_username.text.ToString();
        url = string.Format(url,valuetxtbox);

        WWW hs_post = new WWW(url);

    }
}
