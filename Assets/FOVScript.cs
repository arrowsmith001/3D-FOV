using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

[System.Serializable]
public class PlayerHitEvent : UnityEvent<bool>{};

public class FOVScript : MonoBehaviour
{
    [Tooltip("Invoked when GameObject with tag \"Player\" detection status is changed. Returns TRUE if player has been detected, and FALSE if player detection has ended.")]
    public PlayerHitEvent playerHitEvent = new PlayerHitEvent();

    [Tooltip("Toggle to show/hide raycasts.")]
    public bool displayCasts = true;

    [Tooltip("Raw distance of the visual field.")]
    public float distance = 20;
    
    [Tooltip("The density of the FOV mesh. The higher n is, the more detailed the FOV, but at higher performance cost. If you're having trouble getting collisions, make n larger.")]
    public int n = 35;

    [Tooltip("Value determining the FOV's maximum horizontal reach. 0 is none, 1 is 90 degrees, etc.")]
    public float h = 1f;

    [Tooltip("Value determining the FOV's maximum vertical reach. 0 is none, 1 is 90 degrees, etc.")]
    public float v = 1.02f;

    [Tooltip("Dampening factor for FOV vertical reach i.e. caps how tall the field can get.")]
    public float maxV = 1.06f;

    [Tooltip("Speed at which FOV rotates in direction specified by SetTargetRotation.")]
    public float rotSpeed = 3;

    [Tooltip("Index 0 is normal material for FOV, index 1 is when triggered by the player entering the FOV.")]
    public Material[] FOVMaterials;

    // To be used by external scripts to set the target rotation of the FOV itself. 
    // Note: FOV rotates with the attached character anyway, but in case of up/down movement or slight left/right movement, use this.
    // Don't forget to reset to Vector3.zero!
    public void SetTargetRotation(Vector3 targetRotation)
    {
        this.targetRotation = targetRotation;
    }

    Vector3 targetRotation = Vector3.zero;

    void RotateFOV(){
    
        Quaternion local = Quaternion.Euler(transform.localRotation.eulerAngles);
        Quaternion target = Quaternion.Euler(targetRotation);

        transform.localRotation = Quaternion.Slerp(local, target, rotSpeed * Time.deltaTime);
    }

    bool playerIsBeingDetected = false; // Used to track if player is currently being detected

    int? mask;

    void Awake()
    {
        // Layer mask to be used when ray casting i.e. FOV will ONLY detect on this layer. If null, mask defaults to the default layer.
        // On a complex character model, I'd recommend layermasking OUT the character collider itself, so that rays don't collide with the character itself.
        // TODO: Set custom layermask.
        mask = null;
    }

