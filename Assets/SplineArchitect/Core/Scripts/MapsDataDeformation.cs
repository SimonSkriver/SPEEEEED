// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: SnapData.cs
//
// Author: Mikael Danielsson
// Date Created: 10-10-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;
using Unity.Mathematics;

namespace SplineArchitect
{
    public struct MapsDataDeformation
    {
        public bool mirrorDeformation;
        public bool alignToEnd;
        public bool skipTangents;
        public bool createRayCastCommand;
        public SnapData snapData;
        public LayerMask layerMask;
        public NormalType normalType;
        public float4x4 localSpace;

        public MapsDataDeformation(bool mirrorDeformation, bool alignToEnd, bool skipTangents, bool createRayCastCommand, 
        SnapData snapData, LayerMask layerMask, NormalType normalType, float4x4 localSpace)
        {
            this.mirrorDeformation = mirrorDeformation;
            this.alignToEnd = alignToEnd;
            this.skipTangents = skipTangents;
            this.createRayCastCommand = createRayCastCommand;
            this.snapData = snapData;
            this.layerMask = layerMask;
            this.normalType = normalType;
            this.localSpace = localSpace;
        }
    }
}