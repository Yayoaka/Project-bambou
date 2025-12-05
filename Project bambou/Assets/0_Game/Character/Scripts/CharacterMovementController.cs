using System;
using Entity;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterMovementController : EntityComponent<CharacterBehaviour>
    {
        private NetworkVariable<float> _moveSpeed = new(5f, 
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [SerializeField] private Rigidbody rb;
        private Vector3 _moveDirection;

        public override void LateInit()
        {
            base.LateInit();

            Owner.Stats.OnStatsChanged += UpdateSpeed;
        }

        private new void OnDestroy()
        {
            Owner.Stats.OnStatsChanged -= UpdateSpeed;
        }

        public void Move(Vector2 input)
        {
            _moveDirection.Set(input.x, 0f, input.y);
        }

        private void FixedUpdate()
        {
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                var newPosition = rb.position + _moveDirection * _moveSpeed.Value * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
            }
        }
        
        public void RotateToMouse(Vector3 worldMousePos)
        {
            if (!IsOwner) return;
            
            var direction = worldMousePos - rb.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                var rot = Quaternion.LookRotation(direction);
                rb.MoveRotation(rot);
            }
        }

        private void UpdateSpeed()
        {
            if(IsServer) //SyncVar server only
                _moveSpeed.Value = Owner.Stats.GetStat(StatType.MoveSpeed);
        }
    }
}