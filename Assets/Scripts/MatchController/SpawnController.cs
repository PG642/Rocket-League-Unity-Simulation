using System.Collections.Generic;
using UnityEngine;

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

        private void Start()
        {
            var spawnPositions = transform.Find("SpawnPositions");

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

        private Transform GetSpawnPosition(GameObject car, Transform spawnPositions, int team)
        {
            var childNum = spawnPositions.childCount;

            var idx = Random.Range(0, childNum - 1);
            for (var i = 0; i < childNum; i++)
            {
                idx = (idx + 1) % childNum;
                var spawnPosition = spawnPositions.GetChild(idx);
                if (_spawnPositionUsage.ContainsKey(spawnPosition))
                {
                    if (_spawnPositionUsage[spawnPosition] == car ||
                        _spawnPositionUsage[spawnPosition].transform.position == spawnPosition.transform.position)
                    {
                        _spawnPositionUsage.Add(spawnPosition, car);
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

        public GameObject SpawnCar(GameObject car, bool wasDemolished = false)
        {
            // we do not know how teams are implemented
            // team = car.transform.Find("Team").team;
            var team = 0;
            Transform spawnPositions;
            if (team == 0)
                spawnPositions = wasDemolished ? _orangeRespawnPositions : _orangeSpawnPositions;
            else
                spawnPositions = wasDemolished ? _blueRespawnPositions : _blueSpawnPositions;

            var spawnLocation = GetSpawnPosition(car, spawnPositions, team);
            car.transform.position = spawnLocation.position;
            car.transform.rotation = spawnLocation.rotation;
            return car;
        }

        public GameObject SpawnBall(GameObject ball)
        {
            ball.transform.position = _ballSpawnPosition.position;
            ball.transform.rotation = _ballSpawnPosition.rotation;
            return ball;
        }
    }
}