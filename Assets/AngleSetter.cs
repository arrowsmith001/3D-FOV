using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to set the FOVMeshHolder to constant zero angle.
// Is necessary purely to keep the mesh rotation in line with the other rotations going on.
public class AngleSetter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}
