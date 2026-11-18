# `HandleRegistry.cs`
Namespace: `SplineArchitect`  

### `Overview`
`HandleRegistry` is a static class where all Splines and SplineConnectors register themselves.  
This allows you to access all splines and spline connectors in the currently open scene(s).

Destroyed Splines and SplineConnectors are unregistered, but disabled ones remain registered.

### `Class info`
Static class that manages all splines and spline connectors in the system.

### `Static Functions`
```csharp
// UNSAFE: Returns the live backing HashSet. Do NOT add, remove, or clear elements.
// Modifying this collection will break the internal state of the registry.
public static HashSet<Spline> GetSplinesUnsafe()

// Fills the provided list with all currently registered splines.
// The list is not cleared automatically.
public static void GetSplines(List<Spline> result)

// UNSAFE: Returns the live backing HashSet. Do NOT add, remove, or clear elements.
// Modifying this collection will break the internal state of the registry.
public static HashSet<SplineConnector> GetSplineConnectorsUnsafe()

// Fills the provided list with all currently registered spline connectors.
// The list is not cleared automatically.
public static void GetSplineConnectors(List<SplineConnector> result)

// Gets the total length of all registered splines.
public static float GetTotalLengthOfAllSplines()
```
<hr class="custom-line1">