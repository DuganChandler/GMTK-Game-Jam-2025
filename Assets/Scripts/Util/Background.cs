using UnityEngine;

[ExecuteAlways]
public class Background : MonoBehaviour {
    public Camera  targetCamera;
    public float   distanceBehind = 10f; 
    void LateUpdate() {
        if (!targetCamera) targetCamera = Camera.main;
        
        transform.SetPositionAndRotation(targetCamera.transform.position
                             + targetCamera.transform.forward * -distanceBehind, targetCamera.transform.rotation);

        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;
        transform.localScale = new Vector3(w, h, 1f);
    }
}
