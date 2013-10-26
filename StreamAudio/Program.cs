using System;
using System.Collections;
using NAudio.Wave;
using Renci.SshNet;
using System.Text;

namespace StreamAudio
{
    #region Class "Params"

    public class Params
    {
        private Hashtable _args;

        public Params()
        {
            this.parseParams(Environment.GetCommandLineArgs());
        }

        public bool isExists(string name)
        {
            return this._args.ContainsKey(name);
        }

        public bool remove(string name)
        {
            if (!isExists(name)) return false;
            _args.Remove(name);
            return true;
        }

        public string this[string name]
        {
            get
            {
                if (isExists(name)) return _args[name].ToString();
                return "";
            }
            set
            {
                if (isExists(name)) _args[name] = value;
                else _args.Add(name, value);
            }
        }
        public override string ToString()
        {
            string tmp = "";

            foreach (string key in _args.Keys)
            {
                tmp += "--" + key + " " + (string)_args[key] + " ";
            }
            return tmp;
        }

        private void parseParams(string[] args)
        {
            this._args = new Hashtable();
            string arg_name = "";
            string arg_value = "";
            for (int i = 0; i < args.Length; i++)
            {
                int _count = (
                                 (args[i].TrimStart(new char[' ']).IndexOf("-") == 0) ? 1 : 0         //   looking for "-"
                             ) + (
                                 (args[i].TrimStart(new char[' ']).IndexOf("/") == 0) ? 1 : 0         //   looking for "/"
                             ) + (
                                 (args[i].TrimStart(new char[' ']).IndexOf("--") == 0) ? 1 : 0        //   looking for "--"
                             );
                if (_count > 0)
                {
                    if (arg_name == "" && arg_value == "")
                    {
                        arg_name = args[i].Substring(_count);
                    }
                    else
                    {
                        if (arg_value == "") arg_value = " true";
                        this._args.Add(arg_name, arg_value.Substring(1));
                        arg_name = args[i].Substring(_count); arg_value = "";
                    }
                }
                else
                {
                    if (arg_name != "")
                    {
                        arg_value += " " + args[i];
                    }
                }
            }

            if (arg_name != "" && arg_value != "")
            {
                this._args.Add(arg_name, arg_value.Substring(1));
            }

            if (arg_name != "" && arg_value == "")
            {
                this._args.Add(arg_name, "true");
            }
        }
    }
    #endregion

    class Program
    {
        static SshClient sshClient;
        static ShellStream shell;
        static System.Net.Sockets.TcpClient client;



        static byte[] GetBytes(string data){
            return System.Text.UTF8Encoding.UTF8.GetBytes(data);
        }

        static void Main()
        {
            Params args = new Params();

            client = new System.Net.Sockets.TcpClient();
           
            

            

            int wavInDevices = WaveIn.DeviceCount;
            int selWav = 0;
            for (int wavDevice = 0; wavDevice < wavInDevices; wavDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(wavDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels", wavDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }
            Console.Write("Select device: ");
            selWav=int.Parse(Console.ReadLine());
            Console.WriteLine("Selected device is " + selWav.ToString());
            
            
            
            sshClient = new SshClient(args["host"], args["user"], args["pass"]);
            sshClient.Connect();
            
            if (sshClient.IsConnected) {

                shell = sshClient.CreateShellStream("xterm", 50, 50, 640, 480, 17640);
                Console.WriteLine("Open listening socket...");
                shell.WriteLine("nc -l "+ args["port"] +"|pacat --playback");
                System.Threading.Thread.Sleep(2000);

                Console.WriteLine("Try to connect...");
                client.Connect(args["host"], int.Parse(args["port"]));
                if (!client.Connected) return;

                //====================
                
                WaveInEvent wavInStream = new WaveInEvent();
                wavInStream.DataAvailable += new EventHandler<WaveInEventArgs>(wavInStream_DataAvailable);
                wavInStream.DeviceNumber = selWav;
                wavInStream.WaveFormat = new WaveFormat(44100, 16, 2);
                wavInStream.StartRecording();
                Console.WriteLine("Working.....");             
              

                Console.ReadKey();
                 sshClient.Disconnect();
                client.Close();
                 wavInStream.StopRecording();
                 wavInStream.Dispose();
                 wavInStream = null;
            }
            
            
                


        }

        static void wavInStream_DataAvailable(object sender, WaveInEventArgs e)
        {

            client.GetStream().Write(e.Buffer, 0, e.BytesRecorded);
            client.GetStream().Flush();
          // shell.Write(e.Buffer, 0, e.BytesRecorded);       
           //shell.Flush();


        }
    }
}
