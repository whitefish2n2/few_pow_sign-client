using UnityEngine;

public class NewEmptyCSharpScript : MonoBehaviour
{
    void Start()
    {
        Vector3 def_vec = new Vector3(
        10,
        10,
        10);
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal") / 100;
        float y = Input.GetAxisRaw("Vertical") / 100;
        float s_x = 0, s_z = 0;
        Vector3 vec = new Vector3(
        x,
        0,
        y);
        transform.Translate(vec);
        if(Input.GetKey(KeyCode.W))
        {
            if(transform.localScale.x < 5)
            {
                s_x = 0.01f;
            }
            
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (transform.localScale.x > 0.1)
            {
                s_x = -0.01f;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (transform.localScale.z < 5)
            {
                s_z = 0.01f;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (transform.localScale.z > 0.1)
            {
                s_z = -0.01f;
            }
        }
        transform.localScale = new Vector3(
            transform.localScale.x + s_x,
            transform.localScale.y,
            transform.localScale.z + s_z);

    }
}
