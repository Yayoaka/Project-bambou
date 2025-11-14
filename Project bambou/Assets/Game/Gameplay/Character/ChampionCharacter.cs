using Unity.Netcode;
using UnityEngine;

public class ChampionCharacter : NetworkBehaviour
{
    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsOwner)
        {
            //LocalPlayerController.Instance.SetChampionCharacter(this);
        }
    }

    private Vector2 m_movementInput;
    public void SetMovementInput(Vector2 a_movementInput)
    {
        m_movementInput = a_movementInput;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        transform.position += new Vector3(m_movementInput.x, 0, m_movementInput.y) * (5f * Time.deltaTime);
    }
}