    // Update is called once per frame
    void Update()
    {
        RotateFOV();

        Mesh mesh = new Mesh();
        meshObject.GetComponent<MeshFilter>().mesh = mesh;

        bool playerDetected = false;

        int nH = n, nV = n;

        // For easily navigating the abstract grid that makes up the mesh
        GridNav grid = new GridNav((nH + 1), (nV + 1));

        // Initialise mesh arrays
        Vector3[] vertices = new Vector3[(nH + 1) *(nV + 1) + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[6 * vertices.Length]; // TODO this is vast overestimate

        Vector3 origin = transform.position;        
        vertices[0] = new Vector3(0,0,0);

        // To cycle through arrays
        int vertexIndex = 1;
        int triangleIndex = 0;

        // Calculate vertexes
        Vector3 up = transform.up;
        Vector3 right = transform.right;
        Vector3 fwd = transform.forward;

        float dh = (h * 2) / nH; // Horizontal increments
        float dv = (v * 2) / nV; // Vertical increments

        for(int i = 0; i <= nH; i++)
        {
            float thisH = -h + i * dh; // Horizontal position

            for (int j = 0; j <= nV; j++)
            {
                float thisV = -v + j * dv; // Vertical position

                Vector3 vec = Vector3.Normalize(fwd + right * thisH + up * thisV) * distance; // Raycast direction

                // Calculate distance modifier
                float theta = GetAngleBetweenVectorAndPlane(vec, transform.up);
                float smallD = Mathf.Abs(maxV/Mathf.Sin(theta));
                float modifier = Mathf.Min(smallD / distance, 1);
                vec *= modifier; // Apply modifier

                RaycastHit hit;
                if (Physics.Raycast(origin, vec, out hit, distance, mask ?? 1))
                {
                    vec = vec * (hit.distance / distance); // Cuts vector size in proportion to hit distance

                    if(hit.collider.tag == "Player" && hit.distance <= vec.magnitude) // Responds to "Player" tag attached to a game object
                    {
                        playerDetected = true;
                    }
                }

                // At this point, we have our vector - and therefore position - of FOV mesh point
                vertices[vertexIndex] = vec;
                if(displayCasts) Debug.DrawLine(origin, origin + vec);

                if(vertexIndex > 0){

                    // Front cases
                    if(grid.GetDownLeft(vertexIndex) != null)
                    {
                        triangles[triangleIndex + 2] = vertexIndex;
                        triangles[triangleIndex + 1] = (int) grid.GetDown(vertexIndex);
                        triangles[triangleIndex + 0] = (int) grid.GetDownLeft(vertexIndex); 
                        
                        triangleIndex += 3;
                    }
                    
                    if(grid.GetUpRight(vertexIndex) != null)
                    {
                        triangles[triangleIndex + 2] = vertexIndex;
                        triangles[triangleIndex + 1] = (int) grid.GetUp(vertexIndex);
                        triangles[triangleIndex + 0] = (int) grid.GetUpRight(vertexIndex);   

                        triangleIndex += 3;                      
                    }

                    // Border cases - note, for some reason up/down is actually left/right. You'll have to experiment with commenting out one of the if clauses to determine which is which, if you needed to.
                    if(grid.GetLeft(vertexIndex) == null)
                    {
                        if(grid.GetDown(vertexIndex) != null)
                        {
                            triangles[triangleIndex + 2] = vertexIndex;
                            triangles[triangleIndex + 1] = (int) grid.GetDown(vertexIndex);
                            triangles[triangleIndex + 0] = 0; 

                            triangleIndex += 3;
                        }
                     }

                     if(grid.GetRight(vertexIndex) == null)
                    {
                        if(grid.GetDown(vertexIndex) != null)
                        {
                            triangles[triangleIndex + 0] = vertexIndex;
                            triangles[triangleIndex + 1] = (int) grid.GetDown(vertexIndex);
                            triangles[triangleIndex + 2] = 0; 

                            triangleIndex += 3;
                        }
                    }

                    if(grid.GetUp(vertexIndex) == null)
                    {
                        if(grid.GetLeft(vertexIndex) != null)
                        {
                            triangles[triangleIndex + 2] = vertexIndex;
                            triangles[triangleIndex + 1] = (int) grid.GetLeft(vertexIndex);
                            triangles[triangleIndex + 0] = 0; 

                            triangleIndex += 3;
                        }
                     }
                     if(grid.GetDown(vertexIndex) == null)
                    {
                        if(grid.GetRight(vertexIndex) != null)
                        {
                            triangles[triangleIndex + 2] = vertexIndex;
                            triangles[triangleIndex + 1] = (int) grid.GetRight(vertexIndex);
                            triangles[triangleIndex + 0] = 0; 

                            triangleIndex += 3;
                        }
                    }
                }
                vertexIndex++;
            }

        }    
    
        //mesh.bounds = new Bounds(Vector3.zero, Vector3.one); // TODO This is vast overestimate

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles; 

        // Changes mesh material if player is detected/not detected
        if(playerDetected && !playerIsBeingDetected)
        {
            playerIsBeingDetected = true;
             meshObject.GetComponent<MeshRenderer>().material = FOVMaterials[1]; // Triggered FOV color
             playerHitEvent.Invoke(true);
        }   
        else if(playerIsBeingDetected && !playerDetected)
        {
            playerIsBeingDetected = false;
            meshObject.GetComponent<MeshRenderer>().material = FOVMaterials[0]; // Default FOV color
             playerHitEvent.Invoke(false);
        }

    }

    private float GetAngleBetweenVectorAndPlane(Vector3 vec, Vector3 norm)
    {
        float num = Math.Abs(Vector3.Dot(vec, norm));
        float den1 = Mathf.Sqrt(Mathf.Pow(vec.x, 2) + Mathf.Pow(vec.y, 2) + Mathf.Pow(vec.z, 2) );
        float den2 = Mathf.Sqrt(Mathf.Pow(norm.x, 2) +Mathf.Pow(norm.y, 2) + Mathf.Pow(norm.z, 2) );
        return Mathf.Asin(num/(den1*den2));
    }


    //[HideInInspector]
    public GameObject meshObject;
}

class GridNav{

    public GridNav(int H, int V){
        this.H = H;
        this.V = V;
        this.max = H*V;
    }

    int H;
    int V;
    int max;

    public int? GetLeft(int? i){
        if(i == null) return null;
        if((i-1) % H == 0) return null;
        return i-1;
    }
    public int? GetRight(int? i){
        if(i == null) return null;
        if(i % H == 0) return null;
        return i+1;
        
    }
    public int? GetUp(int? i){
        if(i == null) return null;
        if(i + H > max) return null;
        return i + H;
    }
    public int? GetDown(int? i){
        if(i == null) return null;
        if(i - H <= 0) return null;
        return i - H;
        
    }

    public int? GetUpLeft(int? i) { return GetUp(GetLeft(i)); }
    public int? GetUpRight(int? i) { return GetUp(GetRight(i)); }
    public int? GetDownLeft(int? i) { return GetDown(GetLeft(i)); }
    public int? GetDownRight(int? i) { return GetDown(GetRight(i)); }
}
