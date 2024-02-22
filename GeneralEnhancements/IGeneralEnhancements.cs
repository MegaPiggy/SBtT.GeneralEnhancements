using UnityEngine;

public interface IGeneralEnhancements
{
    /// <summary> Add a map to the advanced maps. Have the object/child objects have "KeepMat" in their name to not replace the material. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    /// <param name="map">The object to use for the map.</param>
    /// <param name="radius">The radius of this planet.</param>
    void AddAdvancedMap(string owrbName, GameObject map, float radius = 250f);

    /// <summary> Update an existing map. </summary>
    void UpdateAdvancedMap(string owrbName, GameObject map);

    void RemoveAdvancedMap(string owrbName);

    /// <summary> Instead of just using the simple sphere, also displays an "Error" text on this planet map. </summary>
    void AddErrorMap(string owrbName);

    bool isReady { get; }
}