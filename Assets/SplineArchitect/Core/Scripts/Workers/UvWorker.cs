// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: FollowerWorker.cs
//
// Author: Mikael Danielsson
// Date Created: 23-12-2025
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using SplineArchitect.Jobs;

namespace SplineArchitect.Workers
{
    internal class UvWorker : BaseWorker
    {
        public struct UvLayersData
        {
            public int vertexCount;
            public bool uv;
            public bool uv2;
            public bool uv3;
            public bool uv4;

            public UvLayersData(int vertexCount, bool uv, bool uv2, bool uv3, bool uv4)
            {
                this.vertexCount = vertexCount;
                this.uv = uv;
                this.uv2 = uv2;
                this.uv3 = uv3;
                this.uv4 = uv4;
            }
        }

        public const int MAX_VERTICES = 30000;

        private NativeArray<Vector2> uv;
        private NativeArray<Vector2> uv2;
        private NativeArray<Vector2> uv3;
        private NativeArray<Vector2> uv4;
        private NativeArray<int> uvMap;
        public NativeArray<UvSettings> uvSettingsMap;
        private JobHandle jobHandle;
        private UvJob uvJob;

        public int totalVertices { get; private set; }
        private int oldTotalVertices;
        private int oldMeshContainersCount;
        private List<MeshContainer> meshContainers;
        public List<UvLayersData> uvLayersData;

        public UvWorker(Spline spline) : base(spline)
        {
            meshContainers = new List<MeshContainer>();
            uvLayersData = new List<UvLayersData>();
        }

        public void Add(SplineObject so, bool updateUv, bool update2Uv, bool update3Uv, bool update4Uv)
        {
#if UNITY_EDITOR
            if (so.SplineParent == null)
            {
                Debug.LogWarning($"[Spline Architect] Tried to add SplineObject {so.name} with null splineParent.");
                return;
            }

            if(workerState == WorkerState.WORKING)
            {
                Debug.LogWarning($"UvWorker allready working! Can't add splineObject {so.name} to worker.");
                return;
            }

            if (so.Type != SplineObjectType.DEFORMATION)
            {
                Debug.LogWarning($"Can't add {so.name} so uv Worker becouse becouse it's not a deformation.");
                return;
            }
#endif

            MeshContainer mc = so.GetMeshContainerAtIndex(0);

            if(!mc.MeshComponentIsMeshFilter())
                return;

            int vertextCount = HandleCachedResources.FetchOriginVertices(mc).Length;
            meshContainers.Add(mc);
            uvLayersData.Add(new UvLayersData(vertextCount, updateUv, update2Uv, update3Uv, update4Uv));

            totalVertices += vertextCount;

            if (workerState == WorkerState.EMPTY)
                workerState = WorkerState.NOT_EMPTY;

            if (totalVertices > MAX_VERTICES)
                workerState = WorkerState.FULL;
        }

        public override bool Start()
        {
            if(workerState == WorkerState.WORKING)
                return false;

            EnsureNativeCapacity();
            int offset = 0;

            for(int i = 0; i < meshContainers.Count; i++)
            {
                MeshContainer mc = meshContainers[i];
                UvLayersData ultu = uvLayersData[i];

#if UNITY_EDITOR
                if(mc == null || !mc.MeshComponentIsMeshFilter())
                {
                    Reset();
                    Debug.LogWarning("[Spline Architect] Found invalid MeshContainer, uv job aborted!");
                    return false;
                }
#endif
                if(ultu.uv)
                {
                    Vector2[] uvContainer = HandleCachedResources.FetchOriginUv(mc);
                    NativeArray<Vector2>.Copy(uvContainer, 0, uv, offset, uvContainer.Length);
                }

                if(ultu.uv2)
                {
                    Vector2[] uv2Container = HandleCachedResources.FetchOriginUv2(mc);
                    NativeArray<Vector2>.Copy(uv2Container, 0, uv2, offset, uv2Container.Length);
                }

                if(ultu.uv3)
                {
                    Vector2[] uv3Container = HandleCachedResources.FetchOriginUv3(mc);
                    NativeArray<Vector2>.Copy(uv3Container, 0, uv3, offset, uv3Container.Length);
                }

                if(ultu.uv4)
                {
                    Vector2[] uv4Container = HandleCachedResources.FetchOriginUv4(mc);
                    NativeArray<Vector2>.Copy(uv4Container, 0, uv4, offset, uv4Container.Length);
                }

                offset += ultu.vertexCount;

                uvSettingsMap[i] = mc.GetUvSettings();
                uvMap[i] = offset;
            }

            uvJob = new UvJob
            {
                uv = uv,
                uv2 = uv2,
                uv3 = uv3,
                uv4 = uv4,
                meshContainersCount = meshContainers.Count,
                uvMap = uvMap,
                uvSettingsMap = uvSettingsMap
            };

            int batchCount = Mathf.Max(totalVertices / 1500, 1);
            jobHandle = uvJob.Schedule(totalVertices, batchCount);
            workerState = WorkerState.WORKING;

            return true;
        }

