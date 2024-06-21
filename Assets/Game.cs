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

    private string armorURL = "http://localhost/database_proyect/get_armors.php";
    private string weaponURL = "http://localhost/database_proyect/get_weapons.php";
    private string addItemUrl = "http://localhost/database_proyect/create_inventory.php";

    private string userID;

    private Armor selectedArmor;
    private Weapon selectedWeapon;

    void Start()
    {
        // Recuperar el ID del usuario desde PlayerPrefs
        userID = PlayerPrefs.GetString("UserID", null);
        Debug.Log("User ID retrieved from PlayerPrefs: " + userID);

        StartCoroutine(GetRandomArmor());
        StartCoroutine(GetRandomWeapon());

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
                    textResultArmor.text = $"Selected Armor:\nID: {selectedArmor.id}\nName: {selectedArmor.name}\nDefense: {selectedArmor.defense}\nQuality: {selectedArmor.quality}\nDrop Rate: {selectedArmor.drop_rate}%";
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
                    textResultWeapon.text = $"Selected Weapon:\nID: {selectedWeapon.id}\nName: {selectedWeapon.name}\nAttack: {selectedWeapon.attack}\nCritical: {selectedWeapon.critical}\nQuality: {selectedWeapon.quality}\nDrop Rate: {selectedWeapon.drop_rate}%";
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
            StartCoroutine(AddItemToInventory("armor", selectedArmor.id));
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
            StartCoroutine(AddItemToInventory("weapon", selectedWeapon.id));
        }
    }

    IEnumerator AddItemToInventory(string itemType, string itemId)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userID);
        form.AddField("item_type", itemType);
        form.AddField("item_id", itemId);

        using (UnityWebRequest www = UnityWebRequest.Post(addItemUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to add item to inventory: " + www.error);
            }
            else
            {
                Debug.Log("Item added to inventory successfully.");
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

    List<Armor> GetArmorList()
    {
        // Aquí deberías tener lógica para obtener la lista real de armaduras
        return new List<Armor>();
    }

    List<Weapon> GetWeaponList()
    {
        // Aquí deberías tener lógica para obtener la lista real de armas
        return new List<Weapon>();
    }
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
