using Camera;
using Character.Data;
using Character.Input;
using Character.State;
using Character.Visual;
using Entity;
using Health;
using Interfaces;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterBehaviour : EntityBehaviour<CharacterBehaviour>
    {
        [SerializeField] private CharacterData data;

        [Header("Modules")]
        public CharacterMovementController Movement { get; private set; }
        public CharacterAnimationController AnimationController { get; private set; }
        public CharacterSkills Skills { get; private set; }
        public CharacterInputController InputController { get; private set; }
        public CharacterState State { get; private set; }
        public CharacterVisual Visual { get; private set; }
        public IStatsComponent Stats { get; private set; }

        private void Awake()
        {
            // Initialise tous les modules
            Movement = InitComponent<CharacterMovementController>();
            AnimationController = InitComponent<CharacterAnimationController>();
            Skills = InitComponent<CharacterSkills>();
            InputController = InitComponent<CharacterInputController>();
            State = InitComponent<CharacterState>();
            Stats = GetComponent<IStatsComponent>();
            
            Movement.LateInit();
            AnimationController.LateInit();
            Skills.LateInit();
            InputController.LateInit();
            State.LateInit();
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
            AnimationController.GetAnimator();
        }

        private void SetData()
        {
            Skills.SetSpells(data.Spells);
            Skills.SetAnimationController(AnimationController);
            Stats.SetStats(data.Stats);
        }
        
        public void Move(Vector2 input)
        {
            if (!IsOwner) return;    
            if (State.IsStunned) return;

            Movement.Move(input);
        }

        public void Rotate(Vector3 input)
        {
            if (!IsOwner) return;
            if (State.IsStunned) return;
            
            Movement.RotateToMouse(input);
        }

        public void TryUseSkill(int index, Vector3 mousePosition, Vector3 direction)
        {
            if (!IsOwner) return;
            if (State.IsStunned) return;

            Skills.TryCast(index, mousePosition, direction);
        }
    }
}
