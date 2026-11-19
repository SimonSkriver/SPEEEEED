// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: MeshSubdivisionJob.cs
//
// Author: Mikael Danielsson
// Date Created: 05-03-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace SplineArchitect.Jobs
{
    [BurstCompile]
    public struct MeshSubdivisionJob : IJob
    {
        public struct Edge : IEquatable<Edge>
        {
            public int a;
            public int b;

            public Edge(int a, int b)
            {
                if (a < b)
                {
                    this.a = a;
                    this.b = b;
                }
                else
                {
                    this.a = b;
                    this.b = a;
                }
            }

            public bool Equals(Edge other)
            {
                return a == other.a && b == other.b;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (a * 73856093) ^ (b * 19349663);
                }
            }
        }
        public struct QuantizedPos : IEquatable<QuantizedPos>
        {
            public int x;
            public int y;
            public int z;

            public QuantizedPos(Vector3 p, float scale)
            {
                x = (int)math.round(p.x * scale);
                y = (int)math.round(p.y * scale);
                z = (int)math.round(p.z * scale);
            }

            public bool Equals(QuantizedPos other)
            {
                return x == other.x && y == other.y && z == other.z;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
                }
            }
        }
        public struct EdgeCut : IEquatable<EdgeCut>
        {
            public Edge edge;
            public int loop;

            public EdgeCut(Edge edge, int loop)
            {
                this.edge = edge;
                this.loop = loop;
            }

            public bool Equals(EdgeCut other)
            {
                return edge.Equals(other.edge) && loop == other.loop;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return edge.GetHashCode() ^ (loop * 83492791);
                }
            }
        }

        [ReadOnly] public IndexFormat indexFormat;
        [ReadOnly] public int iterations;
        [ReadOnly] public ResolutionMode resolutionMode;

        public NativeArray<uint> subMeshMap;
        public NativeArray<float> longestEdgeResult;
        public NativeArray<int> maxResolution;

        public NativeList<int> triangles;
        public NativeList<Vector3> vertices;

        public NativeList<Vector2> uv;
        public NativeList<Vector2> uv2;
        public NativeList<Vector2> uv3;
        public NativeList<Vector2> uv4;
        public NativeList<Vector2> uv5;
        public NativeList<Vector2> uv6;
        public NativeList<Vector2> uv7;
        public NativeList<Vector2> uv8;

        public NativeList<Vector3> normals;
        public NativeList<Vector4> tangents;

        public void Execute()
        {
            if(resolutionMode == ResolutionMode.EDGE_LOOPS_ALONG_Z)
            {
                AddEdgeLoops(2);
            }
            else if(resolutionMode == ResolutionMode.EDGE_LOOPS_ALONG_X)
            {
                AddEdgeLoops(0);
            }
            else
            {
                Subdivide();
            }
        }

        private void Subdivide()
        {
            const float weldScale = 100000f; // tolerance = 0.00001 units
            maxResolution[0] = -1;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                float longestEdge = 0f;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int vA = triangles[i + 0];
                    int vB = triangles[i + 1];
                    int vC = triangles[i + 2];

                    Vector3 a = vertices[vA];
                    Vector3 b = vertices[vB];
                    Vector3 c = vertices[vC];

                    if (resolutionMode == ResolutionMode.SUBDIVIDE_Z_AXEL)
                    {
                        a.x = 0f;
                        a.y = 0f;

                        b.x = 0f;
                        b.y = 0f;

                        c.x = 0f;
                        c.y = 0f;
                    }
                    else if (resolutionMode == ResolutionMode.SUBDIVIDE_X_AXEL)
                    {
                        a.y = 0f;
                        a.z = 0f;

                        b.y = 0f;
                        b.z = 0f;

                        c.y = 0f;
                        c.z = 0f;
                    }

                    float dAB = Vector3.Distance(a, b);
                    float dBC = Vector3.Distance(b, c);
                    float dCA = Vector3.Distance(c, a);

                    longestEdge = Mathf.Max(longestEdge, dAB);
                    longestEdge = Mathf.Max(longestEdge, dBC);
                    longestEdge = Mathf.Max(longestEdge, dCA);
                }

                longestEdgeResult[0] = longestEdge;

                // Build canonical welded vertex ids by quantized position.
                NativeArray<int> canonicalIndexPerVertex = new NativeArray<int>(vertices.Length, Allocator.Temp);
                NativeParallelHashMap<QuantizedPos, int> canonicalFromPos = new NativeParallelHashMap<QuantizedPos, int>(vertices.Length, Allocator.Temp);

                int nextCanonical = 0;
                for (int i = 0; i < vertices.Length; i++)
                {
                    QuantizedPos qp = new QuantizedPos(vertices[i], weldScale);

                    if (canonicalFromPos.ContainsKey(qp))
                    {
                        canonicalIndexPerVertex[i] = canonicalFromPos[qp];
                    }
                    else
                    {
                        canonicalFromPos.Add(qp, nextCanonical);
                        canonicalIndexPerVertex[i] = nextCanonical;
                        nextCanonical++;
                    }
                }

                NativeParallelHashSet<Edge> splitCanonicalEdges = new NativeParallelHashSet<Edge>(triangles.Length, Allocator.Temp);
                NativeParallelHashMap<Edge, int> realMidpointMap = new NativeParallelHashMap<Edge, int>(triangles.Length, Allocator.Temp);
                NativeList<int> newTriangles = new NativeList<int>(triangles.Length * 2, Allocator.Temp);

                // PASS 1: mark canonical edges to split
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int vA = triangles[i + 0];
                    int vB = triangles[i + 1];
                    int vC = triangles[i + 2];

                    Vector3 a = vertices[vA];
                    Vector3 b = vertices[vB];
                    Vector3 c = vertices[vC];

                    if (resolutionMode == ResolutionMode.SUBDIVIDE_Z_AXEL)
                    {
                        a.x = 0f;
                        a.y = 0f;

                        b.x = 0f;
                        b.y = 0f;

                        c.x = 0f;
                        c.y = 0f;
                    }
                    else if (resolutionMode == ResolutionMode.SUBDIVIDE_X_AXEL)
                    {
                        a.y = 0f;
                        a.z = 0f;

                        b.y = 0f;
                        b.z = 0f;

                        c.y = 0f;
                        c.z = 0f;
                    }

                    float dAB = Vector3.Distance(a, b);
                    float dBC = Vector3.Distance(b, c);
                    float dCA = Vector3.Distance(c, a);

                    int cA = canonicalIndexPerVertex[vA];
                    int cB = canonicalIndexPerVertex[vB];
                    int cC = canonicalIndexPerVertex[vC];

                    int longestC1 = cA;
                    int longestC2 = cB;
                    float longestD = dAB;

                    if (dBC > longestD)
                    {
                        longestC1 = cB;
                        longestC2 = cC;
                        longestD = dBC;
                    }

                    if (dCA > longestD)
                    {
                        longestC1 = cC;
                        longestC2 = cA;
                        longestD = dCA;
                    }

                    if (longestD >= longestEdge * 0.5f)
                        splitCanonicalEdges.Add(new Edge(longestC1, longestC2));
                }

                NativeParallelHashSet<Edge> splitRealEdges = new NativeParallelHashSet<Edge>(triangles.Length, Allocator.Temp);

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int A = triangles[i + 0];
                    int B = triangles[i + 1];
                    int C = triangles[i + 2];

                    Edge realAB = new Edge(A, B);
                    Edge realBC = new Edge(B, C);
                    Edge realCA = new Edge(C, A);

                    Edge canAB = new Edge(canonicalIndexPerVertex[A], canonicalIndexPerVertex[B]);
                    Edge canBC = new Edge(canonicalIndexPerVertex[B], canonicalIndexPerVertex[C]);
                    Edge canCA = new Edge(canonicalIndexPerVertex[C], canonicalIndexPerVertex[A]);

                    if (splitCanonicalEdges.Contains(canAB))
                        splitRealEdges.Add(realAB);

                    if (splitCanonicalEdges.Contains(canBC))
                        splitRealEdges.Add(realBC);

                    if (splitCanonicalEdges.Contains(canCA))
                        splitRealEdges.Add(realCA);
                }

                int vertexLimit = indexFormat == IndexFormat.UInt16 ? 65536 : 262144;
                if (vertices.Length + splitRealEdges.Count() > vertexLimit)
                {
                    splitRealEdges.Dispose();
                    newTriangles.Dispose();
                    realMidpointMap.Dispose();
                    splitCanonicalEdges.Dispose();
                    canonicalFromPos.Dispose();
                    canonicalIndexPerVertex.Dispose();
                    maxResolution[0] = iteration;
                    break;
                }

                // PASS 2 + 3: rebuild triangles, creating real midpoints on demand
                int subMeshStart = 0;

                for (int subMesh = 0; subMesh < subMeshMap.Length; subMesh++)
                {
                    int oldSubMeshIndexCount = (int)subMeshMap[subMesh];
                    int newSubMeshStart = newTriangles.Length;

                    for (int t = 0; t < oldSubMeshIndexCount; t += 3)
                    {
                        int triIndex = subMeshStart + t;

                        int A = triangles[triIndex + 0];
                        int B = triangles[triIndex + 1];
                        int C = triangles[triIndex + 2];

                        Edge realAB = new Edge(A, B);
                        Edge realBC = new Edge(B, C);
                        Edge realCA = new Edge(C, A);

                        Edge canAB = new Edge(canonicalIndexPerVertex[A], canonicalIndexPerVertex[B]);
                        Edge canBC = new Edge(canonicalIndexPerVertex[B], canonicalIndexPerVertex[C]);
                        Edge canCA = new Edge(canonicalIndexPerVertex[C], canonicalIndexPerVertex[A]);

                        bool splitAB = splitCanonicalEdges.Contains(canAB);
                        bool splitBC = splitCanonicalEdges.Contains(canBC);
                        bool splitCA = splitCanonicalEdges.Contains(canCA);

                        int splitCount = 0;
                        if (splitAB) splitCount++;
                        if (splitBC) splitCount++;
                        if (splitCA) splitCount++;

                        if (splitCount == 0)
                        {
                            AddTriangle(ref newTriangles, A, B, C);
                        }
                        else
                        {

                            int mAB = splitAB ? GetOrCreateMidpoint(realAB, ref realMidpointMap) : -1;
                            int mBC = splitBC ? GetOrCreateMidpoint(realBC, ref realMidpointMap) : -1;
                            int mCA = splitCA ? GetOrCreateMidpoint(realCA, ref realMidpointMap) : -1;


                            if (splitCount == 1)
                            {
                                if (splitAB)
                                {
                                    AddTriangle(ref newTriangles, A, mAB, C);
                                    AddTriangle(ref newTriangles, mAB, B, C);
                                }
                                else if (splitBC)
                                {
                                    AddTriangle(ref newTriangles, B, mBC, A);
                                    AddTriangle(ref newTriangles, mBC, C, A);
                                }
                                else
                                {
                                    AddTriangle(ref newTriangles, C, mCA, B);
                                    AddTriangle(ref newTriangles, mCA, A, B);
                                }
                            }
                            else if (splitCount == 2)
                            {
                                if (splitAB && splitBC)
                                {
                                    AddTriangle(ref newTriangles, A, mAB, C);
                                    AddTriangle(ref newTriangles, mAB, mBC, C);
                                    AddTriangle(ref newTriangles, mAB, B, mBC);
                                }
                                else if (splitBC && splitCA)
                                {
                                    AddTriangle(ref newTriangles, B, mBC, A);
                                    AddTriangle(ref newTriangles, mBC, mCA, A);
                                    AddTriangle(ref newTriangles, mBC, C, mCA);
                                }
                                else
                                {
                                    AddTriangle(ref newTriangles, C, mCA, B);
                                    AddTriangle(ref newTriangles, mCA, mAB, B);
                                    AddTriangle(ref newTriangles, mCA, A, mAB);
                                }
                            }
                            else
                            {
                                AddTriangle(ref newTriangles, A, mAB, mCA);
                                AddTriangle(ref newTriangles, mAB, B, mBC);
                                AddTriangle(ref newTriangles, mCA, mBC, C);
                                AddTriangle(ref newTriangles, mAB, mBC, mCA);
                            }
                        }
                    }

                    subMeshMap[subMesh] = (uint)(newTriangles.Length - newSubMeshStart);
                    subMeshStart += oldSubMeshIndexCount;
                }

                triangles.Clear();
                triangles.AddRange(newTriangles.AsArray());
                newTriangles.Dispose();
                realMidpointMap.Dispose();
                splitCanonicalEdges.Dispose();
                canonicalFromPos.Dispose();
                canonicalIndexPerVertex.Dispose();
            }
        }

        private void AddEdgeLoops(int axis)
        {
            maxResolution[0] = -1;

            if (iterations <= 0 || vertices.Length == 0)
                return;

            float minPosition = GetAxis(vertices[0], axis);
            float maxPosition = GetAxis(vertices[0], axis);
            for (int i = 1; i < vertices.Length; i++)
            {
                minPosition = Mathf.Min(minPosition, GetAxis(vertices[i], axis));
                maxPosition = Mathf.Max(maxPosition, GetAxis(vertices[i], axis));
            }

            float length = maxPosition - minPosition;
            if (length < 0.00001f)
                return;

            // Vertices this close to an edge loop are treated as being on it, so already existing edge loops are reused.
            float loopEpsilon = length * 0.00001f;
            int vertexLimit = indexFormat == IndexFormat.UInt16 ? 65536 : 262144;
            int triangleCount = triangles.Length / 3;
            int originalVertexCount = vertices.Length;

            NativeArray<int> triangleSubMesh = new NativeArray<int>(triangleCount, Allocator.Temp);
            int subMeshTriangleStart = 0;
            for (int subMesh = 0; subMesh < subMeshMap.Length; subMesh++)
            {
                int subMeshTriangleCount = (int)subMeshMap[subMesh] / 3;
                for (int t = 0; t < subMeshTriangleCount; t++)
                    triangleSubMesh[subMeshTriangleStart + t] = subMesh;

                subMeshTriangleStart += subMeshTriangleCount;
            }

            // Flat triangle pairs forming a convex quad are cut as one polygon. The diagonal between them is not cut,
            // so the quad gets 2 triangles between each edge loop instead of extra vertices along the diagonal.
            NativeArray<int> pairs = new NativeArray<int>(triangleCount, Allocator.Temp);
            PairTrianglesIntoQuads(pairs, triangleSubMesh);
            triangleSubMesh.Dispose();

            // All edges that can be cut by an edge loop, every edge except the quad diagonals.
            NativeParallelHashSet<Edge> outlineEdgeSet = new NativeParallelHashSet<Edge>(triangles.Length, Allocator.Temp);
            for (int t = 0; t < triangleCount; t++)
            {
                for (int e = 0; e < 3; e++)
                {
                    int u = triangles[t * 3 + e];
                    int v = triangles[t * 3 + (e + 1) % 3];

                    if (pairs[t] != -1 && TriangleHasVertex(pairs[t], u) && TriangleHasVertex(pairs[t], v))
                        continue;

                    outlineEdgeSet.Add(new Edge(u, v));
                }
            }

            NativeArray<Edge> outlineEdges = outlineEdgeSet.ToNativeArray(Allocator.Temp);
            outlineEdgeSet.Dispose();

            // Each edge loop crossing an edge adds one vertex. Use fewer edge loops if the vertex limit is reached, so they stay evenly spaced.
            NativeList<float> loopPositions = new NativeList<float>(iterations, Allocator.Temp);
            int newVertexCount = 0;
            for (int loopCount = iterations; loopCount > 0; loopCount--)
            {
                // Resolution 1 is at 1/2, resolution 2 at 1/3 and 2/3 and so on.
                loopPositions.Clear();
                for (int i = 0; i < loopCount; i++)
                    loopPositions.Add(minPosition + length * (i + 1) / (loopCount + 1));

                newVertexCount = 0;
                for (int i = 0; i < outlineEdges.Length; i++)
                    newVertexCount += CountCrossingLoops(outlineEdges[i], loopPositions, loopEpsilon, axis);

                if (originalVertexCount + newVertexCount <= vertexLimit)
                    break;

                maxResolution[0] = loopCount - 1;
                loopPositions.Clear();
            }

            outlineEdges.Dispose();

            if (loopPositions.Length == 0)
            {
                loopPositions.Dispose();
                pairs.Dispose();
                return;
            }

            // The slabs between the edge loops each vertex is in. x is the lowest and y the highest, vertices on an edge loop are in two.
            NativeArray<int2> vertexSlabs = new NativeArray<int2>(originalVertexCount, Allocator.Temp);
            for (int i = 0; i < originalVertexCount; i++)
                vertexSlabs[i] = GetSlabs(GetAxis(vertices[i], axis), loopPositions, loopEpsilon);

            NativeParallelHashMap<EdgeCut, int> cutVertexMap = new NativeParallelHashMap<EdgeCut, int>(math.max(1, newVertexCount), Allocator.Temp);
            NativeList<int> newTriangles = new NativeList<int>(triangles.Length + newVertexCount * 6, Allocator.Temp);
            NativeList<int> polygon = new NativeList<int>(4, Allocator.Temp);
            NativeList<int> outline = new NativeList<int>(16, Allocator.Temp);
            NativeList<int2> outlineSlabs = new NativeList<int2>(16, Allocator.Temp);
            NativeList<int> slabPolygon = new NativeList<int>(16, Allocator.Temp);

            int subMeshStart = 0;

            for (int subMesh = 0; subMesh < subMeshMap.Length; subMesh++)
            {
                int oldSubMeshIndexCount = (int)subMeshMap[subMesh];
                int newSubMeshStart = newTriangles.Length;

                for (int i = 0; i < oldSubMeshIndexCount; i += 3)
                {
                    int t = (subMeshStart + i) / 3;
                    int pair = pairs[t];

                    // Already added together with its pair.
                    if (pair != -1 && pair < t)
                        continue;

                    polygon.Clear();
                    if (pair == -1)
                    {
                        polygon.Add(triangles[t * 3 + 0]);
                        polygon.Add(triangles[t * 3 + 1]);
                        polygon.Add(triangles[t * 3 + 2]);
                    }
                    else
                    {
                        TryGetQuad(t, pair, out int4 quad);
                        polygon.Add(quad.x);
                        polygon.Add(quad.y);
                        polygon.Add(quad.z);
                        polygon.Add(quad.w);
                    }

                    int lowestSlab = int.MaxValue;
                    int highestSlab = int.MinValue;
                    for (int p = 0; p < polygon.Length; p++)
                    {
                        int2 slabs = vertexSlabs[polygon[p]];
                        lowestSlab = math.min(lowestSlab, slabs.y);
                        highestSlab = math.max(highestSlab, slabs.x);
                    }

                    // Not crossed by any edge loop.
                    if (lowestSlab >= highestSlab)
                    {
                        AddTriangle(ref newTriangles, triangles[t * 3 + 0], triangles[t * 3 + 1], triangles[t * 3 + 2]);
                        if (pair != -1)
                            AddTriangle(ref newTriangles, triangles[pair * 3 + 0], triangles[pair * 3 + 1], triangles[pair * 3 + 2]);
                        continue;
                    }

                    // Walk the outline and add a vertex where each edge loop crosses it.
                    outline.Clear();
                    outlineSlabs.Clear();
                    for (int p = 0; p < polygon.Length; p++)
                    {
                        int vA = polygon[p];
                        int vB = polygon[(p + 1) % polygon.Length];
                        float posA = GetAxis(vertices[vA], axis);
                        float posB = GetAxis(vertices[vB], axis);

                        outline.Add(vA);
                        outlineSlabs.Add(vertexSlabs[vA]);

                        for (int l = 0; l < loopPositions.Length; l++)
                        {
                            // Edge loops are sorted by position, walk them in the same direction as the edge.
                            int loop = posA < posB ? l : loopPositions.Length - 1 - l;
                            if (!IsCrossingLoop(posA, posB, loopPositions[loop], loopEpsilon))
                                continue;

                            outline.Add(GetOrCreateCutVertex(vA, vB, loop, loopPositions[loop], axis, ref cutVertexMap));
                            outlineSlabs.Add(new int2(loop, loop + 1));
                        }
                    }

                    // The part of the polygon in each slab between two edge loops is a convex polygon of its own.
                    for (int s = lowestSlab; s <= highestSlab; s++)
                    {
                        slabPolygon.Clear();
                        for (int o = 0; o < outline.Length; o++)
                        {
                            if (outlineSlabs[o].x <= s && s <= outlineSlabs[o].y)
                                slabPolygon.Add(outline[o]);
                        }

                        TriangulateConvexPolygon(ref newTriangles, slabPolygon);
                    }
                }

                subMeshMap[subMesh] = (uint)(newTriangles.Length - newSubMeshStart);
                subMeshStart += oldSubMeshIndexCount;
            }

            triangles.Clear();
            triangles.AddRange(newTriangles.AsArray());

            // Longest edge along the loop axis.
            float longestEdge = 0f;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                float posA = GetAxis(vertices[triangles[i + 0]], axis);
                float posB = GetAxis(vertices[triangles[i + 1]], axis);
                float posC = GetAxis(vertices[triangles[i + 2]], axis);

                longestEdge = Mathf.Max(longestEdge, Mathf.Abs(posA - posB));
                longestEdge = Mathf.Max(longestEdge, Mathf.Abs(posB - posC));
                longestEdge = Mathf.Max(longestEdge, Mathf.Abs(posC - posA));
            }

            longestEdgeResult[0] = longestEdge;

            slabPolygon.Dispose();
            outlineSlabs.Dispose();
            outline.Dispose();
            polygon.Dispose();
            newTriangles.Dispose();
            cutVertexMap.Dispose();
            vertexSlabs.Dispose();
            loopPositions.Dispose();
            pairs.Dispose();
        }

        private void PairTrianglesIntoQuads(NativeArray<int> pairs, NativeArray<int> triangleSubMesh)
        {
            NativeParallelMultiHashMap<Edge, int> edgeTriangles = new NativeParallelMultiHashMap<Edge, int>(triangles.Length, Allocator.Temp);

            for (int t = 0; t < pairs.Length; t++)
            {
                pairs[t] = -1;
                edgeTriangles.Add(new Edge(triangles[t * 3 + 0], triangles[t * 3 + 1]), t);
                edgeTriangles.Add(new Edge(triangles[t * 3 + 1], triangles[t * 3 + 2]), t);
                edgeTriangles.Add(new Edge(triangles[t * 3 + 2], triangles[t * 3 + 0]), t);
            }

            for (int t = 0; t < pairs.Length; t++)
            {
                if (pairs[t] != -1)
                    continue;

                int best = -1;
                float bestLength = 0f;

                for (int e = 0; e < 3; e++)
                {
                    int u = triangles[t * 3 + e];
                    int v = triangles[t * 3 + (e + 1) % 3];

                    int other = GetOtherTriangle(new Edge(u, v), t, ref edgeTriangles);
                    if (other == -1 || pairs[other] != -1 || triangleSubMesh[other] != triangleSubMesh[t])
                        continue;

                    if (!TryGetQuad(t, other, out int4 quad) || !IsFlatConvexQuad(t, other, quad))
                        continue;

                    // The quad diagonal is the longest shared edge.
                    float edgeLength = Vector3.SqrMagnitude(vertices[u] - vertices[v]);
                    if (edgeLength > bestLength)
                    {
                        best = other;
                        bestLength = edgeLength;
                    }
                }

                if (best != -1)
                {
                    pairs[t] = best;
                    pairs[best] = t;
                }
            }

            edgeTriangles.Dispose();
        }

        private int GetOtherTriangle(Edge edge, int triangle, ref NativeParallelMultiHashMap<Edge, int> edgeTriangles)
        {
            // Only edges shared by exactly two triangles.
            int other = -1;
            int count = 0;

            if (edgeTriangles.TryGetFirstValue(edge, out int value, out NativeParallelMultiHashMapIterator<Edge> iterator))
            {
                do
                {
                    count++;
                    if (value != triangle)
                        other = value;
                }
                while (edgeTriangles.TryGetNextValue(out value, ref iterator));
            }

            return count == 2 ? other : -1;
        }

        private bool TryGetQuad(int triangle, int other, out int4 quad)
        {
            // The outline of both triangles in their winding order, without the shared edge.
            for (int e = 0; e < 3; e++)
            {
                int u = triangles[triangle * 3 + e];
                int v = triangles[triangle * 3 + (e + 1) % 3];
                int w = triangles[triangle * 3 + (e + 2) % 3];

                for (int j = 0; j < 3; j++)
                {
                    if (triangles[other * 3 + j] == v && triangles[other * 3 + (j + 1) % 3] == u)
                    {
                        quad = new int4(v, w, u, triangles[other * 3 + (j + 2) % 3]);
                        return true;
                    }
                }
            }

            quad = int4.zero;
            return false;
        }

        private bool IsFlatConvexQuad(int triangle, int other, int4 quad)
        {
            Vector3 normal = GetTriangleNormal(triangle);
            if (Vector3.Dot(normal, GetTriangleNormal(other)) < 0.9999f)
                return false;

            // Every corner needs to turn the same way and not be close to a straight line.
            for (int i = 0; i < 4; i++)
            {
                Vector3 a = vertices[quad[i]];
                Vector3 b = vertices[quad[(i + 1) % 4]];
                Vector3 c = vertices[quad[(i + 2) % 4]];
                Vector3 ab = b - a;
                Vector3 bc = c - b;

                if (Vector3.Dot(Vector3.Cross(ab, bc), normal) <= 0.01f * ab.magnitude * bc.magnitude)
                    return false;
            }

            // The texture mapping only stays the same after retriangulating if the uvs are linear over the whole quad.
            float3 barycentric = GetBarycentric(vertices[quad.x], vertices[quad.y], vertices[quad.z], vertices[quad.w]);

            return IsUvLinear(uv, quad, barycentric) && IsUvLinear(uv2, quad, barycentric) &&
                   IsUvLinear(uv3, quad, barycentric) && IsUvLinear(uv4, quad, barycentric) &&
                   IsUvLinear(uv5, quad, barycentric) && IsUvLinear(uv6, quad, barycentric) &&
                   IsUvLinear(uv7, quad, barycentric) && IsUvLinear(uv8, quad, barycentric);
        }

        private static bool IsUvLinear(NativeList<Vector2> uvs, int4 quad, float3 barycentric)
        {
            if (uvs.Length == 0)
                return true;

            Vector2 predicted = uvs[quad.x] * barycentric.x + uvs[quad.y] * barycentric.y + uvs[quad.z] * barycentric.z;
            return (predicted - uvs[quad.w]).sqrMagnitude < 0.000001f;
        }

        private static float3 GetBarycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;

            float y = (d11 * d20 - d01 * d21) / denom;
            float z = (d00 * d21 - d01 * d20) / denom;
            return new float3(1f - y - z, y, z);
        }

        private Vector3 GetTriangleNormal(int triangle)
        {
            Vector3 a = vertices[triangles[triangle * 3 + 0]];
            Vector3 b = vertices[triangles[triangle * 3 + 1]];
            Vector3 c = vertices[triangles[triangle * 3 + 2]];

            return Vector3.Cross(b - a, c - a).normalized;
        }

        private bool TriangleHasVertex(int triangle, int vertex)
        {
            return triangles[triangle * 3 + 0] == vertex || triangles[triangle * 3 + 1] == vertex || triangles[triangle * 3 + 2] == vertex;
        }

        private static bool IsCrossingLoop(float a, float b, float loopPosition, float loopEpsilon)
        {
            return loopPosition > math.min(a, b) + loopEpsilon && loopPosition < math.max(a, b) - loopEpsilon;
        }

        private int CountCrossingLoops(Edge edge, NativeList<float> loopPositions, float loopEpsilon, int axis)
        {
            float posA = GetAxis(vertices[edge.a], axis);
            float posB = GetAxis(vertices[edge.b], axis);
            int count = 0;

            for (int i = 0; i < loopPositions.Length; i++)
            {
                if (IsCrossingLoop(posA, posB, loopPositions[i], loopEpsilon))
                    count++;
            }

            return count;
        }

        private static int2 GetSlabs(float value, NativeList<float> loopPositions, float loopEpsilon)
        {
            int below = 0;
            bool onLoop = false;

            for (int i = 0; i < loopPositions.Length; i++)
            {
                if (loopPositions[i] < value - loopEpsilon) below++;
                else if (loopPositions[i] <= value + loopEpsilon) onLoop = true;
            }

            return new int2(below, onLoop ? below + 1 : below);
        }

        private void TriangulateConvexPolygon(ref NativeList<int> triList, NativeList<int> polygon)
        {
            if (polygon.Length < 3)
                return;

            if (polygon.Length == 4)
            {
                // Split along the shortest diagonal.
                if (Vector3.SqrMagnitude(vertices[polygon[0]] - vertices[polygon[2]]) <= Vector3.SqrMagnitude(vertices[polygon[1]] - vertices[polygon[3]]))
                {
                    AddTriangle(ref triList, polygon[0], polygon[1], polygon[2]);
                    AddTriangle(ref triList, polygon[0], polygon[2], polygon[3]);
                }
                else
                {
                    AddTriangle(ref triList, polygon[0], polygon[1], polygon[3]);
                    AddTriangle(ref triList, polygon[1], polygon[2], polygon[3]);
                }

                return;
            }

            for (int i = 1; i < polygon.Length - 1; i++)
                AddTriangle(ref triList, polygon[0], polygon[i], polygon[i + 1]);
        }

        private int GetOrCreateCutVertex(int vA, int vB, int loop, float cut, int axis, ref NativeParallelHashMap<EdgeCut, int> cutVertexMap)
        {
            EdgeCut edgeCut = new EdgeCut(new Edge(vA, vB), loop);
            if (cutVertexMap.TryGetValue(edgeCut, out int existing))
                return existing;

            // Always interpolate from the lowest position on the loop axis, so duplicated vertices (uv seams, hard edges) get the exact same position.
            int low = GetAxis(vertices[vA], axis) < GetAxis(vertices[vB], axis) ? vA : vB;
            int high = low == vA ? vB : vA;
            float t = (cut - GetAxis(vertices[low], axis)) / (GetAxis(vertices[high], axis) - GetAxis(vertices[low], axis));

            int vNew = vertices.Length;
            cutVertexMap.Add(edgeCut, vNew);

            Vector3 position = Vector3.Lerp(vertices[low], vertices[high], t);
            SetAxis(ref position, axis, cut);
            vertices.Add(position);

            if (uv.Length > 0) uv.Add(Vector2.Lerp(uv[low], uv[high], t));
            if (uv2.Length > 0) uv2.Add(Vector2.Lerp(uv2[low], uv2[high], t));
            if (uv3.Length > 0) uv3.Add(Vector2.Lerp(uv3[low], uv3[high], t));
            if (uv4.Length > 0) uv4.Add(Vector2.Lerp(uv4[low], uv4[high], t));
            if (uv5.Length > 0) uv5.Add(Vector2.Lerp(uv5[low], uv5[high], t));
            if (uv6.Length > 0) uv6.Add(Vector2.Lerp(uv6[low], uv6[high], t));
            if (uv7.Length > 0) uv7.Add(Vector2.Lerp(uv7[low], uv7[high], t));
            if (uv8.Length > 0) uv8.Add(Vector2.Lerp(uv8[low], uv8[high], t));

            if (normals.Length > 0)
                normals.Add(Vector3.Lerp(normals[low], normals[high], t).normalized);

            if (tangents.Length > 0)
            {
                Vector4 tangent = Vector4.Lerp(tangents[low], tangents[high], t);
                Vector3 xyz = new Vector3(tangent.x, tangent.y, tangent.z).normalized;
                tangents.Add(new Vector4(xyz.x, xyz.y, xyz.z, tangent.w >= 0f ? 1f : -1f));
            }

            return vNew;
        }

        private static float GetAxis(Vector3 vector, int axis)
        {
            return axis == 0 ? vector.x : vector.z;
        }

        private static void SetAxis(ref Vector3 vector, int axis, float value)
        {
            if (axis == 0) vector.x = value;
            else vector.z = value;
        }

        private int GetOrCreateMidpoint(Edge realEdge, ref NativeParallelHashMap<Edge, int> realMidpointMap)
        {
            if (realMidpointMap.ContainsKey(realEdge))
                return realMidpointMap[realEdge];

            int vA = realEdge.a;
            int vB = realEdge.b;

            int vNew = vertices.Length;
            realMidpointMap.Add(realEdge, vNew);

            vertices.Add((vertices[vA] + vertices[vB]) * 0.5f);

            if (uv.Length > 0) uv.Add((uv[vA] + uv[vB]) * 0.5f);
            if (uv2.Length > 0) uv2.Add((uv2[vA] + uv2[vB]) * 0.5f);
            if (uv3.Length > 0) uv3.Add((uv3[vA] + uv3[vB]) * 0.5f);
            if (uv4.Length > 0) uv4.Add((uv4[vA] + uv4[vB]) * 0.5f);
            if (uv5.Length > 0) uv5.Add((uv5[vA] + uv5[vB]) * 0.5f);
            if (uv6.Length > 0) uv6.Add((uv6[vA] + uv6[vB]) * 0.5f);
            if (uv7.Length > 0) uv7.Add((uv7[vA] + uv7[vB]) * 0.5f);
            if (uv8.Length > 0) uv8.Add((uv8[vA] + uv8[vB]) * 0.5f);

            if (normals.Length > 0)
                normals.Add(((normals[vA] + normals[vB]) * 0.5f).normalized);

            if (tangents.Length > 0)
            {
                Vector4 t = (tangents[vA] + tangents[vB]) * 0.5f;
                Vector3 xyz = new Vector3(t.x, t.y, t.z).normalized;
                tangents.Add(new Vector4(xyz.x, xyz.y, xyz.z, t.w >= 0f ? 1f : -1f));
            }

            return vNew;
        }

        private static void AddTriangle(ref NativeList<int> triList, int a, int b, int c)
        {
            triList.Add(a);
            triList.Add(b);
            triList.Add(c);
        }
    }
}
