using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnime(string animName)
    {
        animator.Play(animName);
        StartCoroutine(CheckIsFinishedAnime());
    }

    private IEnumerator CheckIsFinishedAnime()
    {
        // Play実行直後はアニメーションが再生されていないので0.3秒待つ
        yield return new WaitForSeconds(0.3f);
        while(true)
        {
            // normalizedTimeが1以上ならチェック完了
            if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
            {
                Debug.Log("Finished");
                break;
            }
            // 0.1秒ごとにチェックする
            yield return new WaitForSeconds(0.1f);
        }
    }
}
