using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Town : MonoBehaviour
{
    [SerializeField] private GameObject[] GeneralBuildings;
    public float expansionRate = 1.0f;
    private float curTime = 0f;
    private float timeToExpand = 60f;
    private List<GameObject> buildings = new List<GameObject>();
    private List<Vector3> convexHull = new List<Vector3>();
    public float spacing = 2f;
    public float startSpacing = 5f;
    private LineRenderer lr;
    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        buildings.Add(gameObject);
    }
    private void Update()
    {
        curTime += Time.deltaTime * expansionRate;
        if (curTime >= timeToExpand)
            ExpandTown();
    }
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private static int orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        float val = (q.z - p.z) * (r.x - q.x) -
                (q.x - p.x) * (r.z - q.z);

        if (val == 0) return 0; // collinear
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }
    private void GenerateConvexHull()
    {
        convexHull = new List<Vector3>();
        Vector3[] points = new Vector3[buildings.Count];
        int l = -1;
        for (int i = 0; i < buildings.Count; i++)
        {
            points[i] = buildings[i].transform.position;
            if (l == -1)
                l = i;
            else if (points[i].x < points[l].x)
                l = i;

        }
        if (points.Length < 3) {
            for (int i = 0; i < points.Length; i++)
            {
                convexHull.Add(points[i]);
            }
            return; 
        }
        //Gift Wrapping
        int p = l;
        int q;
        int n = points.Length;
        do
        {
            convexHull.Add(points[p]);
            q = (p+1)%n;
            for (int i = 0; i < n; i++)
            {
                if (orientation(points[p], points[i], points[q]) == 2)
                    q = i;
            }
            p = q;
        } 
        while (p!=l);
        visualizeCVH();
    }
    private void visualizeCVH()
    {
        lr.enabled = true;
        lr.positionCount = convexHull.Count+1;
        for (int i = 0; i < convexHull.Count; i++)
        {
            lr.SetPosition(i,convexHull[i]);
        }
        lr.SetPosition(lr.positionCount-1,convexHull[0]);
    }
    private Vector3 calculateTownCenter()
    {
        Vector3 center = new Vector3();
        for (int i = 0; i < convexHull.Count; i++)
            center += convexHull[i];
        center /= convexHull.Count;
        return center;
    }
    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * Vector3.Normalize(B - A) + A;
        return P;
    }
    private void ExpandTown()
    {
        curTime = 0;
        GenerateConvexHull();
        Vector3 randomHullPoint;
        Vector3 dir;
        Vector3 newPosition;
        if (buildings.Count < 6)
        {
            dir = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)).normalized;
            randomHullPoint = (transform.position) + (dir*startSpacing);

        }
        else
        {
            int randomHP = Random.Range(0, convexHull.Count);
            randomHullPoint = LerpByDistance(convexHull[randomHP], convexHull[randomHP == convexHull.Count - 1 ? 0 : randomHP + 1],
                Random.Range(0.1f,(convexHull[randomHP] - convexHull[randomHP == convexHull.Count - 1 ? 0 : randomHP + 1]).magnitude));
            dir = (randomHullPoint - calculateTownCenter()).normalized; //Direction
        }
        dir.x += Random.Range(-1f, 1f);
        dir.z += Random.Range(-1f, 1f);
        newPosition = randomHullPoint + (dir * spacing);

        buildings.Add(Instantiate(GeneralBuildings[Random.Range(0,GeneralBuildings.Length)], 
            new Vector3(newPosition.x, 0, newPosition.z), 
            Quaternion.Euler(0, Random.Range(0f, 360f), 0)));
        GenerateConvexHull();
    }
}
