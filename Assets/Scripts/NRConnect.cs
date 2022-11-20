using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using UnityEngine;

public class NRConnect : MonoBehaviour
{

	[HideInInspector] public bool ConnectionClient = false;
	[HideInInspector] public bool isResponseReceived = false;
	[HideInInspector] public string responseReceived;
	Process process;
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
    #endregion
    private void Awake()
    {
		ExecuteCommand("node-red");
	}
    private void Start()
	{
		ConnectToTcpServer();
	}
    // Connessione socket. 	
    private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
            UnityEngine.Debug.Log("On client connect exception " + e);
		}
	}	
	// Background listening per ricevere dati dal server	     
	public void ListenForData()
	{
		socketConnection = new TcpClient();
		while (!socketConnection.Connected)
		{
			try
			{
				Thread.Sleep(1000);
				socketConnection.Connect("127.0.0.1", 8052);
				ConnectionClient = true;
				UnityEngine.Debug.Log("Connessione stabilita");
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary. 					
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 						
							string serverMessage = Encoding.ASCII.GetString(incommingData);
							isResponseReceived = true;
							responseReceived = serverMessage;
							UnityEngine.Debug.Log("server message received as: " + serverMessage);

							string[] splitarray = serverMessage.Split('*');
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				UnityEngine.Debug.Log("Socket exception: " + socketException);
			}
		}
	}
	// Manda messaggi al server. 	
	public void SendMessageNR(string clientMessage)
	{
		isResponseReceived = false;
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				UnityEngine.Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			UnityEngine.Debug.Log("Socket exception: " + socketException);
		}
	}
	void ExecuteCommand(string command)
	{
		var processInfo = new ProcessStartInfo("cmd.exe", @"/C" + command);
		processInfo.CreateNoWindow = true;
		processInfo.UseShellExecute = false;
		process = Process.Start(processInfo);
	}
	public void ClientReceiveThreadQuit()
    {
		clientReceiveThread.Interrupt();
		clientReceiveThread.Join();
    }
}