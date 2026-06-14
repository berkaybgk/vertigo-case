using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.Editor
{
    // Editor menu tool to disable raycastTarget on Images that do not require user interaction.
    public static class RaycastTargetCleaner
    {
        [MenuItem("Tools/VertigoCase/Disable Non-Interactive Raycasts")]
        public static void DisableNonInteractiveRaycasts()
        {
            int disabled = 0;
            int skipped  = 0;

            var allImages = Object.FindObjectsOfType<Image>(includeInactive: true);

            foreach (var image in allImages)
            {
                if (!image.raycastTarget)
                    continue; // already disabled — skip

                if (IsPartOfInteractable(image.transform))
                {
                    skipped++;
                }
                else
                {
                    Undo.RecordObject(image, "Disable Raycast Target");
                    image.raycastTarget = false;
                    EditorUtility.SetDirty(image);
                    disabled++;
                }
            }

            Debug.Log($"[RaycastTargetCleaner] Done. Disabled: {disabled} | Kept interactive: {skipped}");
        }

        // Returns true if the transform is itself a Selectable component, or if any of its direct ancestors (up to 2 levels) is one.
        private static bool IsPartOfInteractable(Transform t)
        {
            // Check self and up to 2 parent levels
            for (int depth = 0; depth < 3 && t != null; depth++, t = t.parent)
            {
                if (t.GetComponent<Button>() != null)  return true;
                if (t.GetComponent<Toggle>() != null)  return true;
                if (t.GetComponent<Slider>() != null)  return true;
                if (t.GetComponent<Scrollbar>() != null) return true;
                if (t.GetComponent<InputField>() != null) return true;
            }
            return false;
        }
    }
}
