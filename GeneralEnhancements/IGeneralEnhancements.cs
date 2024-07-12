using UnityEngine;

public interface IGeneralEnhancements
{
    /// <summary> Add a map to the advanced maps. Have the object/child objects have "KeepMat" in their name to not replace the material. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    /// <param name="map">The object to use for the map.</param>
    /// <param name="radius">The radius of this planet.</param>
    void AddAdvancedMap(string owrbName, GameObject map, float radius = 250f);

    /// <summary> Get an existing map. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    /// <returns>The object used for the map.</returns>
    GameObject GetAdvancedMap(string owrbName);

    /// <summary> Add to an existing map. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    /// <param name="map">The object to add to the map.</param>
    /// <param name="radius">The new radius of this planet.</param>
    void UpdateAdvancedMap(string owrbName, GameObject map, float radius = 0);

    /// <summary> Replace an existing map. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    /// <param name="map">The object to use for the map.</param>
    /// <param name="radius">The new radius of this planet.</param>
    void ReplaceAdvancedMap(string owrbName, GameObject map, float radius = 0);

    /// <summary> Remove an existing map. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    void RemoveAdvancedMap(string owrbName);

    /// <summary> Instead of just using the simple sphere, also displays an "Error" text on this planet map. </summary>
    /// <param name="owrbName">The name of the OWRigidbody object.</param>
    void AddErrorMap(string owrbName);

    /// <summary> Make a minimap tornado object. </summary>
    GameObject MakeMinimapTornado();

    /// <summary> Make a minimap hurricane object. </summary>
    GameObject MakeMinimapHurricane();

    bool isReady { get; }
}