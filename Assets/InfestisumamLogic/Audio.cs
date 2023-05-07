using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip audioClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
    }
    


}