using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PressKeyToEnter : MonoBehaviour
{
    public GameObject Instruction;
    public bool Action = false;
    void Start()
    {
        Instruction.SetActive(false);
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.tag == "Player")
        {
            Instruction.SetActive(true);
            Action = true;
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        Instruction.SetActive(false);
        Action = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (Action == true)
            {
                Instruction.SetActive(false);
                Action = false;
                SceneManager.LoadScene(1);
            }
        }
    }

}
