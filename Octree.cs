using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Octree : MonoBehaviour
{
    // Start parameters.
    public Vector3 initialSize, rootV, rootPos;
    public OctreeNode root { get; set; }
    bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        //Rigidbody[] starList = new Rigidbody[15];

        //for (int i = 0; i < 15; i++)
        //{
        //    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    go.transform.localScale = new Vector3(1, 1, 1);
        //    go.AddComponent<Rigidbody>();
        //    go.name = i.ToString();

        //    starList[i] = go.GetComponent<Rigidbody>();
        //    starList[i].useGravity = false;
        //    starList[i].position = new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20));
        //}

        //GameObject stars = GameObject.Find("Stars");
        //Rigidbody[] starList = stars.GetComponentsInChildren<Rigidbody>();

        //foreach (Rigidbody star in starList)
        //{
        //    addChild(star.mass, Vector3.zero, star.position, star.name);
        //}

        started = true;
    }

    void Update()
    {
                
    }

    private int findOctant(Vector3 pos, Vector3 origin, OctreeNode parent) {
        Vector3 OP = pos - origin; // Position vector
        int octant = -1;

        if (Vector3.Distance(pos, origin) > initialSize.x) {
            return octant;
        }

        if (OP.x >= 0 && OP.y >= 0 && OP.z >= 0) return 0;
        if (OP.x < 0 && OP.y >= 0 && OP.z >= 0) return 1;
        if (OP.x < 0 && OP.y < 0 && OP.z >= 0) return 2;
        if (OP.x >= 0 && OP.y < 0 && OP.z >= 0) return 3;
        if (OP.x >= 0 && OP.y >= 0 && OP.z < 0) return 4;
        if (OP.x < 0 && OP.y >= 0 && OP.z < 0) return 5;
        if (OP.x < 0 && OP.y < 0 && OP.z < 0) return 6;
        if (OP.x >= 0 && OP.y < 0 && OP.z < 0) return 7;

        // Plane approximation
        if (parent.children[octant] != null && parent != root) {
            if (Mathf.Approximately(OP.x, 0f)) {
                Plane yz = new Plane(new Vector3(1, 0, 0), origin);

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.TransformDirection(yz.normal);
                plane.transform.localScale = new Vector3(2, 2, 2);

                if (yz.GetSide(pos))
                {
                    if (octant == 3 || octant == 7) octant -= 1;
                    else octant += 1;
                }
                else {
                    if (octant == 1 || octant == 5) octant -= 1;
                    else octant += 1;
                }
            }
            if (Mathf.Approximately(OP.y, 0f)) {
                Plane xz = new Plane(new Vector3(0, 1, 0), origin);

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.TransformDirection(xz.normal);
                plane.transform.localScale = new Vector3(2, 2, 2);

                if (xz.GetSide(pos)) {
                    if (octant == 0 || octant == 4) octant += 3;
                    else octant += 1;
                }
                else {
                    if (octant == 3 || octant == 7) octant -= 3;
                    else octant -= 1;
                }
            }
            if (Mathf.Approximately(OP.z, 0f)) {
                Plane xy = new Plane(new Vector3(0, 0, 1), origin);

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.TransformDirection(xy.normal);
                plane.transform.localScale = new Vector3(2, 2, 2);

                if (xy.GetSide(pos))
                {
                    octant += 4;
                }
                else
                {
                    octant -= 4;
                }
            }
        }

        return octant;
    }

    private Vector3 findChildOctantCenter(OctreeNode r, int octant) {
        Vector3 center;
        Vector3 dir;

        switch (octant) {
            case 0:
                dir = new Vector3(1, 1, 1);
                break;
            case 1:
                dir = new Vector3(-1, 1, 1);
                break;
            case 2:
                dir = new Vector3(-1, -1, 1);
                break;
            case 3:
                dir = new Vector3(1, -1, 1);
                break;
            case 4:
                dir = new Vector3(1, 1, -1);
                break;
            case 5:
                dir = new Vector3(-1, 1, -1);
                break;
            case 6:
                dir = new Vector3(-1, -1, -1);
                break;
            case 7:
                dir = new Vector3(1, -1, -1);
                break;
            default:
                return Vector3.zero;
        }

        Vector3 extents = Vector3.Scale(dir, new Vector3(r.bounds.extents.x/2, r.bounds.extents.y/2, r.bounds.extents.z/2));
        Vector3 position = new Vector3(r.bounds.center.x, r.bounds.center.y, r.bounds.center.z);

        center = position + extents;

        return center;
    }

    // Add the child node. (iterative approach)
    public void addChild(Rigidbody rb)
    {
        OctreeNode r = root;
        int octChild = findOctant(rb.position, r.Data.pos, r);

        if (octChild == -1) {
            return;
        }

        Vector3 center = findChildOctantCenter(r, octChild);
        bool sublevelExists = false;
        ArrayList lst = new ArrayList();
        OctreeNode parent = r;

        // Inital transversal to find the first unoccupied leaf (first empty octant)
        while (r.children[octChild] != null) {
            r.Data.totalMass += rb.mass;

            r.Data.COMX += rb.position.x * rb.mass;
            r.Data.COMY += rb.position.y * rb.mass;
            r.Data.COMZ += rb.position.z * rb.mass;

            r.Data.COM = new Vector3(r.Data.COMX / r.Data.totalMass, r.Data.COMY / r.Data.totalMass, r.Data.COMZ / r.Data.totalMass);

            r = r.children[octChild];
            parent = r.Clone();

            if (r.children == null)
            {
                r.children = new OctreeNode[8];
            }

            octChild = findOctant(rb.position, center, r);
            center = findChildOctantCenter(r, octChild);

            sublevelExists = true;
        }

        lst.Add(octChild);

        // Debug.Log("Transversal: " + id + "[" + string.Join(", ", lst.ToArray()) + "]");
        // Set the new child node once an empty octant is found
        r.children[octChild] = new OctreeNode(rb);
        r.children[octChild].bounds = new Bounds(center, r.bounds.extents);

        int prevChildOct = octChild;

        OctreeNode child = r.children[octChild];

        // r.Data.COM = new Vector3(r.Data.COMX / r.Data.totalMass, r.Data.COMY / r.Data.totalMass, r.Data.COMZ / r.Data.totalMass);

        Stack<OctreeNode> octantChildren = new Stack<OctreeNode>();

        // Add each child in the sublevel to a stack to check for bound overlap
        foreach (OctreeNode otNode in r.children) {
            if (otNode != null && !otNode.Equals(child)) {

                octantChildren.Push(otNode);
            }
        }

        //if (r.bounds.Contains(parent.Data.pos))
        //{
        //    octantChildren.Push(parent);

        //    int parentOct = findOctant(parent.Data.pos, r.bounds.center, r);
        //    center = findChildOctantCenter(r, parentOct);

        //    r.children[parentOct] = new OctreeNode(mass, initalVel, pos, parent.Data.id);
        //    r.children[parentOct].bounds = new Bounds(center, r.bounds.extents);
        //}

        // Check: do we need to move the parent node to a child node?
        // PROBLEM: Parent exists in a completely different octant for no reason
        
        if (parent.Data.id != root.Data.id)
        {
            octantChildren.Push(parent);
        }

        if (!child.bounds.Contains(parent.Data.pos))
        {
            int octParent = findOctant(r.Data.pos, r.bounds.center, r);
            center = findChildOctantCenter(r, octParent);

            if (r.children[octParent] == null)
            {
                r.children[octParent] = new OctreeNode((Rigidbody) r.Data.id);
                r.children[octParent].bounds = new Bounds(center, r.bounds.extents);
            }
        }

        r = r.children[octChild];
        OctreeNode prevR = r;

        // Pop each child of the sublevel to check if the new octree bounds overlap more than one of each
        while (octantChildren.Count > 0)
        {
            parent = octantChildren.Pop();

            // Bounds overlap, so subdivide further
            while (r.bounds.Contains(parent.Data.pos) && sublevelExists)
            {
                if (!r.hasChildren())
                {
                    r.children = new OctreeNode[8];
                }

                r.Data.totalMass = parent.Data.totalMass + rb.mass;

                r.Data.COMX = rb.position.x * rb.mass + parent.Data.pos.x * parent.Data.totalMass;
                r.Data.COMY = rb.position.y * rb.mass + parent.Data.pos.y * parent.Data.totalMass;
                r.Data.COMZ = rb.position.z * rb.mass + parent.Data.pos.z * parent.Data.totalMass;

                r.Data.COM = new Vector3(r.Data.COMX / r.Data.totalMass, r.Data.COMY / r.Data.totalMass, r.Data.COMZ / r.Data.totalMass);

                // New bounds -- check if the parent node is inside the child node
                Bounds cBounds = r.bounds;

                // New parent position
                int parentOct = findOctant(parent.Data.pos, cBounds.center, r);
                center = findChildOctantCenter(r, parentOct);

                r.children[parentOct] = new OctreeNode((Rigidbody) parent.Data.id);
                r.children[parentOct].bounds = new Bounds(center, cBounds.extents);

                // New child position
                int childOct = findOctant(r.Data.pos, cBounds.center, r);
                center = findChildOctantCenter(r, childOct);

                r.children[childOct] = new OctreeNode(rb);
                r.children[childOct].bounds = new Bounds(center, cBounds.extents);

                // Set child another level down and check for overlap again
                r = r.children[childOct];
            }

            r = prevR;
        }
    }
}

public class OctreeNode
{
    public OctreeNode[] children { get; set; }

    public OctreeNodeData Data { get; set; }
    public Bounds bounds;

    public OctreeNode(Rigidbody id) {
        Data = new OctreeNodeData(id.mass, id.velocity, id.position, id);
    }

    public OctreeNode Clone() {
        return (OctreeNode) MemberwiseClone();
    }
        
    // See if the current node has children.
    public bool hasChildren() {
        return (children != null);
    }
}

public class OctreeNodeData
{
    public float totalMass { get; set; }
    public Vector3 velocity { get; set; }
    public Vector3 pos { get; set; }
    public float COMX { get; set; }
    public float COMY { get; set; }
    public float COMZ { get; set; }
    public Vector3 COM { get; set; }
    public object id;

    public OctreeNodeData(float totalMass, Vector3 velocity, Vector3 pos, object id)
    {
        this.totalMass = totalMass;
        this.velocity = velocity;
        this.pos = pos;
        this.id = id;
    }
}
