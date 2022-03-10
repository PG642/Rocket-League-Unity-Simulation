using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeBoosting : MonoBehaviour
{
    private const float BoostForce = 991.666f / 100f;
    
    private int _boostCountdown = 13;
    private CubeController _c;
    private InputManager _inputManager;
    private Rigidbody _rb;
    private GUIStyle _style;
    
    public bool isBoosting;
    public bool infiniteBoosting;
    public bool disableBoosting;
    public float boostForceMultiplier = 1f;
    public float boostAmount = 33f;
    public Agent agent;



    private void Start()
    {
        _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        _style.fontSize = 25;
        _style.fontStyle = FontStyle.Bold;
        agent = GetComponentInParent<Agent>();

        if (infiniteBoosting)
        {
            boostAmount = 100f;
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

    private void Boosting()
    {
        if(disableBoosting) return;
        if (_inputManager.isBoost || (_boostCountdown < 13 && _boostCountdown > 0) )
        {
            _boostCountdown--;
            if (boostAmount > 0)
            {
                isBoosting = true;
                if (_c.forwardSpeed < CubeController.MaxSpeedBoost)
                {
                    _rb.AddForce(BoostForce * boostForceMultiplier * transform.forward, ForceMode.Acceleration);
                }

                if (!infiniteBoosting)
                {
                    boostAmount = Mathf.Max(0.0f, boostAmount - 0.27f);
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

    public void SetInfiniteBoost(bool infiniteBoost)
    {
        infiniteBoosting = infiniteBoost;
    }

    //returns true if the boost was alreade 100
    public bool IncreaseBoost(float boost)
    {
        if (boostAmount == 100)
        {
            return true;
        }
        
        boostAmount = Mathf.Min(100f, boostAmount + boost);
        return false;
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(Screen.width - 140, Screen.height - 50, 150, 130), $"Boost {(int)boostAmount}", _style);
    }
}