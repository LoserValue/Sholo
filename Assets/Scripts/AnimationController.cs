using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    DF2Manager df2;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        df2 = GameObject.FindGameObjectWithTag("InputManager").GetComponent<DF2Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(df2.isWaving)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isWaving", true);
            df2.isWaving = false;
        }
        else
            animator.SetBool("isWaving", false);

        if(df2.isBowing)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isBowing", true);
            df2.isBowing = false;
        }
        else
            animator.SetBool("isBowing", false);

        if (df2.isDancing)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isDancing", true);
            df2.isDancing = false;
        }
        else
            animator.SetBool("isDancing", false);

        if (df2.isLooking)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isLooking", true);
            df2.isLooking = false;
        }
        else
            animator.SetBool("isLooking", false);

        if (df2.isScared)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isScared", true);
            df2.isScared = false;
        }
        else
            animator.SetBool("isScared", false);

        if (df2.isClapping)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isClapping", true);
            df2.isClapping = false;
        }
        else
            animator.SetBool("isClapping", false);

        if (df2.isAcrobat)
        {
            Debug.Log("Animation Start");
            animator.SetBool("isAcrobat", true);
            df2.isAcrobat = false;
        }
        else
            animator.SetBool("isAcrobat", false);
    }
}