        public override void Complete()
        {
            if ((int)workerState > 1)
                Start();

            if (workerState != WorkerState.WORKING)
            {
                Reset();
                return;
            }

            jobHandle.Complete();
            int verticesId = 0;

            for (int i = 0; i < meshContainers.Count; i++)
            {
                MeshContainer mc = meshContainers[i];
                UvLayersData ultu = uvLayersData[i];
                Mesh instanceMesh = mc.GetInstanceMesh();

                if(ultu.uv)
                {
                    NativeArray<Vector2> uv = uvJob.uv.GetSubArray(verticesId, instanceMesh.vertexCount);
                    instanceMesh.SetUVs(0, uv);
                }

                if(ultu.uv2)
                {
                    NativeArray<Vector2> uv2 = uvJob.uv2.GetSubArray(verticesId, instanceMesh.vertexCount);
                    instanceMesh.SetUVs(1, uv2);
                }

                if(ultu.uv3)
                {
                    NativeArray<Vector2> uv3 = uvJob.uv3.GetSubArray(verticesId, instanceMesh.vertexCount);
                    instanceMesh.SetUVs(2, uv3);
                }

                if(ultu.uv4)
                {
                    NativeArray<Vector2> uv4 = uvJob.uv4.GetSubArray(verticesId, instanceMesh.vertexCount);
                    instanceMesh.SetUVs(3, uv4);
                }

                verticesId += ultu.vertexCount;
            }

            Reset();
        }

        public override void CompleteWithoutAssignData()
        {
            jobHandle.Complete();
            Reset();
        }

        public override int GetWorkCount()
        {
            return meshContainers.Count;
        }

        public override void DisposeNativeData()
        {
            if(uv.IsCreated)
            {
                uv.Dispose();
                uv2.Dispose();
                uv3.Dispose();
                uv4.Dispose();

                uvMap.Dispose();
                uvSettingsMap.Dispose();
            }
        }

        private void Reset()
        {
            meshContainers.Clear();
            uvLayersData.Clear();
            totalVertices = 0;
            workerState = WorkerState.EMPTY;
        }

        private void EnsureNativeCapacity()
        {
            if(totalVertices > oldTotalVertices || meshContainers.Count > oldMeshContainersCount || !uv.IsCreated)
            {
                oldTotalVertices = totalVertices;
                oldMeshContainersCount = meshContainers.Count;

                DisposeNativeData();

                uv = new NativeArray<Vector2>(totalVertices, Allocator.Persistent);
                uv2 = new NativeArray<Vector2>(totalVertices, Allocator.Persistent);
                uv3 = new NativeArray<Vector2>(totalVertices, Allocator.Persistent);
                uv4 = new NativeArray<Vector2>(totalVertices, Allocator.Persistent);

                uvMap = new NativeArray<int>(meshContainers.Count, Allocator.Persistent);
                uvSettingsMap = new NativeArray<UvSettings>(meshContainers.Count, Allocator.Persistent);
            }
        }
    }
}
