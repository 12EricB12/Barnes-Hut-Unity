using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public float speed;
    public float sensitivity = 3.0f;
    private bool xz;
    private Toggle goUp;
    private float camSpeed;

    // Start is called before the first frame update
    void Start()
    {
        xz = true;
        // goUp = GameObject.FindWithTag("goUp").GetComponent<Toggle>();
        camSpeed = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        speed = camSpeed;

        if (xz)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.up * Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }

        transform.position += transform.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        if (Input.GetMouseButton(1)) rotateCam();

        if (Input.GetKey("-")) Camera.main.fieldOfView += 0.5f;
        if (Input.GetKey("=")) Camera.main.fieldOfView -= 0.5f;

        // checkUp();
    }
    private void rotateCam()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.eulerAngles += new Vector3(mouseY * sensitivity, mouseX * sensitivity, 0);
    }
    private void checkUp()
    {
        if (goUp.isOn)
        {
            xz = false;
        }
        else
        {
            xz = true;
        }
    }
}
