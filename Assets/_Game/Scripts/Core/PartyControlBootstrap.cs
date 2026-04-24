using UnityEngine;

public class PartyControlBootstrap : MonoBehaviour
{
    [Header("Initial Controlled Member")]
    [SerializeField] private PartyMemberControlBridge initiallyControlledMember;

    [Header("Other Party Members")]
    [SerializeField] private PartyMemberControlBridge[] otherMembers;

    private void Start()
    {
        if (initiallyControlledMember == null)
        {
            Debug.LogWarning("[PartyControlBootstrap] No initially controlled member assigned.");
            return;
        }

        initiallyControlledMember.SetPlayerControlled(true);

        if (otherMembers != null)
        {
            for (int i = 0; i < otherMembers.Length; i++)
            {
                if (otherMembers[i] == null || otherMembers[i] == initiallyControlledMember)
                    continue;

                otherMembers[i].SetPlayerControlled(false);
            }
        }
    }
}