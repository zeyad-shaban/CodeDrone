using UnityEngine;

public class PayloadHolder : MonoBehaviour
{
    [SerializeField] Brain brain;
    [SerializeField] GameObject payload;

    private SpringJoint springJoint;

    void Start()
    {
        springJoint = payload.GetComponent<SpringJoint>();
        brain.OnCutRope += OnCutRopeHandler;
    }

    private void OnCutRopeHandler()
    {
        Destroy(springJoint);
    }

    void Update()
    {
    }
}
