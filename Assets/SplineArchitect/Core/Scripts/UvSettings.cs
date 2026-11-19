// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: UvSettings.cs
//
// Author: Mikael Danielsson
// Date Created: 04-10-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace SplineArchitect
{
    [Serializable]
    public struct UvSettings
    {
        public Vector2 uvOffset;
        public Vector2 uv2Offset;
        public Vector2 uv3Offset;
        public Vector2 uv4Offset;
    }
}
