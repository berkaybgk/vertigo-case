using UnityEngine;

namespace VertigoCase.UI
{
    // Lightweight static helpers used across UI scripts.
    public static class UIExtensions
    {
        // Enables or disables a CanvasGroup's interactability and raycasting in one call.
        public static void SetInteractable(this CanvasGroup group, bool interactable)
        {
            group.interactable   = interactable;
            group.blocksRaycasts = interactable;
        }
    }
}
