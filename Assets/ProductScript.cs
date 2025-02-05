using Unity.VisualScripting;
using UnityEngine;


public class ProductScript : MonoBehaviour
{
    private string currentStationName;

    public void SetStationName(string stName)
    {
        currentStationName = stName;
    }

    void OnMouseDown()
    {
        Debug.Log($"{name} => I'm at station: {currentStationName}");
    }
}

