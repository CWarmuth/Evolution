using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    public Transform Follow;
    private Camera MainCamera;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        MainCamera = Camera.main;

        offset = transform.position - MainCamera.WorldToScreenPoint(Follow.position);
    }

    // Update is called once per frame
    void Update()
    {
        var screenPos = MainCamera.WorldToScreenPoint(Follow.position) + offset;
        transform.position = screenPos;
    }
}
