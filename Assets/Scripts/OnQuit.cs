using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class OnQuit : MonoBehaviour
{
    Process process;
    void ExecuteCommand(string command)
    {
        var processInfo = new ProcessStartInfo("cmd.exe", @"/C" + command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        process = Process.Start(processInfo);
    }
    void OnApplicationQuit()
    {
        ExecuteCommand("taskkill /F /IM node.exe");
        process.Close();
        Process[] ProcessNodeRed = Process.GetProcessesByName("node.exe");
        if (ProcessNodeRed.Length == 0)
            UnityEngine.Debug.Log("Node-red chiuso con successo!");
        else
            UnityEngine.Debug.Log("Node-red non e' stato chiuso!");
        gameObject.GetComponent<NRConnect>().ClientReceiveThreadQuit();
    }
}
