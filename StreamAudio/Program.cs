using System;
using System.Collections;
using NAudio.Wave;
using Renci.SshNet;
using BlackCore;
using System.Text;

namespace StreamAudio
{
    
   partial class Program
   {
       static SshClient sshClient;
       static ShellStream shell;
       static System.Net.Sockets.TcpClient client;

       static byte[] GetBytes(string data)
       {
           return System.Text.UTF8Encoding.UTF8.GetBytes(data);
       }

       static void _main()
       {
           BlackCore.basic.cParams args = bcore.app.args;

           client = new System.Net.Sockets.TcpClient();





           int wavInDevices = WaveIn.DeviceCount;
           int selWav = 0;
           for (int wavDevice = 0; wavDevice < wavInDevices; wavDevice++)
           {
               WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(wavDevice);
               Console.WriteLine("Device {0}: {1}, {2} channels", wavDevice, deviceInfo.ProductName, deviceInfo.Channels);
           }
           
           Console.Write("Select device: ");
           selWav = int.Parse(Console.ReadLine());
           Console.WriteLine("Selected device is " + selWav.ToString());



           sshClient = new SshClient(args["host"], args["user"], args["pass"]);
           sshClient.Connect();

           if (sshClient.IsConnected)
           {

               shell = sshClient.CreateShellStream("xterm", 50, 50, 640, 480, 17640);
               Console.WriteLine("Open listening socket...");
               shell.WriteLine("nc -l " + args["port"] + "|pacat --playback");
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
