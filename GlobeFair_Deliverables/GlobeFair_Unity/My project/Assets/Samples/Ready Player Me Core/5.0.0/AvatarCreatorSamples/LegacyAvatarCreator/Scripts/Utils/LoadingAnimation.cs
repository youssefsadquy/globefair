using System.Collections;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerator LerpScale(Transform transform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
