using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour
{
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Button createAccountButton;
    public Button loginButton;
    public Text resultText;

    private string createAccountUrl = "http://localhost/database_proyect/create_user.php";
    private string loginUrl = "http://localhost/database_proyect/user_login.php";

    void Start()
    {
        createAccountButton.onClick.AddListener(CreateAccount);
        loginButton.onClick.AddListener(Login);
    }

    public void CreateAccount()
    {
        StartCoroutine(CreateAccountCoroutine());
    }

    public void Login()
    {
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator CreateAccountCoroutine()
    {
        User user = new User
        {
            username = usernameInputField.text,
            password = passwordInputField.text
        };

        string jsonString = JsonUtility.ToJson(user);
        UnityWebRequest request = new UnityWebRequest(createAccountUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            resultText.text = "Error: " + request.error;
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log(responseText); // Para verificar la respuesta en la consola de Unity

            // Tratar de encontrar el JSON válido en la respuesta
            int startIndex = responseText.IndexOf('{');
            if (startIndex != -1)
            {
                string jsonResponse = responseText.Substring(startIndex);
                try
                {
                    ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(jsonResponse);
                    if (serverResponse != null)
                    {
                        if (!string.IsNullOrEmpty(serverResponse.message))
                        {
                            resultText.text = serverResponse.message;
                        }
                        else if (!string.IsNullOrEmpty(serverResponse.error))
                        {
                            resultText.text = "Error: " + serverResponse.error;
                        }
                        else
                        {
                            resultText.text = "Unexpected response format.";
                        }
                    }
                    else
                    {
                        resultText.text = "Invalid JSON received.";
                    }
                }
                catch (System.Exception ex)
                {
                    resultText.text = "Error parsing JSON: " + ex.Message;
                }
            }
            else
            {
                resultText.text = "Unexpected response format.";
            }
        }
    }

    private IEnumerator LoginCoroutine()
    {
        User user = new User
        {
            username = usernameInputField.text,
            password = passwordInputField.text
        };

        string jsonString = JsonUtility.ToJson(user);
        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            resultText.text = "Error: " + request.error;
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Login Response: " + responseText); // Para verificar la respuesta en la consola de Unity

            // Tratar de encontrar el JSON válido en la respuesta
            int startIndex = responseText.IndexOf('{');
            if (startIndex != -1)
            {
                string jsonResponse = responseText.Substring(startIndex);
                try
                {
                    ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(jsonResponse);
                    if (serverResponse != null)
                    {
                        Debug.Log("Server Response Parsed: " + jsonResponse);

                        if (!string.IsNullOrEmpty(serverResponse.user_id))
                        {
                            Debug.Log("User ID retrieved: " + serverResponse.user_id);
                            PlayerPrefs.SetString("UserID", serverResponse.user_id);
                            PlayerPrefs.Save();

                            Debug.Log("User ID stored in PlayerPrefs: " + PlayerPrefs.GetString("UserID")); // Verificar que se guarda correctamente

                            SceneManager.LoadScene("Game");
                        }
                        else if (!string.IsNullOrEmpty(serverResponse.message))
                        {
                            resultText.text = serverResponse.message;
                        }
                        else if (!string.IsNullOrEmpty(serverResponse.error))
                        {
                            resultText.text = "Error: " + serverResponse.error;
                        }
                        else
                        {
                            resultText.text = "Unexpected response format.";
                        }
                    }
                    else
                    {
                        resultText.text = "Invalid JSON received.";
                    }
                }
                catch (System.Exception ex)
                {
                    resultText.text = "Error parsing JSON: " + ex.Message;
                }
            }
            else
            {
                resultText.text = "Unexpected response format.";
            }
        }
    }
}

[System.Serializable]
public class ServerResponse
{
    public string message;
    public string error;
    public string user_id; // Asegúrate de que el servidor esté devolviendo este campo
}

[System.Serializable]
public class User
{
    public string username;
    public string password;
}
