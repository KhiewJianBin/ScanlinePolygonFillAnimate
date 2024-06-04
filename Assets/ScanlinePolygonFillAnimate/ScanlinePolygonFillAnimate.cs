using asim.unity.helpers;
using asim.unity.utils.geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class BoundaryFillAnimate : MonoBehaviour
{
    [SerializeField] Camera cam;

    /// <summary>
    /// Input Polygon is asuming points are arrange in order, no intersecting connection
    /// </summary>
    [SerializeField] GameObject Polygon;
    [SerializeField] bool IntegerStep = true;
    [SerializeField] float Step = 1;
    [SerializeField] float OutStepMargin = 0.1f;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(StartScanLine());
        }
    }

    Vector2 yLineP1;
    Vector2 yLineP2;
    List<Vector2> intersectionPoints = new List<Vector2>();
    List<Vector2> colorPoints = new List<Vector2>();
    IEnumerator StartScanLine()
    {
        List<Vector3> vertices = new List<Vector3>();

        //Step 0. Collect Vertices of polygon
        for (int i = 0; i < Polygon.transform.childCount; i++)
        {
            var vertex = Polygon.transform.GetChild(i).transform.position;
            vertices.Add(vertex);
        }
        //Step 0b. Get ymin and ymax of polygon
        var ymin = vertices.Min(v => v.y);
        var ymax = vertices.Max(v => v.y);

        //Step 1. Loop though ymin -> ymax, per define step(size) , calculate intersection
        for (float y = ymin; y < ymax + 1; y+= Step)
        {
            yLineP1 = new Vector2(-1000, y);
            yLineP2 = new Vector2(1000, y);

            yield return new WaitForSeconds(0.2f);

            intersectionPoints.Clear();
            Vector2 previntersectPoint = Vector2.negativeInfinity;
            for (int i = 0; i < vertices.Count; i++)
            {
                (Vector2 start, Vector2 end) line = (vertices[i], vertices[(i + 1) % vertices.Count]);

                (bool isParallel, bool isIntersect, Vector2 intersectPoint, Vector2 _) = GeometryUtils.IsLinesIntercept(yLineP1, yLineP2, line.start, line.end);

                if (!isParallel && isIntersect)
                {
                    //Special Case Handeling
                    // Case when intersection point is a vertex
                    // if the prevs point is the same as current point, means the point is a vertex,
                    // Check the prev line and current line if both ymin is the same
                    // if same, add again
                    if (previntersectPoint == intersectPoint)
                    {
                        (Vector2 start, Vector2 end) prevLine = (vertices[i-1], vertices[(i)]);
                        var prevLineymin = Mathf.Min(prevLine.start.y,prevLine.end.y);
                        var Lineymin = Mathf.Min(line.start.y, line.end.y);

                        var prevLineymax = Mathf.Max(prevLine.start.y, prevLine.end.y);
                        var Lineymax = Mathf.Max(line.start.y, line.end.y);

                        if (prevLineymin != Lineymin && prevLineymax != Lineymax)
                        {
                            continue;
                        }
                    }

                    intersectionPoints.Add(intersectPoint);

                    previntersectPoint = intersectPoint;
                }
            }

            //Step 2. Sort Intersection points
            intersectionPoints = intersectionPoints.OrderBy(p => p.x).ToList();

            for (int i = 0; i < intersectionPoints.Count; i += 2)
            {
                var startX = intersectionPoints[i].x;
                var endX = intersectionPoints[i + 1].x;
                if (IntegerStep) startX = Mathf.Floor(startX + OutStepMargin);
                for (float j = startX; j < endX + OutStepMargin; j += Step)
                {
                    colorPoints.Add(new Vector2(j, y));
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
    }

    
    void OnGUI()
    {
        if (!cam) return;

        for (int i = 0; i < Polygon.transform.childCount; i++)
        {
            Transform p1 = Polygon.transform.GetChild(i);
            Transform p2 = Polygon.transform.GetChild((i + 1) % Polygon.transform.childCount);

            //Convert World Pos to GUI Pos
            Vector3 p1pos = cam.WorldToScreenPoint(p1.position);
            Vector3 p2pos = cam.WorldToScreenPoint(p2.position);
            p1pos.y = UnityOnGUIHelper.Height - p1pos.y;
            p2pos.y = UnityOnGUIHelper.Height - p2pos.y;

            UnityOnGUIHelper.DrawLine(p1pos, p2pos, Color.red, 3);
        }

        //Convert World Pos to GUI Pos
        Vector3 yLinep1pos = cam.WorldToScreenPoint(yLineP1);
        Vector3 yLinep2pos = cam.WorldToScreenPoint(yLineP2);
        yLinep1pos.y = UnityOnGUIHelper.Height - yLinep1pos.y;
        yLinep2pos.y = UnityOnGUIHelper.Height - yLinep2pos.y;
        UnityOnGUIHelper.DrawLine(yLinep1pos, yLinep2pos, Color.black, 4);

        foreach (var ipoint in intersectionPoints)
        {
            Vector3 ipointPos = cam.WorldToScreenPoint(ipoint);
            ipointPos.y = UnityOnGUIHelper.Height - ipointPos.y;
            UnityOnGUIHelper.DrawDot(ipointPos, 8,0,Color.green, Color.green);
        }

        foreach (var cpoint in colorPoints)
        {
            Vector3 cpointPos = cam.WorldToScreenPoint(cpoint);
            cpointPos.y = UnityOnGUIHelper.Height - cpointPos.y;
            UnityOnGUIHelper.DrawDot(cpointPos, 16, 0, Color.blue, Color.blue);
        }
    }
}
