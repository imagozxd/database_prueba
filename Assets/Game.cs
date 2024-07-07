using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    public Text textResultArmor;
    public Text textResultWeapon;
    public Button addArmorButton;
    public Button addWeaponButton;
    public Button enemigo1; // Declaración del nuevo botón

    private string armorURL = "http://localhost/database_proyect/get_armors.php";
    private string weaponURL = "http://localhost/database_proyect/get_weapons.php";
    private string updateItemUrl = "http://localhost/database_proyect/update_inventory.php";

    private string userID;

    private Armor selectedArmor;
    private Weapon selectedWeapon;

    void Start()
    {
        // Recuperar el ID del usuario desde PlayerPrefs
        userID = PlayerPrefs.GetString("UserID", null);
        Debug.Log("User ID retrieved from PlayerPrefs: " + userID);

        enemigo1.onClick.AddListener(GenerateItemsAndSetupButtons);
    }

    void GenerateItemsAndSetupButtons()
    {
        StartCoroutine(GetRandomArmor());
        StartCoroutine(GetRandomWeapon());

        addArmorButton.onClick.RemoveAllListeners();
        addWeaponButton.onClick.RemoveAllListeners();

        addArmorButton.onClick.AddListener(AddArmorToInventory);
        addWeaponButton.onClick.AddListener(AddWeaponToInventory);
    }

    IEnumerator GetRandomArmor()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(armorURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch armor data: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                ArmorData armorData = JsonUtility.FromJson<ArmorData>(json);

                selectedArmor = SelectRandomItem(armorData.armors);

                if (selectedArmor != null)
                {
                    //textResultArmor.text = $"Selected Armor:\nID: {selectedArmor.id}\nName: {selectedArmor.name}\nDefense: {selectedArmor.defense}\nQuality: {selectedArmor.quality}\nDrop Rate: {selectedArmor.drop_rate}%";

                    textResultArmor.text = $"Name: {selectedArmor.name}\nDefense: {selectedArmor.defense}%";
                }
            }
        }
    }

    IEnumerator GetRandomWeapon()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(weaponURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch weapon data: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                WeaponData weaponData = JsonUtility.FromJson<WeaponData>(json);

                selectedWeapon = SelectRandomItem(weaponData.weapons);

                if (selectedWeapon != null)
                {
                    //textResultWeapon.text = $"Selected Weapon:\nID: {selectedWeapon.id}\nName: {selectedWeapon.name}\nAttack: {selectedWeapon.attack}\nCritical: {selectedWeapon.critical}\nQuality: {selectedWeapon.quality}\nDrop Rate: {selectedWeapon.drop_rate}%";

                    textResultWeapon.text = $"Name: {selectedWeapon.name}\nAttack: {selectedWeapon.attack}\nCritical: {selectedWeapon.critical}%";
                }
            }
        }
    }

    public void AddArmorToInventory()
    {
        if (userID == null)
        {
            Debug.LogError("User ID not set.");
            return;
        }

        if (selectedArmor != null)
        {
            StartCoroutine(UpdateItemInInventory("armor", selectedArmor.id));
        }
    }

    public void AddWeaponToInventory()
    {
        if (userID == null)
        {
            Debug.LogError("User ID not set.");
            return;
        }

        if (selectedWeapon != null)
        {
            StartCoroutine(UpdateItemInInventory("weapon", selectedWeapon.id));
        }
    }

    IEnumerator UpdateItemInInventory(string itemType, string itemId)
    {
        InventoryUpdateData updateData = new InventoryUpdateData
        {
            id_user = userID,
            item_type = itemType,
            item_id = itemId
        };

        string jsonString = JsonUtility.ToJson(updateData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(updateItemUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending data: " + jsonString);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to update inventory: " + www.error);
            }
            else
            {
                Debug.Log("Inventory updated successfully.");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }

    T SelectRandomItem<T>(List<T> itemList) where T : IRandomizable
    {
        float totalWeight = 0;

        foreach (T item in itemList)
        {
            totalWeight += item.GetDropRate();
        }

        float randomPoint = Random.value * totalWeight;

        foreach (T item in itemList)
        {
            if (randomPoint < item.GetDropRate())
            {
                return item;
            }
            randomPoint -= item.GetDropRate();
        }

        return default(T);
    }
}

[System.Serializable]
public class InventoryUpdateData
{
    public string id_user;
    public string item_type;
    public string item_id;
}

[System.Serializable]
public class ArmorData
{
    public List<Armor> armors;
}

[System.Serializable]
public class WeaponData
{
    public List<Weapon> weapons;
}

[System.Serializable]
public class Armor : IRandomizable
{
    public string id;
    public string name;
    public string defense;
    public string quality;
    public string drop_rate;

    public float GetDropRate()
    {
        return float.Parse(drop_rate);
    }
}

[System.Serializable]
public class Weapon : IRandomizable
{
    public string id;
    public string name;
    public string attack;
    public string critical;
    public string quality;
    public string drop_rate;

    public float GetDropRate()
    {
        return float.Parse(drop_rate);
    }
}

public interface IRandomizable
{
    float GetDropRate();
}
