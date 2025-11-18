using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

class RectangleCorners
{
    public readonly float minX = float.PositiveInfinity;
    public readonly float minY = float.PositiveInfinity;
    public readonly float maxX = float.NegativeInfinity;
    public readonly float maxY = float.NegativeInfinity;

    public RectangleCorners(Vector3[] corners)
    {
        if (corners.Length != 4)
            Debug.LogError($"Rectangle needs to have 4 corners, got {corners.Length}");


        foreach (Vector3 corner in corners)
        {
            if (corner.x >= maxX)
                maxX = corner.x;
            else if (corner.x < minX)
                minX = corner.x;

            if (corner.z > maxY)
                maxY = corner.z;
            else if (corner.z < minY)
                minY = corner.z;
        }
    }
}

public static class SearchGridPlanner
{
    public static List<Vector2> GenerateSearchGrid(Vector3[] searchCorners, float effectiveMowHeight, float effectiveMowWidth, Vector3 dronePos)
    {
        List<Vector2> searchGrid = new();
        RectangleCorners rectangle = new(searchCorners);

        float dx = rectangle.maxX - rectangle.minX - effectiveMowHeight;
        float dy = rectangle.maxY - rectangle.minY - effectiveMowHeight;

        // Bottom Left clockwise
        Vector2[] points = {
            new(rectangle.minX + effectiveMowHeight / 2, rectangle.minY + effectiveMowWidth / 2), // BL
            new(rectangle.minX + effectiveMowHeight / 2, rectangle.maxY - effectiveMowWidth / 2), // TL
            new(rectangle.maxX - effectiveMowHeight / 2, rectangle.maxY - effectiveMowWidth / 2), // TR
            new(rectangle.maxX - effectiveMowHeight / 2, rectangle.minY + effectiveMowWidth / 2), // BR
        };

        int closestPointIdx = GetClosestPointIdx(points, dronePos);
        Vector2 currPnt = points[closestPointIdx];

        float xInc = effectiveMowHeight;
        float yInc = effectiveMowWidth;

        int nx = (int)((dx - xInc) / xInc);
        int ny = (int)((dy - yInc) / yInc);

        // Down case at TL, TR
        if (closestPointIdx == 1 || closestPointIdx == 2)
            yInc *= -1;

        if (closestPointIdx == 3 || closestPointIdx == 2)
            xInc *= -1;

        // X major
        if (dx >= dy)
        {
            for (int y_idx = 0; y_idx <= ny; ++y_idx)
            {
                searchGrid.Add(currPnt);
                for (int x_idx = 0; x_idx <= nx; ++x_idx)
                {
                    currPnt += new Vector2(xInc, 0);
                    searchGrid.Add(currPnt);
                }

                xInc *= -1;
                currPnt += new Vector2(0, yInc);
            }
        }
        // Y major
        else
        {
            for (int x_idx = 0; x_idx <= nx; ++x_idx)
            {
                searchGrid.Add(currPnt);

                for (int y_idx = 0; y_idx <= ny; ++y_idx)
                {
                    currPnt += new Vector2(0, yInc);
                    searchGrid.Add(currPnt);
                }

                yInc *= -1;
                currPnt += new Vector2(xInc, 0);
            }
        }

        return searchGrid;
    }

    public static int GetClosestPointIdx(Vector2[] points, Vector3 target)
    {
        int closestPointIdx = -1;
        float closestDist = 10000;
        for (int i = 0; i < points.Length; ++i)
        {
            Vector2 point = points[i];
            float dist = Vector3.Distance(new Vector3(point.x, 0, point.y), target);

            if (closestPointIdx == -1 || dist < closestDist)
            {
                closestDist = dist;
                closestPointIdx = i;
            }
        }

        return closestPointIdx;
    }
}
