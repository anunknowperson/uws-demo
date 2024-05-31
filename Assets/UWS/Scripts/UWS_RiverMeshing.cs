using ClipperLib;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using Poly2Tri;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using System.Threading;

public class UWS_RiverMeshing : UWS_Meshing
{
    public override void GenerateMesh(UWS_WaterFactory factory, ConcurrentQueue<UWS_WaterFactoryResult> returnQueue, UWS_WaterObject obj, List<IntPoint> clip, UWS_WaterChunk chunk, List<UWS_WaterObject> objects, UWS_MeshingRequest request)
    {
        UWS_MeshingShape shape = new UWS_MeshingShape();

        shape.SetClip(clip);

        shape.AddGeometry(obj.GetPolygon());

        Paths toMerge = new Paths();

        foreach (UWS_WaterObject fobj in objects)
        {
            if (fobj is UWS_Lake)
            {
                shape.Merge(((UWS_Lake)fobj).GetPolygon());
            }
            else if (fobj is UWS_Ocean)
            {
                shape.Intersect(((UWS_Ocean)fobj).GetHole());
            }else if (fobj is UWS_Island) {
                shape.IntersectWithCheck(((UWS_Island)fobj).GetPolygon());
            } else if(fobj is UWS_River)
            {
                if (fobj != obj)
                {
                    shape.Merge(((UWS_River)fobj).GetPolygon());
                }
            }
        }

        Paths holesToCut = new Paths();

        foreach (UWS_WaterObject fobj in objects)
        {
            if (fobj is UWS_Island)
            {
                holesToCut.Add(((UWS_Island)fobj).GetPolygon());
            }
        }

        shape.AddHolesConditioned(holesToCut);

        Paths meshes;
        Dictionary<Path, Path> holes;

        shape.ReturnGeometry(out meshes, out holes);

        ProcessMeshing(factory, returnQueue, chunk, obj, meshes, holes, request);


        /*Path meshTempPath = obj.GetPolygon();

        Paths mesh = new Paths();
        
        Clipper c = new Clipper();
        c.AddPath(meshTempPath, PolyType.ptSubject, true);
        c.AddPath(clip, PolyType.ptClip, true);
        c.Execute(ClipType.ctIntersection, mesh);

        if (mesh.Count == 0)
        {
            Interlocked.Increment(ref factory.CompletedMeshes);
            return;
        }
        else
        {
            for (int i = 0; i < mesh.Count; i++)
            {
                ProcessOneSplit(mesh[i], ReturnQueue, obj, clip, chunk, objects);
            }

            Interlocked.Increment(ref factory.CompletedMeshes);*/
    }

    /*
    void ProcessOneSplit(Path m, ConcurrentQueue<UWS_WaterFactoryResult> returnQueue, UWS_WaterObject obj, List<IntPoint> clip, UWS_WaterChunk chunk, List<UWS_WaterObject> objects)
    {
        Paths mesh = new Paths();

        UWS_Ocean ocean = null;

        foreach (UWS_WaterObject fobj in objects)
        {
            if (fobj is UWS_Ocean)
            {
                ocean = (UWS_Ocean)fobj;
                break;
            }
        }

        Clipper c = new Clipper();

        c.AddPath(m, PolyType.ptSubject, true);
        c.AddPath(ocean.GetHole(), PolyType.ptClip, true);
        c.Execute(ClipType.ctIntersection, mesh);

        UWS_Lake lake;

        foreach (UWS_WaterObject fobj in objects)
        {
            if (fobj is UWS_Lake)
            {
                lake = (UWS_Lake)fobj;

                c.Clear();

                c.AddPaths(mesh, PolyType.ptSubject, true);
                c.AddPath(lake.GetPolygon(), PolyType.ptClip, true);
                c.Execute(ClipType.ctDifference, mesh);
            }
        }

        Shape meshShape = new Shape();

        List<Triangle> triangulationResult = new List<Triangle>();

        if (mesh.Count == 0)
        {
            return;
        }

        foreach (IntPoint intPoint in mesh[0])
        {
            TriPoint triPoint = new TriPoint(intPoint.X * 0.001, intPoint.Y * 0.001);
            meshShape.Points.Add(triPoint);
        }

        meshShape.Triangulate(triangulationResult);

        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        for (int i = 0; i < triangulationResult.Count; i++)
        {
            TriPoint v = triangulationResult[i].Points[0];

            vertices.Add(new Vector3((float)v.X, 0.0f, (float)v.Y));

            v = triangulationResult[i].Points[1];

            vertices.Add(new Vector3((float)v.X, 0.0f, (float)v.Y));

            v = triangulationResult[i].Points[2];

            vertices.Add(new Vector3((float)v.X, 0.0f, (float)v.Y));

            tris.Add(i * 3 + 2);
            tris.Add(i * 3 + 1);
            tris.Add(i * 3 + 0);
        }

        UWS_WaterFactoryResult factoryResult = new UWS_WaterFactoryResult();

        factoryResult.vertices = vertices;
        factoryResult.tris = tris;

        factoryResult.Chunk = chunk;

        returnQueue.Enqueue(factoryResult);
    }*/
}
