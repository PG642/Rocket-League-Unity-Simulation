using System;
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
    public float remainingTime;

    public bool isBig = false;

    private GameObject[] _cars;

    private Renderer _rend;

    private BoxCollider _hitbox;

    private Dictionary<GameObject, float> _carsInRadius;

    void Start()
    {
        _rend = transform.GetChild(0).GetComponent<Renderer>();
        _hitbox = transform.GetComponent<BoxCollider>();
        _carsInRadius = new Dictionary<GameObject, float>();
        _cars = GameObject.FindGameObjectsWithTag("ControllableCar");
    }


    void Update()
    {
        
        foreach (GameObject car in _cars)
        {
            Vector3 relativePosition = gameObject.transform.position - car.transform.position;
            float dist = new Vector2(relativePosition.x, relativePosition.z).magnitude;
            float relativeHeight = Mathf.Abs(relativePosition.y);
            if (_carsInRadius == null)
            {
                _carsInRadius = new Dictionary<GameObject, float>();
            }
            if (_carsInRadius.ContainsKey(car))
            {
                _carsInRadius[car] = dist;
            }
            else
            {
                if (relativeHeight <= height && dist <= radius)
                {
                    _carsInRadius.Add(car, dist);
                }
            }
        }

        if (!_isActive)
        {
            _rend.material.SetColor("_Color", Color.black);
            if (isBig)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }

            remainingTime = Math.Max(0, refreshTime - Time.time + _lastPickup);
            if (remainingTime==0)
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
            if (_carsInRadius.Keys.Count > 0)
            {
                PickUpBoost(getFurthestCarInRadius());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BodyCollider"))
        {
            _carsInRadius.Remove(other.GetComponentInParent<Rigidbody>().gameObject);
        }
    }

    private GameObject getFurthestCarInRadius()
    {
        GameObject furthestCar = null;
        float maxDist = 0f;
        foreach (KeyValuePair<GameObject, float> carDist in _carsInRadius)
        {
            if (carDist.Value > maxDist)
            {
                furthestCar = carDist.Key;
                maxDist = carDist.Value;
            }
        }
        return furthestCar;
    }

    private void PickUpBoost(GameObject car)
    {
        var cube = car.GetComponentInChildren<CubeBoosting>();
        bool isBoostFull = cube.IncreaseBoost(boostAmount); //IncreaseBoost returns true, if boost was already at 100
        if (!isBoostFull)
        {
            _lastPickup = Time.time;
            _isActive = false;
        }
       
    }
}
