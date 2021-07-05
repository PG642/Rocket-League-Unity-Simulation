using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Rounds the physics Properties of the Ball and cars in the same way Rocketleague does at the end of every frame
public class Rounding : MonoBehaviour
{

    Rigidbody ball;

    Rigidbody[] cars;

    public int decimalsPosition = 4;

    public int decimalsVelocity = 4;

    public int decimalsAngularVelocity = 4;

    public int decimalsRotation = 4;
    
    void Start()
    {
        ball = FindObjectOfType<Ball>().gameObject.GetComponent<Rigidbody>();
        List<Rigidbody> carlist = new List<Rigidbody>();
        foreach(InputManager car in FindObjectsOfType<InputManager>())
        {
            carlist.Add(car.gameObject.GetComponent<Rigidbody>());
        }
        cars = carlist.ToArray();
    }
    
    void LateUpdate()
    {
        roundRigidbody(ball);
        foreach(Rigidbody car in cars)
        {
            roundRigidbody(car);
        }
    }

    void roundRigidbody(Rigidbody rb)
    {
        rb.transform.position = roundToDecimals(decimalsPosition, rb.transform.position);
        rb.velocity = roundToDecimals(decimalsVelocity, rb.velocity);
        rb.angularVelocity = roundToDecimals(decimalsAngularVelocity, rb.angularVelocity);
        Vector3 eulerRotation = roundToDecimals(decimalsRotation, rb.rotation.eulerAngles);
        rb.rotation.eulerAngles.Set(eulerRotation.x, eulerRotation.y, eulerRotation.z);
    }

    Vector3 roundToDecimals(int decimalPlaces, Vector3 toRound)
    {
        return new Vector3(roundToDecimals(decimalPlaces, toRound.x),
                           roundToDecimals(decimalPlaces, toRound.y),
                           roundToDecimals(decimalPlaces, toRound.z));
    }

    float roundToDecimals(int decimalPlaces, float toRound)
    {
        float factor = Mathf.Pow(10f, decimalPlaces);
        return Mathf.Round(toRound * factor) / factor;
    }
}
