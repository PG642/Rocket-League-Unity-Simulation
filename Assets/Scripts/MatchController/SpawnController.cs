using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using Random = UnityEngine.Random;

namespace MatchController
{
    public struct SpawnLocation
    {
        public SpawnLocation(float x, float y, float z, float yaw)
        {
            location = new Vector3(x, y, z);
            orientation = Quaternion.AngleAxis(yaw, Vector3.up);
            last_used = float.NegativeInfinity;
        }
            
        public Vector3 location;
        public Quaternion orientation;
        public float last_used;
    }
    
    public class SpawnController : MonoBehaviour
    {
        List<SpawnLocation> blue_spawn_points = new List<SpawnLocation>();
        List<SpawnLocation> orange_spawn_points = new List<SpawnLocation>();
        List<SpawnLocation> blue_demolition_spawn_points = new List<SpawnLocation>();
        List<SpawnLocation> orange_demolition_spawn_points = new List<SpawnLocation>();

        private void Start()
        {
            blue_spawn_points.Add(new SpawnLocation(-20.48f,  0.0f, -25.6f, 45));
            blue_spawn_points.Add(new SpawnLocation(20.48f, 0.0f, -25.6f, 135));
            blue_spawn_points.Add(new SpawnLocation(-2.56f, 0.0f, -38.4f, 90));
            blue_spawn_points.Add(new SpawnLocation(2.56f, 0.0f, -38.4f, 90));
            blue_spawn_points.Add(new SpawnLocation(0.0f, 0.0f, -46.08f, 90));
            
            orange_spawn_points.Add(new SpawnLocation(20.48f,  0.0f, 25.6f, 135));
            orange_spawn_points.Add(new SpawnLocation(-20.48f, 0.0f, 25.6f, -45));
            orange_spawn_points.Add(new SpawnLocation(2.56f, 0.0f, 38.4f, -90));
            orange_spawn_points.Add(new SpawnLocation(-2.56f, 0.0f, 38.4f, -90));
            orange_spawn_points.Add(new SpawnLocation(0.0f, 0.0f, 46.08f, -90));
            
            blue_demolition_spawn_points.Add(new SpawnLocation(-23.04f, 0.0f, -46.08f, 90));
            blue_demolition_spawn_points.Add(new SpawnLocation(-26.88f, 0.0f, -46.08f, 90));
            blue_demolition_spawn_points.Add(new SpawnLocation(23.04f, 0.0f, -46.08f, 90));
            blue_demolition_spawn_points.Add(new SpawnLocation(26.88f, 0.0f, -46.08f, 90));
            
            orange_demolition_spawn_points.Add(new SpawnLocation(-23.04f, 0.0f, -46.08f, 90));
            orange_demolition_spawn_points.Add(new SpawnLocation(-26.88f, 0.0f, -46.08f, 90));
            orange_demolition_spawn_points.Add(new SpawnLocation(23.04f, 0.0f, -46.08f, 90));
            orange_demolition_spawn_points.Add(new SpawnLocation(26.88f, 0.0f, -46.08f, 90));
        }

        public SpawnLocation GetSpawnPosition(int team)
        {
            List<SpawnLocation> spawn_points = team == 0 ? orange_spawn_points : blue_spawn_points;
            
            int idx = Random.Range(0,4);
            while (Time.time - spawn_points[idx].last_used < 5.0f)
            {
                idx++;
            }

            return spawn_points[idx];
        }
        
        public SpawnLocation GetDemolitionSpawnPosition(int team)
        {
            List<SpawnLocation> spawn_points = team == 0 ? orange_demolition_spawn_points : blue_demolition_spawn_points;
            
            int idx = Random.Range(0, 3);
            while (Time.time - spawn_points[idx].last_used < 5.0f)
            {
                idx++;
            }

            return spawn_points[idx];
        }
        
        
    }
}