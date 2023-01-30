using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Syrus.Plugins.DFV2Client;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;

public class DF2Manager : MonoBehaviour
{
	[HideInInspector] public string session;
	private LoadAssets loadAssets;
	private int _index;
	private int _spawnCount;

	//animation section
	[HideInInspector] public bool isWaving = false;
	[HideInInspector] public bool isDancing = false;
	[HideInInspector] public bool isScared = false;
	[HideInInspector] public bool isClapping = false;
	[HideInInspector] public bool isLooking = false;
	[HideInInspector] public bool isAcrobat = false;
	[HideInInspector] public bool isBowing = false;


	private DialogFlowV2Client client;
	private NRConnect nodeRed;

	[HideInInspector] public string phrase = "Gira l'albero";
	[HideInInspector] public bool isPhrasesRecognized = false;
	private string[] HappyResponse = {"Sono felice per te", "Bene!", "Ne sono felice"};
	private string[] SadResponse = { "Mi dispiace per te", "Mi dispiace", "Che peccato" };
	private string resultCamObject;
	private string ResponseText;
	private bool isRecognized = false;

	// Start is called before the first frame update
	public void Start()
	{
		client = GetComponent<DialogFlowV2Client>();

		// Adjustes session name if it is blank.
		string sessionName = GetSessionName();

		client.ChatbotResponded += LogResponseText;
		client.DetectIntentError += LogError;
		client.ReactToContext("DefaultWelcomeIntent-followup",
			context => Debug.Log("Reacting to welcome followup"));
		client.SessionCleared += sess => Debug.Log("Cleared session " + session);
		client.AddInputContext(new DF2Context("userdata", 1, ("name", "George")), sessionName);
		loadAssets = FindObjectOfType<LoadAssets>();
		nodeRed = GetComponent<NRConnect>();
	}

    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.F1))
		{
			_index = 0;
			_spawnCount = 1;
			RequestSpawnModel();
		}
		if (Input.GetKeyDown(KeyCode.F3))
		{
			_index = 0;
			_spawnCount = 1;
			RequestkillModel();
		}
		if(Input.GetKeyDown(KeyCode.Space))
			isWaving = true;
		if (isPhrasesRecognized)
        {
			SendText();
			isPhrasesRecognized = false;
		}
		//wait until the string take the response
        if (isRecognized)
        {
			if(ResponseText == "card_spawn")
            {
				_index = 1;
				_spawnCount = 1;
				RequestSpawnModel();
				nodeRed.SendMessageNR("cmdmp3 carta pescata!");
			}
			else if(ResponseText == "presentation_INPUT")
            {
				isBowing = true;
				nodeRed.SendMessageNR("cmdmp3 Io sono Sholo, un ologramma interattivo, capace di capire il linguaggio umano, e reagire ad alcuni stimoli visivi");
            }
			else if(ResponseText == "saluto_INPUT")
            {
				isWaving = true;
			}
			else if(ResponseText == "dance_INPUT")
            {
				isDancing = true;
			}
			else if(ResponseText == "clapping_INPUT")
            {
				isClapping = true;
			}
			else if(ResponseText == "inchino_INPUT")
            {
				isBowing = true;
			}
			else if(ResponseText == "acrobatic_INPUT")
            {
				isAcrobat = true;
            }
			else if(ResponseText == "DeleteAll_output")
            {
				_index = 1;
				_spawnCount = 1;
				RequestkillModel();
            }
			else if(ResponseText == "WebCam_NR_INPUT")
            {
				isLooking = true;
				nodeRed.SendMessageNR(ResponseText);
				_ = HandleResponseCamResult();
			}
			else if(ResponseText == "sentiment_INPUT")
            {
				nodeRed.SendMessageNR("MIC_Input_SENTIMENT");
				_ = HandleResponseSentiment();
			}
			else
            {
				nodeRed.SendMessageNR("cmdmp3 "+ ResponseText);
			}
			isRecognized = false;
		}
	}
	public void RequestSpawnModel()
	{
		for (int i = 0; i < _spawnCount; i++)
			loadAssets.Spawn(_index);
	}
	public void RequestkillModel()
    {
		for (int i = 0; i < _spawnCount; i++)
        {
			loadAssets.RemoveModel(_index);
		}

	}

	public async Task HandleResponseSentiment()
    {
		while (!nodeRed.isResponseReceived)
		{
			await Task.Yield();
		}
		if(float.Parse(nodeRed.responseReceived, CultureInfo.InvariantCulture.NumberFormat) > 0.05)
        {

			nodeRed.SendMessageNR("cmdmp3 " + HappyResponse[Random.Range(0, HappyResponse.Count())]);
		}
		else
			nodeRed.SendMessageNR("cmdmp3 " + SadResponse[Random.Range(0, SadResponse.Count())]);

	}
	public async Task HandleResponseCamResult()
	{
		while (!nodeRed.isResponseReceived)
		{
			await Task.Yield();
		}
		resultCamObject = nodeRed.responseReceived.ToLower();
		if ((resultCamObject.Contains("gun") || resultCamObject.Contains("revolver")) || resultCamObject.Contains("weapon"))
        {
			isScared = true;
        }
		if(resultCamObject.Contains("flower") || resultCamObject.Contains("plant"))
        {
			isClapping = true;
        }

	}
	void SendText()
	{
		string sessionName = GetSessionName();
		client.DetectIntentFromText(phrase, sessionName);
		isPhrasesRecognized = false;
	}
	private void LogResponseText(DF2Response response)
	{
		Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
		Debug.Log(GetSessionName() + " said: \"" + response.queryResult.fulfillmentText + "\"");
		ResponseText = response.queryResult.fulfillmentText;
		isRecognized = true;
	}

	private void LogError(DF2ErrorResponse errorResponse)
	{
		Debug.LogError(string.Format("Error {0}: {1}", errorResponse.error.code.ToString(),
			errorResponse.error.message));
	}


    //public void SendEvent()
    //{
    //	client.DetectIntentFromEvent(content.text,
    //		new Dictionary<string, object>(), GetSessionName());
    //}

    public void Clear()
	{
		client.ClearSession(GetSessionName());
	}


	private string GetSessionName(string defaultFallback = "DefaultSession")
	{
		string sessionName = session;
		if (sessionName.Trim().Length == 0)
			sessionName = defaultFallback;
		return sessionName;
	}
}