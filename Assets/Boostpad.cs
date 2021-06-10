using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boostpad : MonoBehaviour
{
    public bool isBig = true;

    private bool _isActive = true;
    private float _lastPickup;

    private const float _SmallRefresh = 4f;
    private const int _SmallBoostAmount = 12;

    private const float _BigRefresh = 10f;
    private const int _BigBoostAmount = 100;

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

            if ((!isBig && Time.time - _lastPickup >= _SmallRefresh) || (isBig && Time.time - _lastPickup >= _BigRefresh))
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
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag.Equals("BodyCollider"))
        {
            if (_isActive)
            {
                var cube =other.gameObject.GetComponentInParent<CubeBoosting>();
                Debug.Log(cube);
                bool isBoostFull = cube.IncreaseBoost(isBig ? _BigBoostAmount : _SmallBoostAmount); //IncreaseBoost returns true, if boost was already at 100
                if (!isBoostFull)
                {
                    PickUpBoost();
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
