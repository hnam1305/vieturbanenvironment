using UnityEngine;
public class AudioZone : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float fadeSpeed = 0.5f;
    private bool playerInside;
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
       
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float target;
        if (playerInside)
        {
            target = maxVolume;
        }
        else
        {
            target = minVolume;
        }
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, target, fadeSpeed * Time.deltaTime);
    }
}
