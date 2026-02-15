using UnityEngine;
using System.Collections.Generic;

public class AIResourceManager : MonoBehaviour
{
    [Header("Team Settings")]
    public int teamID;

    [Header("Resource Claiming")]
    public float claimRadius = 35f;

    // Internal list of nodes this AI has claimed
    private readonly List<ResourceNode> claimedNodes = new List<ResourceNode>();

    private AIThreatDetector threatDetector;
    private AIMilitary military;

    void Awake()
    {
        threatDetector = GetComponent<AIThreatDetector>();
        military = GetComponent<AIMilitary>();
    }

    // Called by your AI tick system
    public void Tick()
    {
        ClaimNearbyNodes();
        DefendNodes();
    }

    // ---------------------------------------------------------
    // CLAIM NODES
    // ---------------------------------------------------------
    void ClaimNearbyNodes()
    {
        ResourceNode[] allNodes = GameObject.FindObjectsOfType<ResourceNode>();
        float claimRadiusSqr = claimRadius * claimRadius;
        Vector3 currentPosition = transform.position;

        foreach (var node in allNodes)
        {
            if (node == null) continue;
            if (node.IsDepleted) continue;
            if (node.IsClaimedByOther(teamID)) continue;
            if (claimedNodes.Contains(node)) continue;

            float sqrDist = (currentPosition - node.transform.position).sqrMagnitude;
            if (sqrDist > claimRadiusSqr)
                continue;

            node.claimedByTeam = teamID;
            claimedNodes.Add(node);
        }
    }

    // ---------------------------------------------------------
    // DEFEND CLAIMED NODES
    // ---------------------------------------------------------
    void DefendNodes()
    {
        if (threatDetector == null || military == null)
            return;

        foreach (var node in claimedNodes)
        {
            if (node == null) continue;
            if (node.IsDepleted) continue;

            Attackable threat = threatDetector.DetectThreatNear(node.transform.position);
            if (threat != null)
                military.DefendLocation(node.transform.position);
        }
    }
}
