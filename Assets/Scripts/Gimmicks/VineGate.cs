using UnityEngine;

// つるの門。台座（PressurePlate）の On Pressed / On Released から Open / Close を呼んで開け閉めする
public class VineGate : MonoBehaviour
{
    public void Open()
    {
        gameObject.SetActive(false);
    }

    public void Close()
    {
        gameObject.SetActive(true);
    }
}
