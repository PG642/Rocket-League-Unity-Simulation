using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeBoosting : MonoBehaviour
{
    public float BoostForceMultiplier = 1f;
    const float BoostForce = 991 / 100;

    public bool isBoosting = false;

    private float _boostAmount = 32f;
    private int _boostCountdown = 13;
    CubeController _c;
    InputManager _inputManager;
    Rigidbody _rb;

    private void Start()
    {
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
                _boostAmount = Mathf.Max(0.0f, _boostAmount - 0.27f);

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
        Debug.Log($"Boost: {_boostAmount}");

    }
}
