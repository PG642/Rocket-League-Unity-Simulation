using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    private bool _isActive = true;
    private float _lastPickup;
   
    //Default small boost pad
    public float refreshTime = 4f;
    public float boostAmount = 12;
    public float height = 1.65f;
    public float radius = 1.44f;

    public bool isBig = false;

    private Renderer _rend;

    void Start()
    {
        _rend = transform.GetChild(0).GetComponent<Renderer>();
    }


    void Update()
    {
        if (!_isActive)
        {
            _rend.material.SetColor("_Color", Color.black);
            if (isBig)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }

            if (Time.time - _lastPickup >= refreshTime)
            {
                _isActive = true;
            }
        }
        else
        {
            _rend.material.SetColor("_Color", new Color(1f, 1f, 0.3254716f, 1f));
            if (isBig)
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }

            GameObject[] cars = GameObject.FindGameObjectsWithTag("ControllableCar");
            
            foreach (GameObject car in cars)
            {
                Vector3 relativePosition = gameObject.transform.position - car.transform.position;
                float dist = new Vector2(relativePosition.x, relativePosition.z).magnitude;
                float relativeHeight = Mathf.Abs(relativePosition.y);
                if (relativeHeight <= height && dist <= radius)
                {
                    if (_isActive) {
                        var cube = car.GetComponentInChildren<CubeBoosting>();
                        Debug.Log(cube);
                        bool isBoostFull = cube.IncreaseBoost(boostAmount); //IncreaseBoost returns true, if boost was already at 100
                        if (!isBoostFull)
                        {
                            PickUpBoost();
                        }
                    }
                }
            }
            
        }
    }

    private void PickUpBoost()
    {
        _lastPickup = Time.time;
        _isActive = false;
    }
}
