using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Input
{
    public class CharacterInputController : NetworkBehaviour
    {
        private CharacterController _character;
        private UnityEngine.Camera _cam;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                var pi = GetComponent<PlayerInput>();
                pi.enabled = false;
                enabled = false;
                return;
            }

            _cam = UnityEngine.Camera.main;
        }
        
        private void Update()
        {
            if (!IsOwner) return;

            _character.Rotate(GetMousePosition);
        }

        public void SetCharacter(CharacterController newCharacter)
        {
            _character = newCharacter;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!IsOwner || _character == null) return;

            var input = context.ReadValue<Vector2>();
            _character.Move(input);
        }

        public void OnSkill1(InputAction.CallbackContext context)
        {
            if (!IsOwner || _character == null) return;
            if (!context.performed) return;

            var dir = GetMouseDirection();
            _character.TryUseSkill(0, GetMousePosition, dir);
        }

        public void OnSkill2(InputAction.CallbackContext context)
        {
            if (!IsOwner || _character == null) return;
            if (!context.performed) return;

            var dir = GetMouseDirection();
            _character.TryUseSkill(1, GetMousePosition, dir);
        }

        public void OnSkill3(InputAction.CallbackContext context)
        {
            if (!IsOwner || _character == null) return;
            if (!context.performed) return;

            var dir = GetMouseDirection();
            _character.TryUseSkill(2, GetMousePosition, dir);
        }

        private Vector3 GetMousePosition
        {
            get
            {
                var ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var plane = new Plane(Vector3.up, _character.transform.position);

                if (!plane.Raycast(ray, out var dist)) return _character.transform.forward;
            
                var hit = ray.GetPoint(dist);

                return hit;
            }
        }

        private Vector3 GetMouseDirection()
        {
            var ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            var plane = new Plane(Vector3.up, _character.transform.position);

            if (!plane.Raycast(ray, out var dist)) return _character.transform.forward;
            
            var hit = ray.GetPoint(dist);
            
            return (hit - _character.transform.position).normalized;

        }
    }
}