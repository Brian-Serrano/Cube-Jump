using UnityEngine;
using System.Collections;
using TMPro;

public static class AnimationManager
{
    public static IEnumerator AnimateCoinIcon(RectTransform from, RectTransform to, float duration = 0.5f)
    {
        Vector3 startPos = from.position;
        Vector3 targetPos = to.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // smoothstep easing
            from.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        from.position = targetPos;
        Object.Destroy(from.gameObject);
    }

    public static IEnumerator AnimateCoinText(TMP_Text text, int current, int target, string prefix = "", string postfix = "", float duration = 1f)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            text.text = prefix + Mathf.RoundToInt(Mathf.Lerp(current, target, t)) + postfix;

            yield return null;
        }

        text.text = prefix + target + postfix;
    }
}
