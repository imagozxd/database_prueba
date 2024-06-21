//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class DBConn : MonoBehaviour
{
    private string url = "http://localhost/database_proyect/user_login.php";
    [SerializeField] private User user;
    IEnumerator Start()
    {
        user.username = "testuser";
        user.password = "password123";
        string jsonString = JsonUtility.ToJson(user);   //para no usar el newtonsoft cambiar a JsonUtility.ToJson    o descargar lo que hice en casa
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log(responseText);
        }
    }
}

