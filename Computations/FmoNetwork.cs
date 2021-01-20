using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMO.Dataplane;
using FMO.Dataplane.PacketRouter;
using FMO.Energy;
using FMO.Intilization;
using FMO.FuzzySets;
using FMO.ui;
using System.Threading;

namespace FMO.Computations
{
    public class FmoNetwork
    {
        public List<Sensor> myNetWork;

        public FmoNetwork(List<Sensor> myNetWork) 
        {
            this.myNetWork = myNetWork;
        }
        public void compute() 
        {
            initialWeightEachLinks();
            initialForwardPathEachNodes();
        }
        //this.forwardPath = new Stack<int>(fmo.UpdateForwardPath(this).ToArray());
        public void UpdateForwardPath()
        {
            //each nodes in the entire network update the weight of link between this node and neighbors
            initialWeightEachLinks();
            //the sink collects the informations
            theSinkCollectInfo();
            //the sink compute the shortest path;
            foreach (Sensor sen in myNetWork)
            {
                if (sen.ID != PublicParamerters.SinkNode.ID)
                {
                    sen.forwardPath = new Stack<int>(MinimumWeightPath(sen, PublicParamerters.SinkNode).ToArray());
                }
            }
            //the sink sends the packet including updatePath to nodes 
            foreach (Sensor sen in myNetWork)
            {
                if (sen.ID != PublicParamerters.SinkNode.ID)
                {
                    sen.GenerateUpdatePacket();
                    //theSinkSendPacketIncludingShortestPath(sen);
                }
            }
        }
        public void theSinkCollectInfo() 
        {
            foreach (Sensor sen in myNetWork) 
            {
                if (sen.ID != PublicParamerters.SinkNode.ID) 
                {
                    sen.GenerateUpdatePacket();
                }
            }
        }
        public void theSinkSendPacketIncludingShortestPath(Sensor routingRequestNode) 
        {
            foreach (Sensor sen in myNetWork) 
            {
                if (sen.ID == routingRequestNode.ID) 
                {
                    //sen.GenerateUpdatePacket();
                    PublicParamerters.NumberofUpdatePackets += 1;
                    UpdateOverhead(sen);
                    //PublicParamerters.MainWindow.Dispatcher.Invoke(() => PublicParamerters.MainWindow.lbl_number_of_update_packet.Content = PublicParamerters.NumberofUpdatePackets);
                }
            }
        }
        public void UpdateOverhead(Sensor sen) 
        {
            Stack<int> tmp = new Stack<int>(sen.forwardPath.ToArray());
            Sensor sender = sen;
            Sensor receiver = null;
            while (tmp.Count != 0) 
            {
                int nextNodeID = tmp.Pop();
                foreach (NeighborsTableEntry nei in sender.NeighborsTable) 
                {
                    if (nei.NeiNode.ID == nextNodeID) 
                    {
                        receiver = nei.NeiNode;
                        PublicParamerters.TotalReduntantTransmission += 1;
                        break;
                    }
                }
                //计算发送和接收的能量开销;
                SenderOverhead(sender, receiver);
                ReceiverOverhead(receiver);
                sender = receiver;
                receiver = null;
            } 
        }
        public void SenderOverhead(Sensor sender,Sensor receiver) 
        {
            FirstOrderRadioModel EnergyModel = new FirstOrderRadioModel();
            double Distance_M = Operations.DistanceBetweenTwoSensors(sender, receiver);
            double UsedEnergy_Nanojoule = EnergyModel.Transmit(PublicParamerters.UpdatePacketLength, Distance_M);
            double UsedEnergy_joule = sender.ConvertToJoule(UsedEnergy_Nanojoule);
            sender.ResidualEnergy = sender.ResidualEnergy - UsedEnergy_joule;
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;
            PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule;
            //PublicParamerters.MainWindow.Dispatcher.Invoke(() => PublicParamerters.MainWindow.lbl_Wasted_Energy_percentage.Content = PublicParamerters.WastedEnergyPercentage);
        }
        public void ReceiverOverhead(Sensor receiver) 
        {
            FirstOrderRadioModel EnergyModel = new FirstOrderRadioModel();
            double UsedEnergy_Nanojoule = EnergyModel.Receive(PublicParamerters.UpdatePacketLength);
            double UsedEnergy_joule = receiver.ConvertToJoule(UsedEnergy_Nanojoule);
            receiver.ResidualEnergy = receiver.ResidualEnergy - UsedEnergy_joule;
            PublicParamerters.TotalEnergyConsumptionJoule += UsedEnergy_joule;
            PublicParamerters.TotalWastedEnergyJoule += UsedEnergy_joule;
            //PublicParamerters.MainWindow.Dispatcher.Invoke(() => PublicParamerters.MainWindow.lbl_Wasted_Energy_percentage.Content = PublicParamerters.WastedEnergyPercentage);
        }
        public void initialWeightEachLinks() 
        {
            //更新网络中每条链路的weight
            FirstOrderRadioModel EnergyModel = new FirstOrderRadioModel();
            foreach (Sensor sen in myNetWork) 
            {
                foreach (NeighborsTableEntry nei in sen.NeighborsTable) 
                {
                    double Distance_M = Operations.DistanceBetweenTwoSensors(sen, nei.NeiNode);
                    double tx_Nanojoule = EnergyModel.Transmit(PublicParamerters.RoutingDataLength, Distance_M);
                    double tx = sen.ConvertToJoule(tx_Nanojoule);
                    double re = sen.ResidualEnergy - tx;
                    LifetimeFuzzySet lifetime = new LifetimeFuzzySet(sen.BatteryIntialEnergy,PublicParamerters.alpha,PublicParamerters.gamma,tx,re);
                    double ult = lifetime.Ult;

                    double maxTx = Max_TX(sen);
                    MinimumEnergyFuzzySet minimumEnergy = new MinimumEnergyFuzzySet(PublicParamerters.delta,tx,maxTx);
                    double ume = minimumEnergy.Ume;

                    double u = PublicParamerters.beta * Min(ult, ume) + (1 - PublicParamerters.beta) * ((ult + ume) / 2.0);
                    nei.weight = 1 - u;
                }
            }
        }
        public void initialForwardPathEachNodes()
        {
            foreach (Sensor sen in myNetWork) 
            {
                if (sen.ID != PublicParamerters.SinkNode.ID) 
                {
                    sen.forwardPath = new Stack<int>(MinimumWeightPath(sen, PublicParamerters.SinkNode).ToArray());
                }
            }
        }

