using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMO.Dataplane;

namespace FMO.Computations
{
    public class Dijkstra
    {
        double[,] edges = new double[PublicParamerters.myNetWorkSize,PublicParamerters.myNetWorkSize];
        double[] dis = new double[PublicParamerters.myNetWorkSize];
        bool[] vis = new bool[PublicParamerters.myNetWorkSize];
        int[] recordPath = new int[PublicParamerters.myNetWorkSize];
        public Dijkstra(double[,] _edges,double[] _dis,bool[] _vis,int[] _recordPath) 
        {
            edges = _edges;
            dis = _dis;
            vis = _vis;
            recordPath = _recordPath;
        }
        public Stack<int> ShortestPath(int sourceID, int sinkNodeID)
        {
            Stack<int> forwardIdSetAlongThePath = new Stack<int>();
            dis[sourceID] = 0.0;
            for (int i = 0; i < PublicParamerters.myNetWorkSize; i++) 
            {
                double minx = PublicParamerters.INFINITE;
                int minmark=sourceID;
                for (int j = 0; j < PublicParamerters.myNetWorkSize; j++) 
                {
                    if (vis[j] == false && dis[j] <= minx) 
                    {
                        minx = dis[j];
                        minmark = j;
                    }
                }
                vis[minmark] = true;

                for (int j = 0; j < PublicParamerters.myNetWorkSize; j++) 
                {
                    if (vis[j] == false && dis[j] > (dis[minmark] + edges[minmark,j])) 
                    {
                        dis[j] = dis[minmark] + edges[minmark, j];
                        recordPath[j] = minmark;
                    }
                }
            }

            int temp = sinkNodeID;
            while (recordPath[temp]!=sourceID) 
            {
                forwardIdSetAlongThePath.Push(temp);
                temp = recordPath[temp];
            }
            forwardIdSetAlongThePath.Push(temp);
            return forwardIdSetAlongThePath;
        }
    }
}
