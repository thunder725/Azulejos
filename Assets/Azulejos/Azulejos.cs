using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using KModkit;

public class Azulejos : MonoBehaviour {

    [SerializeField] Texture2D[] masksArray;
    [SerializeField] GameObject[] tiles;
    bool mazeActive;
    bool moduleSolved;
    List<int> subgrid;
    List<int> forbidden = new List<int>();
    List<int> startingTiles = new List<int>();
    List<int> path = new List<int>();
    int[] inversions = {
        0, 0, 1, 0, 0, 0, 1,
        0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 1, 1,
        1, 0, 1, 0, 1, 0, 0,
        1, 0, 0, 0, 0, 0, 1,
        1, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 1, 0, 0, 1};
    int pathIndex;
    int startingTile;
    // Use this for initialization
    void Start()
    {
        GetComponent<KMSelectable>().OnDefocus += delegate { mazeActive = false; };
        int[] edgeworkConditions = new int[] { GetComponent<KMBombInfo>().GetBatteryCount() % 7, GetComponent<KMBombInfo>().GetPortCount() % 7 };
        startingTiles.AddRange(new int[] {(7 * edgeworkConditions[0]) + edgeworkConditions[1], (7 * edgeworkConditions[1]) + edgeworkConditions[0]} );
        int absoluteStartingTile = startingTiles.PickRandom();
        int xCoord = absoluteStartingTile % 7;
        int yCoord = absoluteStartingTile / 7;
        int subgridTop = Random.Range(Mathf.Max(0, yCoord - 4), Mathf.Min(2, yCoord));
        int subgridLeft = Random.Range(Mathf.Max(0, xCoord - 4), Mathf.Min(2, xCoord));
        subgrid = Enumerable.Range(0, 48).Where(x => x / 7 >= subgridTop && x / 7 < subgridTop + 5 && x % 7 >= subgridLeft && x % 7 < subgridLeft + 5).ToList();
        startingTile = subgrid.IndexOf(absoluteStartingTile);
        GenerateMaze();
        List<int> excluded = new List<int>();
        excluded.AddRange(startingTiles);
        excluded.AddRange(path.Select(x => subgrid[x]));
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
            Material _m = Instantiate(renderer.material);
            int gridNumber = path.Contains(i) ? subgrid[i] : forbidden.Contains(i) ? Enumerable.Range(0, 48).Where(x => x != subgrid[i] && !excluded.Contains(x)).PickRandom() : Enumerable.Range(0, 48).Where(x => !excluded.Contains(x)).PickRandom();
            excluded.Add(gridNumber);
            float inversionValue = Random.value;
            _m.SetTexture("_Mask", inversionValue <= 0.5 ? masksArray[gridNumber] : masksArray[7 * (6 - gridNumber / 7) + 6 - gridNumber % 7]);
            _m.SetFloat("_Invert", Math.Abs(inversions[gridNumber] - inversionValue));
            renderer.material = _m;
            KMSelectable selectable = tile.GetComponent<KMSelectable>();
            if (i == startingTile) selectable.OnInteract += delegate { if (!moduleSolved) { mazeActive = true; pathIndex = 1; } return false; };
            else selectable.OnInteract += delegate { if (!moduleSolved) { GetComponent<KMBombModule>().HandleStrike(); } return false; };
            int dummyplug = i;
            selectable.OnHighlight += delegate { MazeStep(dummyplug); };
        }
    }
    void GenerateMaze()
    {
        int previousTile = startingTile;
        int pathLength = Random.Range(7, 10);
        for (int i = 0; i < pathLength; i++) {
            path.Add(previousTile);
            if (Enumerable.Range(0, 24).Where(x => Adjacent(x, previousTile) && !forbidden.Contains(x) && !startingTiles.Select(y => subgrid.IndexOf(y)).Contains(x)).Count() == 0) return;
            int newTile = Enumerable.Range(0, 24).Where(x => Adjacent(x, previousTile) && !forbidden.Contains(x) && !startingTiles.Select(y => subgrid.IndexOf(y)).Contains(x)).PickRandom();
            forbidden.AddRange(Enumerable.Range(0, 24).Where(x => Adjacent(x, previousTile) && !path.Contains(x)));
            previousTile = newTile;
            }
    }


	void MazeStep (int position) {
		if (!moduleSolved && mazeActive)
        {
            if (position == path[pathIndex])
            {
                pathIndex++;
                if (pathIndex == path.Count)
                {
                    StartCoroutine(SolveAnim());
                }
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                mazeActive = false;
            }

        }
	}
    bool Adjacent(int position1, int position2)
    {
        return (position1 / 5 == position2 / 5 && (position1 % 5 == position2 % 5 + 1 || position1 % 5 == position2 % 5 - 1)) || (position1 % 5 == position2 % 5 && (position1 / 5 == position2 / 5 + 1 || position1 / 5 == position2 / 5 - 1));
    }
    IEnumerator SolveAnim()
    {
        yield return new WaitForSeconds(1.0f);
        GetComponent<KMBombModule>().HandlePass();
        moduleSolved = true;
    }
}
