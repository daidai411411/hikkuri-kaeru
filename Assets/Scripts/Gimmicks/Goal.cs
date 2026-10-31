using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private Timer timer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() == null)
        {
            return;
        }

        timer.RecordResult();
        sceneTransition.LoadScene("Result");
    }
}
