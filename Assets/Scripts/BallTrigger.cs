using UnityEngine;

public class BallTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<BallCount>().ballCount += 1;
            Destroy(this.gameObject);
        }
    }
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
