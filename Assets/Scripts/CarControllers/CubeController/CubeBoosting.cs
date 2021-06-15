using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeBoosting : MonoBehaviour
{
    public float BoostForceMultiplier = 1f;
    const float BoostForce = 991 / 100;

    public bool isBoosting = false;
    private float _boostAmount = 32f;
    private int _boostCountdown = 13;
    public bool _infiniteBoosting = false;
    CubeController _c;
    InputManager _inputManager;
    Rigidbody _rb;
    GUIStyle _style;

    private void Start()
    {
        _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        _style.fontSize = 25;
        _style.fontStyle = FontStyle.Bold;

        if (_infiniteBoosting)
        {
            _boostAmount = 100f;
        }

        _inputManager = GetComponentInParent<InputManager>();
        _c = GetComponent<CubeController>();
        _rb = GetComponentInParent<Rigidbody>();

        // Activate ParticleSystems GameObject
        if (Resources.FindObjectsOfTypeAll<CubeParticleSystem>()[0] != null)
            Resources.FindObjectsOfTypeAll<CubeParticleSystem>()[0].gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        Boosting();
    }

    void Boosting()
    {
        if (_inputManager.isBoost || (_boostCountdown < 13 && _boostCountdown > 0))
        {
            _boostCountdown--;
            if (_boostAmount > 0)
            {
                isBoosting = true;
                if (_c.forwardSpeed < CubeController.MaxSpeedBoost)
                {
                    _rb.AddForce(BoostForce * BoostForceMultiplier * transform.forward, ForceMode.Acceleration);
                }
                if (!_infiniteBoosting)
                {
                    _boostAmount = Mathf.Max(0.0f, _boostAmount - 0.27f);
                }

            }
            else
            {
                isBoosting = false;
            }
        }
        else
        {
            _boostCountdown = 13;
            isBoosting = false;
        }

    }

    //returns true if the boost was alreade 100
    public bool IncreaseBoost(float boost)
    {
        if(_boostAmount == 100)
        {
            return true;
        }
        else
        {
            _boostAmount = Mathf.Min(100f, _boostAmount + boost);
            return false;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 140, Screen.height - 50, 150, 130), $"Boost {(int)_boostAmount}", _style);

    }
}
