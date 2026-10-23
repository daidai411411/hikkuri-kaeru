using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FrogImage : MonoBehaviour
{
    [SerializeField] private Image spriteRenderer;
    [SerializeField] private Sprite[] frogSprites;
    [SerializeField] private float frameInterval = 0.15f;
    private int currentSpriteIndex = 0;
    private List<int> frameSheet = new List<int> {1, 1, 2, 3, 4, 4, 4, 5};

    void Start()
    {
        StartCoroutine(AnimateFrog());
    }

    private IEnumerator AnimateFrog()
    {
        int frame = 0;

        while (true)
        {
            spriteRenderer.sprite = frogSprites[frame];
            frame = frameSheet[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % frameSheet.Count;

            yield return new WaitForSecondsRealtime(frameInterval);
        }
    }
}
