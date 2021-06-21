using System;
using System.Collections;
using System.IO;
using System.Linq;
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
        //car.SetPositionAndRotation(position,rotation);
        var car_rb =GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        car_rb.position = position + new Vector3(0.0f,0.18f,0.0f);
        car_rb.rotation = rotation;
        car_rb.velocity = new Vector3(carStartValue.velocity.x, carStartValue.velocity.y, carStartValue.velocity.z);
        var result = StartCoroutine(TestMethod(3, car_rb, carStartValue));
        
    }

    public IEnumerator TestMethod(int number, Rigidbody car, ValuePair carStartValue)
    {
        
        yield return new WaitForSeconds(number) ;
        car.velocity = new Vector3(carStartValue.velocity.x, carStartValue.velocity.y, carStartValue.velocity.z);
        
    }


    // Update is called once per frame
    void Update()
    {
    }
}