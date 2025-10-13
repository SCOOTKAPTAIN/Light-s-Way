using UnityEngine;

namespace NueGames.NueDeck.Scripts.Utils
{
    /// <summary>
    /// Marker component for designer-placed FX anchors in scenes. Place this on an empty GameObject
    /// in the combat scene and CombatManager will locate it at the start of combat.
    /// </summary>
    public class FxAnchor : MonoBehaviour
    {
        // Intentionally empty - serves as a scene marker for FX spawn anchors
    }
}
