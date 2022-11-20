using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    private LoadAssets loadAssets;
    [SerializeField] GameObject Scene;
    [SerializeField] GameObject InputManager;
    NRConnect nrC;
    Animation m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        nrC = InputManager.GetComponent<NRConnect>();
        m_Animator = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if(nrC.ConnectionClient)
        {
            m_Animator.Stop();
            loadAssets = FindObjectOfType<LoadAssets>();
            loadAssets.Spawn(0);
            Scene.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
