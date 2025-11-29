using Camera;
using Character.Data;
using Character.Input;
using Character.State;
using Character.Visual;
using Health;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterBehaviour : NetworkBehaviour, IAffectable
    {
        [SerializeField] private CharacterData data;

        [Header("Modules")]
        public CharacterMovementController Movement { get; private set; }
        public CharacterAnimationController AnimationController { get; private set; }
        public CharacterSkills Skills { get; private set; }
        public CharacterInputController InputController { get; private set; }
        public CharacterState State { get; private set; }
        public CharacterVisual Visual { get; private set; }
        public CharacterHealth Health { get; private set; }
        public CharacterStats Stats { get; private set; }

        private void Awake()
        {
            Movement = GetComponent<CharacterMovementController>();
            Skills = GetComponent<CharacterSkills>();
            InputController = GetComponent<CharacterInputController>();
            State = GetComponent<CharacterState>();
            Health = GetComponent<CharacterHealth>();
            Stats = GetComponent<CharacterStats>();
            
            Movement.Init(this);
            Skills.Init(this);
            InputController.Init(this);
            State.Init(this);
            Health.Init(this);
            Stats.Init(this);
            Stats.SetStats(data.Stats);
        }

        public override void OnNetworkSpawn()
        {
            SpawnVisual();
            SetData();
            
            if (!IsOwner)
            {
                if (InputController != null)
                    InputController.enabled = false;
                
                return;
            }
            
            InputController.SetCharacter(this);
            CameraManager.SetTarget(transform);
        }
        
        private void SpawnVisual()
        {
            if (Visual != null) return;

            var prefab = data.CharacterVisualPrefab;
            if (prefab == null)
            {
                Debug.LogError("CharacterVisualPrefab is missing in CharacterData!");
                return;
            }

            Visual = Instantiate(prefab, transform);
            AnimationController = Visual.GetComponent<CharacterAnimationController>();
            AnimationController.Init(this);
        }

        private void SetData()
        {
            Skills.SetSpells(data.Spells);
            Skills.SetAnimationController(AnimationController);
        }
        
        public void Move(Vector2 input)
        {
            if (!IsOwner) return;

            if (State.IsStunned)
                return;

            Movement.Move(input);
        }

        public void Rotate(Vector3 input)
        {
            if (!IsOwner) return;
            
            if (State.IsStunned)
                return;
            
            Movement.RotateToMouse(input);
        }

        public void TryUseSkill(int index, Vector3 mousePosition, Vector3 direction)
        {
            if (!IsOwner) return;

            if (State.IsStunned) return;

            Skills.TryCast(index, mousePosition, direction);
        }

        public void Damage(HealthEventData healthEventData)
        {
            Health.ApplyDamage(healthEventData);
        }
    }
}