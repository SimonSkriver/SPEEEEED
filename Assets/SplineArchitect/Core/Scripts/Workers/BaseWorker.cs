// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: BaseWorker.cs
//
// Author: Mikael Danielsson
// Date Created: 14-01-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

using SplineArchitect.Jobs;

namespace SplineArchitect.Workers
{
    public abstract class BaseWorker
    {
        public WorkerState workerState { get; protected set; }
        protected Spline spline;

        protected BaseWorker(Spline spline)
        {
            this.spline = spline;
            spline?.allWorkers.Add(this);
        }

        public void SetSpline(Spline spline)
        {
            this.spline?.allWorkers.Remove(this);
            this.spline = spline;
            spline.allWorkers.Add(this);
        }

        public abstract bool Start();
        public abstract void Complete();
        public abstract void CompleteWithoutAssignData();
        public abstract int GetWorkCount();
        public abstract void DisposeNativeData();

        protected DeformJob CreateDeformJob(int splineObjectCount,
                                  NativeArray<Vector3> vertices,
                                  NativeArray<Vector3> meshNormals,
                                  NativeArray<Vector4> meshTangents,
                                  NativeArray<RaycastCommand> raycastCommands,
                                  NativeArray<int> localSpaceMap,
                                  NativeArray<MapsDataDeformation> mapsData)
        {
            DeformJob deformJob = new DeformJob()
            {
                splineObjectCount = splineObjectCount,
                vertices = vertices,
                meshNormals = meshNormals,
                meshTangents = meshTangents,
                raycastCommands = raycastCommands,
                localSpaceMap = localSpaceMap,
                mapsData = mapsData,
                nativeSegments = spline.NativeSegmentsLocal,
                segmentSamples = spline.SegmentSamples,
                noises = spline.NativeNoises,
                splineLength = spline.Length,
                splineLocalToWorldMatrix = spline.transform.localToWorldMatrix,
                distanceMap = spline.DistanceMap,
                normals = spline.NormalsLocal,
                normalsMap = spline.NormalsMapLocal,
                positionMapLocal = spline.PositionMapLocal,
                splineResolution = spline.GetSplineResolution(),
                loop = spline.Loop,
            };

            return deformJob;
        }

        protected FollowerJob CreateFollowerJob(NativeArray<Vector3> newLocalPositions,
                                                NativeArray<Quaternion> newLocalRotations,
                                                NativeArray<Matrix4x4> newWorldMatrices,
                                                NativeArray<float> closestLinkZPositions,
                                                NativeArray<int> closestLinkIds,
                                                NativeArray<Vector3> localSplinePositions,
                                                NativeArray<Quaternion> localSplineRotations,
                                                NativeArray<Quaternion> combinedParentRotations,
                                                NativeHashMap<int, float4x4> localSpaces,
                                                NativeArray<int> localSpaceMap,
                                                NativeArray<bool> alignToEndMap,
                                                NativeArray<float> lockPositions,
                                                NativeArray<Vector3> rightDirs,
                                                NativeArray<Vector3> upDirs,
                                                NativeArray<Vector3> forwardDirs,
                                                NativeArray<RotationMode> rotationModes)
        {
            FollowerJob followerJob = new FollowerJob()
            {
                // Out data
                newLocalPositions = newLocalPositions,
                newLocalRotations = newLocalRotations,
                newWorldMatrices = newWorldMatrices,
                closestLinkZPositions = closestLinkZPositions,
                closestLinkIds = closestLinkIds,
                rightDirs = rightDirs,
                upDirs = upDirs,
                forwardDirs = forwardDirs,

                // In data
                splineLength = spline.Length,
                splineResolution = spline.GetSplineResolution(),
                loop = spline.Loop,
                matrix = spline.transform.localToWorldMatrix,
                distanceMap = spline.DistanceMap,
                positionMapLocal = spline.PositionMapLocal,
                normals = spline.NormalsLocal,
                normalsMap = spline.NormalsMapLocal,
                nativeSegments = spline.NativeSegmentsLocal,
                noises = spline.NativeNoises,
                segmentSamples = spline.SegmentSamples,
                localSpaces = localSpaces,
                localSpaceMap = localSpaceMap,
                alignToEndMap = alignToEndMap,
                lockPositionMap = lockPositions,
                localSplinePositions = localSplinePositions,
                combinedParentRotations = combinedParentRotations,
                localSplineRotations = localSplineRotations,
                rotationModeMap = rotationModes
            };

            return followerJob;
        }
    }
}
