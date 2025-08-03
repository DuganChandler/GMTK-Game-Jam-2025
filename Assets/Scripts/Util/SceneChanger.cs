using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void TransitionToScene(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void TransitionToScene(string s)
    {
        SceneManager.LoadScene(s);
    }
}
