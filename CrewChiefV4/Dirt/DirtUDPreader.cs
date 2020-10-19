using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace CrewChiefV4.Dirt
{
    public class DirtUDPreader : GameDataReader
    {
        private long packetRateCheckInterval = 1000;
        private long packetCountAtStartOfNextRateCheck = 0;
        private long ticksAtStartOfCurrentPacketRateCheck = 0;

        int packetCount = 0;

        private Boolean newSpotterData = true;
        private Boolean running = false;
        private Boolean initialised = false;
        private List<DirtStructWrapper> dataToDump;
        private DirtStructWrapper workingData = new DirtStructWrapper();
        private DirtStructWrapper[] dataReadFromFile = null;
        private int dataReadFromFileIndex = 0;
        private int udpPort = 20777;

        private string hardwareSettingsFileDirt = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally\hardwaresettings\hardware_settings_config.xml";
        private string hardwareSettingsFileDirt2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally 2.0\hardwaresettings\hardware_settings_config.xml";
        private string hardwareSettingsFileDirt2VR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally 2.0\hardwaresettings\hardware_settings_config_vr.xml";

        private byte[] receivedDataBuffer;

        private IPEndPoint broadcastAddress;
        private UdpClient udpClient;

        private String lastReadFileName = null;

        private AsyncCallback socketCallback;

        private static Boolean[] buttonsState = new Boolean[32];

        public override void DumpRawGameData()
        {
            if (dumpToFile && dataToDump != null && dataToDump.Count > 0 && filenameToDump != null)
            {
                SerializeObject(dataToDump.ToArray<DirtStructWrapper>(), filenameToDump);
            }
        }

        public override void ResetGameDataFromFile()
        {
            dataReadFromFileIndex = 0;
        }

        public override Object ReadGameDataFromFile(String filename, int pauseBeforeStart)
        {
            if (dataReadFromFile == null || filename != lastReadFileName)
            {
                dataReadFromFileIndex = 0;
                var filePathResolved = Utilities.ResolveDataFile(this.dataFilesPath, filename);
                dataReadFromFile = DeSerializeObject<DirtStructWrapper[]>(filePathResolved);
                lastReadFileName = filename;
                Thread.Sleep(pauseBeforeStart);
            }
            if (dataReadFromFile != null && dataReadFromFile.Length > dataReadFromFileIndex)
            {
                DirtStructWrapper structWrapperData = dataReadFromFile[dataReadFromFileIndex];
                workingData = structWrapperData;
                newSpotterData = true;
                dataReadFromFileIndex++;
                return structWrapperData;
            }
            else
            {
                return null;
            }
        }

        protected override Boolean InitialiseInternal()
        {
            if (!this.initialised)
            {
                if (CrewChief.gameDefinition.gameEnum == GameEnum.DIRT)
                {
                    this.udpPort = UserSettings.GetUserSettings().getInt("dirt_rally_udp_data_port");
                    UpdateXML(hardwareSettingsFileDirt);
                }
                else if (CrewChief.gameDefinition.gameEnum == GameEnum.DIRT_2)
                {                    
                    this.udpPort = UserSettings.GetUserSettings().getInt("dirt_rally_2_udp_data_port");
                    UpdateXML(hardwareSettingsFileDirt2);
                    UpdateXML(hardwareSettingsFileDirt2VR);
                }
                socketCallback = new AsyncCallback(ReceiveCallback);
                packetCount = 0;
                packetCountAtStartOfNextRateCheck = packetRateCheckInterval;
                ticksAtStartOfCurrentPacketRateCheck = DateTime.UtcNow.Ticks;

                if (dumpToFile)
                {
                    dataToDump = new List<DirtStructWrapper>();
                }
                this.broadcastAddress = new IPEndPoint(IPAddress.Any, udpPort);
                this.udpClient = new UdpClient();
                this.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.udpClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
                this.udpClient.Client.Bind(this.broadcastAddress);
                this.receivedDataBuffer = new byte[this.udpClient.Client.ReceiveBufferSize];
                this.running = true;
                this.udpClient.Client.BeginReceive(this.receivedDataBuffer, 0, this.receivedDataBuffer.Length, SocketFlags.None, ReceiveCallback, this.udpClient.Client);
                this.initialised = true;
                Console.WriteLine("Listening for UDP data on port " + udpPort);
            }
            return this.initialised;
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            //Socket was the passed in as the state
            try
            {
                Socket socket = (Socket)result.AsyncState;
                int received = socket.EndReceive(result);
                if (received > 0)
                {
                    // do something with the data
                    lock (this)
                    {
                        packetCount++;
                        try
                        {
                            readFromOffset(0, this.receivedDataBuffer);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error reading UDP data " + e.Message);
                        }
                    }
                }
                if (running)
                {
                    // socket.BeginReceive(this.receivedDataBuffer, 0, this.receivedDataBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                    socket.BeginReceive(this.receivedDataBuffer, 0, this.receivedDataBuffer.Length, SocketFlags.None, socketCallback, socket);
                }
            }
            catch (Exception e)
            {
                this.initialised = false;
                if (e is ObjectDisposedException || e is SocketException)
                {
                    Console.WriteLine("Socket is closed");
                    return;
                }
                throw;
            }
        }

        public override Object ReadGameData(Boolean forSpotter)
        {
            DirtStructWrapper latestData = workingData.CreateCopy(DateTime.UtcNow.Ticks, forSpotter);
            lock (this)
            {
                if (!initialised)
                {
                    if (!InitialiseInternal())
                    {
                        throw new GameDataReadException("Failed to initialise UDP client");
                    }
                }
                if (forSpotter)
                {
                    newSpotterData = false;
                }
            }
            if (!forSpotter && dumpToFile && dataToDump != null && workingData != null /* && latestData has some sane data?*/)
            {
                dataToDump.Add(latestData);
            }
            return latestData;
        }

        private int readFromOffset(int offset, byte[] rawData)
        {
            // for packets of type Motion:
            // lapDistance is System.BitConverter.ToSingle(rawData, 8)
            // trackLength is System.BitConverter.ToSingle(rawData, 244)
            this.workingData.dirtData.time = BitConverter.ToSingle(rawData, 0);
            this.workingData.dirtData.currentStageTime = BitConverter.ToSingle(rawData, 4);
            this.workingData.dirtData.lapDistance = BitConverter.ToSingle(rawData, 8);
            this.workingData.dirtData.speed = BitConverter.ToSingle(rawData, 28);
            this.workingData.dirtData.stageLength = BitConverter.ToSingle(rawData, 244);
            this.workingData.dirtData.trackNumber = BitConverter.ToInt32(rawData, 272);
            return 0;
        }
        
        public override void Dispose()
        {
            if (udpClient != null)
            {
                try
                {
                    if (running)
                    {
                        stop();
                    }
                    udpClient.Close();
                }
                catch (Exception) { }
            }
            initialised = false;
        }

        public override bool hasNewSpotterData()
        {
            return newSpotterData;
        }

        public override void stop()
        {
            running = false;
            if (udpClient != null && udpClient.Client != null && udpClient.Client.Connected)
            {
                udpClient.Client.Disconnect(true);
            }
            Console.WriteLine("Stopped UDP data receiver, received " + packetCount + " packets");
            this.initialised = false;
            packetCount = 0;
        }

        public static bool[] ConvertBytesToBoolArray(uint buttons)
        {
            bool[] result = new bool[32];
            // check each bit in each of the bytes. if 1 set to true, if 0 set to false
            for (int i = 0; i < 32; i++)
            {
                result[i] = (buttons & (1 << i)) == 0 ? false : true;
            }

            return result;
        }

        private void UpdateXML(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    bool save = false;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    XmlNode root = doc.DocumentElement;
                    XmlNode udpNode = root.SelectSingleNode("descendant::udp");
                    if (udpNode == null)
                    {
                        // no UDP node, create it and it's motion_platform parent, with the attributes we need
                        save = true;
                        CreateElement(doc, root);
                    }
                    else
                    {
                        // check the attributes and update them if necessary
                        save = UpdateUDPAttributes(udpNode);
                    }
                    if (save)
                    {
                        Console.WriteLine("Updating UDP element in " + fileName);
                        if (!File.Exists(fileName + "_backup"))
                        {
                            File.Copy(fileName, fileName + "_backup");
                        }
                        doc.Save(fileName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to update settings XML file " + fileName + ", " + e.Message);
                }
            }
        }

        private bool UpdateUDPAttributes(XmlNode udpNode)
        {
            bool save = false;
            if (udpNode.Attributes["enabled"] == null || !udpNode.Attributes["enabled"].Value.Equals("true"))
            {
                save = true;
                udpNode.Attributes["enabled"].Value = "true";
            }
            if (udpNode.Attributes["extradata"] == null || !int.TryParse(udpNode.Attributes["extradata"].Value, out var edv) || edv < 3)
            {
                // extradata doesn't exist, or it exists but it's not set to "3" or above
                save = true;
                udpNode.Attributes["extradata"].Value = "3";
            }
            if (udpNode.Attributes["port"] == null || !udpNode.Attributes["port"].Value.Equals(this.udpPort.ToString()))
            {
                save = true;
                udpNode.Attributes["port"].Value = this.udpPort.ToString();
            }
            return save;
        }

        private void CreateElement(XmlDocument doc, XmlNode root)
        {
            // try to create it
            XmlNode motionPlatform = root.SelectSingleNode("descendant::motion_platform");
            if (motionPlatform == null)
            {
                motionPlatform = doc.CreateElement("motion_platform");
                root.AppendChild(motionPlatform);
            }
            XmlNode udpNode = doc.CreateElement("udp");
            XmlAttribute enabledAttrib = doc.CreateAttribute("enabled");
            enabledAttrib.Value = "true";
            udpNode.Attributes.Append(enabledAttrib);
            XmlAttribute extradataAttrib = doc.CreateAttribute("extradata");
            extradataAttrib.Value = "3";
            udpNode.Attributes.Append(extradataAttrib);
            XmlAttribute ipAttrib = doc.CreateAttribute("ip");
            ipAttrib.Value = "127.0.0.1";
            udpNode.Attributes.Append(ipAttrib);
            XmlAttribute portAttrib = doc.CreateAttribute("port");
            portAttrib.Value = CrewChief.gameDefinition.gameEnum == GameEnum.DIRT ?
                UserSettings.GetUserSettings().getInt("dirt_rally_udp_data_port").ToString() : UserSettings.GetUserSettings().getInt("dirt_rally_2_udp_data_port").ToString();
            udpNode.Attributes.Append(portAttrib);
            XmlAttribute delayAttrib = doc.CreateAttribute("delay");
            delayAttrib.Value = "1";
            udpNode.Attributes.Append(delayAttrib);
            motionPlatform.AppendChild(udpNode);
        }
    }
}
