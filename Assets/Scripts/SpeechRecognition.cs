using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SpeechRecognition : MonoBehaviour
{
    private LoadAssets loadAssets;

    public string[] keywords = new string[] { "sciolo"};
    public ConfidenceLevel Confidence = ConfidenceLevel.Medium;
    protected string word = "sciolo";
    NRConnect nrC;
    DF2Manager DF2;
    protected PhraseRecognizer recognizer;

    // Start is called before the first frame update
    private void Start()
    {
        recognizer = new KeywordRecognizer(keywords, Confidence);
        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
        recognizer.Start();
        nrC = GetComponent<NRConnect>();
        DF2 = GetComponent<DF2Manager>();
    }
   
    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log(args.text);
        if(args.text == "sciolo" && nrC.ConnectionClient)
        {
            nrC.SendMessageNR("MIC_Input");
            _ = WaitResponseServer();
        }
    }
    public async Task WaitResponseServer()
    {
        while (!nrC.isResponseReceived) 
        {
            await Task.Yield();
        }
        DF2.phrase = nrC.responseReceived;
        DF2.isPhrasesRecognized = true;
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }
}
