using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyScript : MonoBehaviour
{
    CharacterController _controller;
    public float rotSpeed = 3;
    public float moveSpeed = 5;
    float navPointDistanceTolerance = 0.5f;

    public List<GameObject> navPoints;

    GameObject currentNavPoint;
    public GameObject rotTarget;

    private void Awake() {
        _controller = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentNavPoint = navPoints[0];
    }

    void GoToNextNavPoint(){
        int nextIndex = (navPoints.IndexOf(currentNavPoint) + 1) % navPoints.Count;
        GameObject nextNavPoint = navPoints[nextIndex];
        currentNavPoint = nextNavPoint;    
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToNavPoint = (transform.position - currentNavPoint.transform.position).magnitude;
        if(distanceToNavPoint < navPointDistanceTolerance) GoToNextNavPoint();

        Vector3 moveVec = Vector3.Normalize(currentNavPoint.transform.position - transform.position);
        _controller.Move(moveVec * moveSpeed);

        // Quaternion q = Quaternion.Euler(transform.LookAt(rotTarget.transform.position));
        // q = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotSpeed);
        // transform.rotation = q;

        // Quaternion q = transform.rotation;
        // q.eulerAngles = new Vector3(q.eulerAngles.x, q.eulerAngles.y + 90, q.eulerAngles.z);
        // transform.rotation = q;

        transform.LookAt(rotTarget.transform.position);
    }
}
