using System.Collections;
using UnityEngine;

public class TutorialImage : MonoBehaviour
{
    private SpriteRenderer _sr;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();

        Color c = _sr.color;
        c.a = 0f;
        _sr.color = c;

        StartCoroutine(PlayTutorial());
    }

    private IEnumerator PlayTutorial()
    {
        // 1√ÎΩ•œ‘
        yield return StartCoroutine(Fade(0f, 1f, 1f));

        // œ‘ æ5√Î
        yield return new WaitForSeconds(5f);

        // 3√ÎΩ•“˛
        yield return StartCoroutine(Fade(1f, 0f, 3f));

        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;
        Color c = _sr.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, timer / duration);
            _sr.color = c;
            yield return null;
        }

        c.a = to;
        _sr.color = c;
    }
}