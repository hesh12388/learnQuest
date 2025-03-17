using UnityEngine;
using TMPro;  // Import TextMeshPro
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
   
        public static TransitionManager Instance { get; private set; } // Singleton instance

        public Animator transitionAnimator;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep this object between scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start(){
            transitionAnimator = GetComponent<Animator>();
        }


    public IEnumerator transition(int levelIndex){
        
        transitionAnimator.SetTrigger("End");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Module" + levelIndex);
        transitionAnimator.SetTrigger("Start");
    }
}