        //sink work
        public Stack<int> MinimumWeightPath(Sensor source,Sensor sinkNode) 
        {
            Stack<int> minimumWeightPath = new Stack<int>();
            //ths sink updates the weight of edges in the entire network 
            double[,] edges = new double[PublicParamerters.myNetWorkSize, PublicParamerters.myNetWorkSize];
            for (int i = 0; i < PublicParamerters.myNetWorkSize; i++) 
            {
                for (int j = 0; j < PublicParamerters.myNetWorkSize; j++) 
                {
                    edges[i, j] = PublicParamerters.INFINITE; 
                }
            }
            foreach (Sensor sen in myNetWork) 
            {
                foreach (NeighborsTableEntry nei in sen.NeighborsTable) 
                {
                    edges[sen.ID, nei.NeiNode.ID] = nei.weight;
                }
            }
            double[] dis = new double[PublicParamerters.myNetWorkSize];
            for (int i = 0; i < PublicParamerters.myNetWorkSize; i++) 
            {
                dis[i] = PublicParamerters.INFINITE;
            }
            bool[] vis = new bool[PublicParamerters.myNetWorkSize];
            for (int i = 0; i < PublicParamerters.myNetWorkSize; i++) 
            {
                vis[i] = false;
            }
            int[] recordPath = new int[PublicParamerters.myNetWorkSize];
            for (int i = 0; i < PublicParamerters.myNetWorkSize; i++) 
            {
                recordPath[i] = source.ID;
            }
            //the sink gets the shortest path from source to sink
            Dijkstra dij = new Dijkstra(edges,dis,vis,recordPath);
            minimumWeightPath = dij.ShortestPath(source.ID, sinkNode.ID);
            return minimumWeightPath;
        }
        public double Max_TX(Sensor sen) 
        {
            double maxTx = 0.0;
            foreach (NeighborsTableEntry nei in sen.NeighborsTable) 
            {
                double distance = Operations.DistanceBetweenTwoSensors(sen, nei.NeiNode);
                if (distance >= maxTx) 
                {
                    maxTx = distance;
                }
            }
            return maxTx;
        }
        public double Min(double ult, double ume) 
        {
            if (ult > ume)
            {
                return ume;
            }
            else 
            {
                return ult;
            }
        }
    }
}
