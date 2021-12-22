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
            var idx = Random.Range(0, childNum - 1);
            for (var i = 0; i < childNum; i++)
            {
                idx = (idx + 1) % childNum;
                var spawnPosition = spawnPositions.GetChild(idx);
                if (_spawnPositionUsage.ContainsKey(spawnPosition))
                {
                    if (_spawnPositionUsage[spawnPosition] == car ||
                        (_spawnPositionUsage[spawnPosition].transform.position - spawnPosition.transform.position).magnitude > 0.1)
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
            car.transform.position = spawnLocation.position;
            car.transform.rotation = spawnLocation.rotation;

            return car;
        }

        public void SpawnOppositeCars(GameObject[] teamBlue, GameObject[] teamOrane)
        {
            var teamSize = teamBlue.Length;
            var spawnNum = _blueSpawnPositions.childCount;

            var rnd = new System.Random();
            var spawns = Enumerable.Range(0, spawnNum).ToList().OrderBy(x=>rnd.Next()).Take(teamSize).ToList();

            for(int i=0; i<teamSize; i++)
            {
                teamBlue[i].transform.position = _blueSpawnPositions.GetChild(spawns[i]).position;
                teamBlue[i].transform.rotation = _blueSpawnPositions.GetChild(spawns[i]).rotation;
                teamBlue[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                teamBlue[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                teamOrane[i].transform.position = _orangeSpawnPositions.GetChild(spawns[i]).position;
                teamOrane[i].transform.rotation = _orangeSpawnPositions.GetChild(spawns[i]).rotation;
                teamOrane[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                teamOrane[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }

        public GameObject SpawnBall(GameObject ball)
        {
            ball.transform.position = _ballSpawnPosition.position;
            ball.transform.rotation = _ballSpawnPosition.rotation;
            return ball;
        }
    }
}