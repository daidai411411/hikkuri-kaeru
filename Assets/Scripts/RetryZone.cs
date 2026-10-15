using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryZone : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null ||
            other.attachedRigidbody.gameObject != playerObject)
        {
            return;
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
