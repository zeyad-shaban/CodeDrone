using UnityEngine;

class Target
{

    public Vector3 worldPos;
    private float confirmationScore;
    private float alpha = 0.05f;
    public float lastDetectedTime;

    public Target(Vector3 worldPos)
    {
        this.worldPos = worldPos;
        lastDetectedTime = Time.time;
        confirmationScore = 0;
    }

    public float GetConfirmationScore()
    {
        return confirmationScore;
    }

    public void IncrementConfirmationScore(float conf, Vector3 newPos)
    {
        confirmationScore += conf * 2;
        lastDetectedTime = Time.time;
        worldPos = (1 - alpha) * worldPos + alpha * newPos;
    }

    public void DecrementConfirmationScore()
    {
        confirmationScore -= Time.time - lastDetectedTime;
    }
    public bool ShouldDestroy()
    {
        return confirmationScore <= 0;
    }
}