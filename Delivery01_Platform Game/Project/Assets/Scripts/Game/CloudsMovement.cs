using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsMovement : MonoBehaviour
{

    public GameObject StartPoint;
    public GameObject EndPoint;

    public float speed;

    // Update is called once per frame
    void Update()
    {
        if (EndPoint.transform.position.x < this.gameObject.transform.position.x) 
        {
            this.gameObject.transform.position = new Vector3(StartPoint.transform.position.x , this.gameObject.transform.position.y , this.gameObject.transform.position.z);
        }

        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x + speed * Time.deltaTime, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
    }
}
