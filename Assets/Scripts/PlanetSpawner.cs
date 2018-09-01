using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
public class PlanetSpawner {
    static EntityManager planetManager;
    static MeshInstanceRenderer planetRenderer;
    static EntityArchetype planetArchetype;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        planetManager = World.Active.GetOrCreateManager<EntityManager>();
        planetArchetype = planetManager.CreateArchetype(typeof(Position),
                                                        typeof(Heading),
                                                        typeof(MoveForward),
                                                        typeof(TransformMatrix),
                                                        typeof(MoveSpeed));
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitWithScene()
    {
        planetRenderer = GameObject.FindObjectOfType<MeshInstanceRendererComponent>().Value;
        for(int i = 0; i < 40000; i++)
        {
            SpawnPlanet();
        }
    }

    static void SpawnPlanet()
    {
        Entity planetEnitity = planetManager.CreateEntity(planetArchetype);
        Vector3 pos = Random.insideUnitSphere * 100;
        planetManager.SetComponentData(planetEnitity, new Position { Value = new float3(pos.x, 0, pos.z)});
        planetManager.SetComponentData(planetEnitity, new Heading { Value = new float3(1, 0, 0) });
        planetManager.SetComponentData(planetEnitity, new MoveSpeed { speed = 15f });

        planetManager.AddSharedComponentData(planetEnitity, planetRenderer);
    }
}
