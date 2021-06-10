using System;
using UnityEngine;

namespace Managers
{
    public class MatchManager : MonoBehaviour
    {
        public TeamInfo[] teamInfos;

        private void Start()
        {
            teamInfos = new TeamInfo[2];
            teamInfos[0] = new TeamInfo()
            {
                teamColor = new Color(0.0f, 0.48f, 1.0f)
            };
            teamInfos[1] = new TeamInfo()
            {
                teamColor = new Color(0.8f, 0.4f, 0.0f)
            };
        }

        public struct TeamInfo
        {
            public Color teamColor;
        }
    }
}