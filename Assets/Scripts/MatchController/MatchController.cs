using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchController
    {
    public class MatchController : MonoBehaviour
    {

        private TeamController _teamController;
        private MapData _mapData;
        private Transform _ball;

        // Start is called before the first frame update
        void Start()
        {
            _mapData = transform.Find("World").GetComponentInChildren<MapData>();
            _teamController = GetComponent<TeamController>();
            _teamController.Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            if(_mapData.isScoredBlue || _mapData.isScoredOrange)
            {
                _teamController.SpawnTeams();
                _ball.GetComponent<Ball>().ResetBall();
                _mapData.ResetIsScored();
            }
        }

    }

}
