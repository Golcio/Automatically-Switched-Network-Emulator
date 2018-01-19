using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Router
{
    class RouterSwitcher
    {
        static byte[] result = new byte[4];
        public static byte[] switchPacket(byte[] receivedData, List<String[]> switchTables)
        {
            DTOs.Packet packet = DTOs.Packet.ByteToPacket(receivedData, 4);
            byte[] currentport = new byte[4];
            for (int k = 0; k < 4; k++)
            {
                currentport[k] = receivedData[k];
            }
            int currentport2 = BitConverter.ToInt32(currentport, 0);
            int signal = 0;
            recursionSwitch(ref receivedData, switchTables, currentport2, packet, ref signal);
            if (signal == 0)
            {
                throw new Exception();
            }
            return receivedData;
        }

        public static void recursionSwitch(ref byte[] receivedData, List<String[]> switchTables, int currentport2, DTOs.Packet packet, ref int signal)
        {
            for (int i = 0; i < switchTables.Count; i++)
            {
                if (Int32.Parse(switchTables[i][0]) == currentport2 && Int32.Parse(switchTables[i][1]) == packet.Labels.Peek() && Int32.Parse(switchTables[i][3]) == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tUsunięto etykietę nr " + packet.Labels.Pop());
                    signal++;
                    recursionSwitch(ref receivedData, switchTables, currentport2, packet, ref signal);
                }
                else if (Int32.Parse(switchTables[i][0]) == currentport2 && Int32.Parse(switchTables[i][1]) == packet.Labels.Peek() && switchTables[i].Length == 5)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tUsunięto etykietę nr " + packet.Labels.Pop());
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tDodano etykietę nr " + Int32.Parse(switchTables[i][3]));
                    packet.Labels.Push(Int32.Parse(switchTables[i][3]));
                    Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tPakiet skierowany na interfejs wyjściowy: " + Int32.Parse(switchTables[i][2]));
                    byte[] intBytes = BitConverter.GetBytes(Int32.Parse(switchTables[i][2]));
                    Array.Copy(intBytes, result, 4);
                    signal++;
                    recursionSwitch(ref receivedData, switchTables, currentport2, packet, ref signal);
                }
                else if (Int32.Parse(switchTables[i][0]) == currentport2 && Int32.Parse(switchTables[i][1]) == packet.Labels.Peek() && switchTables[i].Length == 6)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Router.RouterMain.GetTimestamp(DateTime.Now) + "\tDodano etykietę nr " + Int32.Parse(switchTables[i][3]));
                    packet.Labels.Push(Int32.Parse(switchTables[i][3]));
                    signal++;
                }
            }
            
            byte[] packetbytes = DTOs.Packet.PacketToByte(packet);
            receivedData = new byte[packetbytes.Length + 4];
            Array.Copy(packetbytes, 0, receivedData, 4, packetbytes.Length);
            for (int j = 0; j < 4; j++)
            {
                receivedData[j] = result[j];
            }
            return;
        }
    }
}
