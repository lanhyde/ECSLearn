using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Orbit : MonoBehaviour {
    private GameObject[] planets;
    public GameObject sun;
    public int numPlanets = 500;

    Transform[] planetTransforms;
    TransformAccessArray planetTransformAccessArray;
    PositionUpdateJob planetJob;
    JobHandle planetPositionJobHandle;
    struct PositionUpdateJob : IJobParallelForTransform
    {
        public Vector3 sunPos;
        public void Execute(int index, TransformAccess transform)
        {
            Vector3 direction = (sunPos - transform.position);
            float gravity = Mathf.Clamp(direction.magnitude / 100f, 0, 1);
            Quaternion lookQuaternion = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookQuaternion, gravity);

            float orbitalSpeed = Mathf.Sqrt(50 / direction.magnitude);
            transform.position += transform.rotation * Vector3.forward * orbitalSpeed;

        }
    }
    // Use this for initialization
    void Start () {
        planets = new GameObject[numPlanets];
        planetTransforms = new Transform[numPlanets];

        for(int i = 0; i < numPlanets; i++)
        {
            planets[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planets[i].transform.position = Random.insideUnitSphere * 50;
            planetTransforms[i] = planets[i].transform;
        }
        planetTransformAccessArray = new TransformAccessArray(planetTransforms);
	}
	
	// Update is called once per frame
	void Update () {
        planetJob = new PositionUpdateJob()
        {
            sunPos = sun.transform.position
        };
        planetPositionJobHandle = planetJob.Schedule(planetTransformAccessArray);
	}

    private void LateUpdate()
    {
        planetPositionJobHandle.Complete();   
    }

    private void OnDestroy()
    {
        planetTransformAccessArray.Dispose();
    }
}
