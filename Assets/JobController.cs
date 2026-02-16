using UnityEngine;

public class JobController : MonoBehaviour
{
    [SerializeField] private CivilianJobType jobType;

    public CivilianJobType JobType => jobType;

    public void SetJobType(CivilianJobType type)
    {
        jobType = type;
    }
}