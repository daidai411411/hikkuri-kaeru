using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Goal Triggered");
        sceneTransition.LoadScene("Result");
    }
}
