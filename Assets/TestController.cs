using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Tests;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public TextAsset jsonFile;

    // Start is called before the first frame update
    void Start()
    {
        var fromJson = JsonUtility.FromJson<Root>(jsonFile.text);
        Debug.Log(fromJson);
        SetupCar(fromJson);
    }

    private void SetupCar(Root fromJson)
    {
        var car = transform.Find("ControllableCar");
        var carStartValue = fromJson.startValues.Find(x => x.name == "car");
        var rotation = Quaternion.Euler(carStartValue.rotation.x, carStartValue.rotation.y, carStartValue.rotation.z);
        var position = new Vector3(carStartValue.position.x, carStartValue.position.y, carStartValue.position.z);
        car.SetPositionAndRotation(position,rotation);

    }


    // Update is called once per frame
    void Update()
    {
    }
}