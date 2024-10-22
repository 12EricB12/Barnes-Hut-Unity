using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Transactions;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BarnesHut : MonoBehaviour
{
    Rigidbody[] stars;
    Octree bhOctree;

    Vector3 initialSize = new Vector3(300, 300, 300);
    Vector3 rootV = new Vector3(0, 0, 0);
    Vector3 rootPos = new Vector3(0, 0, 0);

    float theta = 0.5f;
    float G = 3f;
    float posRange = 70f;

    bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        //GameObject s = GameObject.Find("Stars");
        //stars = s.GetComponentsInChildren<Rigidbody>();

        stars = new Rigidbody[15];

        for (int i = 0; i < 15; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.AddComponent<Rigidbody>();
            go.name = i.ToString();

            stars[i] = go.GetComponent<Rigidbody>();
            stars[i].useGravity = false;
            stars[i].velocity = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-15, 15));
            stars[i].position = new Vector3(UnityEngine.Random.Range(-posRange, posRange), UnityEngine.Random.Range(-posRange, posRange), UnityEngine.Random.Range(-posRange, posRange));
            stars[i].mass = 50;
        }

        initializeOctree();
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        Octree script = GetComponent<Octree>();
        Destroy(script);
        initializeOctree();
        Stack<OctreeNode> unvisited = new Stack<OctreeNode>();

        unvisited.Push(bhOctree.root);

        while (unvisited.Count > 0) {
            OctreeNode cNode = unvisited.Pop();

            // If the node is a star
            if (!cNode.hasChildren()) {
                Vector3 F = findNetForces(cNode, bhOctree);

                cNode.Data.velocity += (F / cNode.Data.totalMass) * Time.deltaTime;
                cNode.Data.pos += cNode.Data.velocity * Time.deltaTime;

                Rigidbody curr = (Rigidbody) cNode.Data.id;
                curr.velocity = cNode.Data.velocity;
                curr.position = cNode.Data.pos;

                continue;
            }

            foreach (OctreeNode child in cNode.children) {
                if (child != null) {
                    unvisited.Push(child);
                }
            }
        }
    }

    Vector3 findNetForces(OctreeNode cStar, Octree ot)
    {
        Vector3 FNet = Vector3.zero;
        Stack<OctreeNode> unvisited = new Stack<OctreeNode>();
        unvisited.Push(ot.root);

        FNet += pairwiseForce(cStar, ot.root);

        while (unvisited.Count > 0)
        {
            OctreeNode cNode = unvisited.Pop();

            if (cNode == null || cNode.Equals(cStar)) continue;
            float dist = Vector3.Distance(cStar.Data.pos, cNode.Data.COM);

            if (cStar.Data.pos == cNode.Data.pos) continue;
            if (!cNode.hasChildren() || (cNode.bounds.size.x / dist) < theta)
            {
                FNet += pairwiseForce(cStar, cNode);
            }
            else {
                foreach (OctreeNode otNode in cNode.children) {
                    unvisited.Push(otNode);
                }
            }
        }

        return FNet;
    }

    void initializeOctree() {
        //GameObject s = GameObject.Find("Stars");
        //stars = s.GetComponentsInChildren<Rigidbody>();
        bhOctree = gameObject.AddComponent<Octree>();
        Rigidbody root = GameObject.Find("Center").GetComponent<Rigidbody>();
        root.mass = 10;

        bhOctree.initialSize = initialSize;
        bhOctree.rootV = rootV;
        bhOctree.rootPos = rootPos;
        bhOctree.root = new OctreeNode(root);
        bhOctree.root.bounds = new Bounds(rootPos, initialSize);
        bhOctree.root.children = new OctreeNode[8];

        foreach (Rigidbody star in stars)
        {
            bhOctree.addChild(star);
        }
    }

    // Update is called once per frame
    private void OnDrawGizmosSelected()
    {
        if (!started)
        {
            return;
        }

        Stack<OctreeNode> unvisited = new Stack<OctreeNode>();
        unvisited.Push(bhOctree.root);

        while (unvisited.Count > 0)
        {
            OctreeNode currRoot = unvisited.Pop();

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(currRoot.bounds.center, 0.2f);
            Gizmos.DrawWireCube(currRoot.bounds.center, currRoot.bounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currRoot.bounds.max, 0.2f);
            Gizmos.DrawSphere(currRoot.bounds.min, 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currRoot.Data.COM, 0.5f);

            if (currRoot.children == null)
            {
                continue;
            }

            foreach (OctreeNode ot in currRoot.children)
            {
                if (ot != null)
                {
                    unvisited.Push(ot);
                }
            }
        }
    }

    Vector3 pairwiseForce(OctreeNode node1, OctreeNode node2) {
        float m1 = node1.Data.totalMass;
        float m2 = node2.Data.totalMass;

        if (node2.Equals(bhOctree.root)) {
            m2 = ((Rigidbody)node2.Data.id).mass;
        }

        Vector3 q1 = node1.Data.pos;
        Vector3 q2 = node2.Data.pos;

        float s = (float) ((G * m1 * m2) / (Math.Pow(Vector3.Distance(q1, q2), 3)));
        return s * (q2 - q1);
    }
}
