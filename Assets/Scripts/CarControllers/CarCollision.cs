using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollision : MonoBehaviour
{
    private MatchController.MatchController _matchController;
    private float forwardSpeed;


    void Start()
    {
        _matchController = transform.GetComponentInParent<MatchController.MatchController>();
    }


    private void FixedUpdate()
    {
        forwardSpeed = Vector3.Dot(GetComponent<Rigidbody>().velocity, transform.forward);
    }


    private void OnCollisionEnter(Collision collisionInfo)
    {
        if (!collisionInfo.gameObject.CompareTag("ControllableCar"))
            return;
        TeamController teamController = GetComponentInParent<TeamController>();
        if (teamController.GetTeamOfCar(collisionInfo.gameObject) == teamController.GetTeamOfCar(gameObject))
            return;

        Vector3 directionToOtherCogLow = collisionInfo.transform.Find("CubeController").Find("cogLow").position - transform.Find("CubeController").Find("cogLow").position;
        
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(directionToOtherCogLow, transform.up);
        Debug.DrawRay(transform.position, horizontalDirection, Color.red, 3f);
        Vector3 verticalDirection = Vector3.ProjectOnPlane(directionToOtherCogLow, transform.right);
        Debug.DrawRay(transform.position, verticalDirection.normalized*3f, Color.blue, 3f);

        float horizontalAngle = Vector3.Angle(horizontalDirection, transform.forward); 
        float verticalAngle = Vector3.Angle(verticalDirection.normalized * 3f, transform.forward); 

        Debug.Log(teamController.GetTeamOfCar(gameObject) + " horizontal :" + horizontalAngle);
        Debug.Log(teamController.GetTeamOfCar(gameObject) + " vertical :" + verticalAngle);


        if(horizontalAngle <= 45 && verticalAngle <= 37 && forwardSpeed >= 21f)
        {
            Debug.Log("DEMO!");
            _matchController.HandleDemolition(collisionInfo.gameObject);
        }
        else if(horizontalAngle <= 70 && verticalAngle <= 70)
        {
            Debug.Log("BUMPING!");
        }
    }


}