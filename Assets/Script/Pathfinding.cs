﻿using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform seeker, target;
    Player player;
    public bool driveable = true;
    Vector3 baslangicKonumu;
    float kusUcumuMesafe;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Player'ı bul

        baslangicKonumu = player.transform.position;

    }
    void GoToTarget()
    {
        kusUcumuMesafe = Vector3.Distance(player.transform.position, target.position);
        if (kusUcumuMesafe<=4f)
        {
            Debug.LogWarning("tp attim");
            player.transform.position = baslangicKonumu;

        }
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {

            Vector3 hedefNokta = grid.path1[0].WorldPosition;  // İlk path noktası 
            player.LookToTarget(hedefNokta);

            //     Debug.Log(Vector3.Distance(player.transform.position, target.position));  // hedefle kus ucumu mesafe olcer 

            player.GidilcekYer(hedefNokta);  // Hedef noktayı Player'a gönder

        }
    }



    private void Update()
    {
        FindPath(seeker.position, target.position);
        GoToTarget();
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz);
        Node targetNode = grid.NodeFromWorldPoint(targetPoz);

        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (currentNode.fCost > openSet[i].fCost || currentNode.fCost == openSet[i].fCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                // Kavşak kontrolü
                if (!currentNode.kavsak && !neighbour.kavsak)
                {
                    // Yön kontrolü
                    if (currentNode.gridY < neighbour.gridY && !currentNode.right) // Yukarı hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX < neighbour.gridX && !currentNode.right) // Sağa hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridY > neighbour.gridY && !currentNode.left) // Aşağı hareket (left == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX > neighbour.gridX && !currentNode.left) // Sola hareket (left == true)
                    {
                        continue;
                    }
                }

                // Right'tan direkt Left'e veya Left'ten direkt Right'a geçişi engelle
                if (currentNode.right && neighbour.left && !neighbour.kavsak)
                {
                    continue;
                }
                if (currentNode.left && neighbour.right && !neighbour.kavsak)
                {
                    continue;
                }

                // Hareket maliyetini hesapla ve komşuyu ekle
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        // Yol bulunamadıysa hata mesajı
        Debug.LogWarning("Path not found!");
    }


  


    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        grid.path1 = path;
    }



    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstx = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dsty = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstx > dsty)
            return 14 * dsty + 10 * dstx;
        return 14 * dstx + 10 * (dsty - dstx);
    }


}
