
using Exception = System.Exception;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Example script to demonstrate rotating the FOV independently of the character it is attached to.
// FOV should ideally be left to rotate as the character rotates, but there may be required up-down direction in particular, in which case SetTargetRotation does the job.
// Rotation speed is set by the FOVScript itself.
public class FOVRotationScriptExample : MonoBehaviour
{
    FOVScript fov;

    private void Awake() {
        
        fov = gameObject.GetComponent<FOVScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Rotate(fov));
    }

    // Try-catch clauses to deal with a weird null exception on the FOVscript. Works just as expected otherwise.
    IEnumerator Rotate(FOVScript fov)
    {
        while(true)
        {
            try{
            fov.SetTargetRotation(Vector3.up * 30);
            }   catch(Exception e){}  

            yield return new WaitForSeconds(2);

            try{
            fov.SetTargetRotation(Vector3.down * 30);

            }   catch(Exception e){}  

            yield return new WaitForSeconds(2);
            try{
            fov.SetTargetRotation(Vector3.left * 30);

            }   catch(Exception e){}  

            yield return new WaitForSeconds(2);

            try{
            fov.SetTargetRotation(Vector3.right * 30);

            }   catch(Exception e){}  

            yield return new WaitForSeconds(2);
            
            try{
            fov.SetTargetRotation(Vector3.zero);
            }   catch(Exception e){}  

            yield return new WaitForSeconds(2);
        }
    }

}
