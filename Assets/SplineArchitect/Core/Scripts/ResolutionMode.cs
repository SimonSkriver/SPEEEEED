// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: ResolutionMode.cs
//
// Author: Mikael Danielsson
// Date Created: 11-02-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

namespace SplineArchitect
{
    public enum ResolutionMode : byte
    {
        SUBDIVIDE_ALL_AXELS,
        SUBDIVIDE_Z_AXEL,
        SUBDIVIDE_X_AXEL,
        EDGE_LOOPS_ALONG_Z,
        EDGE_LOOPS_ALONG_X
    }
}