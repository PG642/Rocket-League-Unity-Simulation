using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using System.Linq;

namespace MatchController
{
    public class SpawnController : MonoBehaviour
    {
        private readonly Dictionary<Transform, GameObject>
            _spawnPositionUsage = new Dictionary<Transform, GameObject>();

        private Transform _ballSpawnPosition;

        private Transform _blueFallBackSpawnPosition;
        private Transform _blueRespawnPositions;
        private Transform _blueSpawnPositions;

        private Transform _orangeFallBackSpawnPosition;
        private Transform _orangeRespawnPositions;
        private Transform _orangeSpawnPositions;
        
        private void Awake()
        {
            var spawnPositions = transform.Find("World").Find("Rocket_Map").Find("SpawnPositions");

            _orangeSpawnPositions = spawnPositions.Find("Orange").Find("Spawn");
            _blueSpawnPositions = spawnPositions.Find("Blue").Find("Spawn");
            _orangeRespawnPositions = spawnPositions.Find("Orange").Find("Respawn");
            _blueRespawnPositions = spawnPositions.Find("Blue").Find("Respawn");

            var up = transform.up;
            _orangeFallBackSpawnPosition =
                Instantiate(_orangeSpawnPositions.GetChild(_orangeSpawnPositions.childCount - 1));
            _orangeFallBackSpawnPosition.transform.localPosition += 10 * up;
            _blueFallBackSpawnPosition = Instantiate(_blueSpawnPositions.GetChild(_blueSpawnPositions.childCount - 1));
            _blueFallBackSpawnPosition.transform.localPosition += 10 * up;

            _ballSpawnPosition = spawnPositions.Find("Ball").Find("Center");
        }

        private Transform GetSpawnPosition(GameObject car, TeamController.Team team, bool wasDemolished)
        {
            Transform spawnPositions;
            if (team == TeamController.Team.ORANGE)
                spawnPositions = wasDemolished ? _orangeRespawnPositions : _orangeSpawnPositions;

            else
                spawnPositions = wasDemolished ? _blueRespawnPositions : _blueSpawnPositions;

            var childNum = spawnPositions.childCount;
            var idx = Random.Range(0, childNum);
            for (var i = 0; i < childNum; i++)
            {
                var spawnPosition = spawnPositions.GetChild(idx);
                if (_spawnPositionUsage.ContainsKey(spawnPosition))
                {
                    if (_spawnPositionUsage[spawnPosition] == car ||
                        (_spawnPositionUsage[spawnPosition].transform.localPosition - spawnPosition.transform.localPosition).magnitude > 0.1)
                    {
                        _spawnPositionUsage[spawnPosition] = car;
                        return spawnPosition;
                    }
                }
                else
                {
                    _spawnPositionUsage.Add(spawnPosition, car);
                    return spawnPosition;
                }
            }

            // this code should never be reached but it is here as a safety net
            return team == 0 ? _orangeFallBackSpawnPosition : _blueFallBackSpawnPosition;
        }
        
        public GameObject SpawnCar(GameObject car, TeamController.Team team, bool wasDemolished = false)
        {
            var spawnLocation = GetSpawnPosition(car, team, wasDemolished);
            car.transform.localPosition = new Vector3(0f, 0.1701f, 0f) + spawnLocation.localPosition;
            car.transform.localRotation = Quaternion.AngleAxis(0.55f, Vector3.right) * spawnLocation.localRotation;
            Rigidbody rb = car.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            return car;
        }

        public void SpawnOppositeCars(GameObject[] teamBlue, GameObject[] teamOrange)
        {
            var blueTeamSize = teamBlue.Length;
            var orangeTeamSize = teamOrange.Length;
            var maxTeamSize = blueTeamSize > orangeTeamSize ? blueTeamSize : orangeTeamSize;
            var spawnNum = _blueSpawnPositions.childCount;

            var rnd = new System.Random();
            var spawns = Enumerable.Range(0, spawnNum).ToList().OrderBy(x=>rnd.Next()).Take(maxTeamSize).ToList();

            for (int i = 0; i < maxTeamSize; i++)
            {
                if (i < blueTeamSize)
                {
                    teamBlue[i].transform.localPosition = new Vector3(0f, 0.1701f, 0f) + _blueSpawnPositions.GetChild(spawns[i]).localPosition;
                    teamBlue[i].transform.localRotation = Quaternion.AngleAxis(0.55f, Vector3.right) * _blueSpawnPositions.GetChild(spawns[i]).localRotation;
                    teamBlue[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                    teamBlue[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                }

                if (i < orangeTeamSize)
                {
                    teamOrange[i].transform.localPosition = new Vector3(0f, 0.1701f, 0f) + _orangeSpawnPositions.GetChild(spawns[i]).localPosition;
                    teamOrange[i].transform.localRotation = Quaternion.AngleAxis(0.55f, Vector3.right) * _orangeSpawnPositions.GetChild(spawns[i]).localRotation;
                    teamOrange[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                    teamOrange[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                }
            }
        }

        public GameObject SpawnBall(GameObject ball)
        {
            ball.transform.localPosition = new Vector3(0, ball.GetComponentInChildren<SphereCollider>().radius, 0) + _ballSpawnPosition.localPosition;
            ball.transform.localRotation = _ballSpawnPosition.localRotation;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            return ball;
        }
    }
}