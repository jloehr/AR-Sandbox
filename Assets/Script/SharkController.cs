using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharkController : MonoBehaviour {

    public GameObject SharkPrefab;
    public DepthMesh Terrain;

    public float SharksPerSquare;
    public int MaxSharks;
    public float SharkLayer;
    public float SharkLayerWidth;

    private int TerrainWidthBuffer = 0;
    private int TerrainHeightBuffer = 0;

    private List<SharkMovement> SharkPopulation = new List<SharkMovement>();

	// Use this for initialization
	void Start () {
	    if(SharkPrefab == null)
        {
            Debug.LogError("No Shark Prefab given!");
            this.enabled = false;
            return;
        }

        if (Terrain == null)
        {
            Debug.LogError("No Terrain given!");
            this.enabled = false;
            return;
        }

	}
	
	// Update is called once per frame
	void Update () {
        CheckTerrain();
	}

    void CheckTerrain()
    {
        if((Terrain.Width != TerrainWidthBuffer) || (Terrain.Height != TerrainHeightBuffer))
        {
            TerrainWidthBuffer = Terrain.Width;
            TerrainHeightBuffer = Terrain.Height; 
            RecalculatePopulation();
        }
    }

    void RecalculatePopulation()
    {
        int Population = Mathf.Min(Mathf.RoundToInt(TerrainWidthBuffer * TerrainHeightBuffer * SharksPerSquare), MaxSharks);

        if(Population > SharkPopulation.Count)
        {
            IncreasePopulation(Population);
        }
        else if(Population < SharkPopulation.Count)
        {
            DecreasePopulation(Population);
        }
    }

    void IncreasePopulation(int NewPopulation)
    {
        while (SharkPopulation.Count != NewPopulation)
        {
            GameObject NewShark = Instantiate(SharkPrefab, GetRandomLocation(), Quaternion.identity) as GameObject;
            NewShark.transform.parent = transform;
            SharkMovement Shark = NewShark.GetComponent<SharkMovement>();
            Shark.SharkController = this;
            SharkPopulation.Add(Shark);
        }
    }
    void DecreasePopulation(int NewPopulation)
    {
        while (SharkPopulation.Count != NewPopulation)
        {
            SharkMovement Shark = SharkPopulation[0];
            SharkPopulation.RemoveAt(0);
            Destroy(Shark.gameObject);
        }
    }

    public Vector3 GetRandomLocation()
    {
        Vector3 NewLocation = Vector3.zero;
        NewLocation.z = SharkLayer + Random.Range(0, SharkLayerWidth);
        NewLocation.x = Random.Range(0, TerrainWidthBuffer);
        NewLocation.y = Random.Range(0, TerrainHeightBuffer);

        return NewLocation;
    }
}
