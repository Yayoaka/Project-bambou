using UnityEngine;

namespace Character.State
{
    public class CharacterState : CharacterComponent
    {
        public bool IsStunned { get; private set; }
        public bool IsRooted { get; private set; }
        public bool IsSilenced { get; private set; }
        public bool IsDead { get; private set; }

        private float _stunTimer;
        private float _rootTimer;
        private float _silenceTimer;

        private void Update()
        {
            UpdateStun();
            UpdateRoot();
            UpdateSilence();
        }

        public void ApplyStun(float duration)
        {
            IsStunned = true;
            _stunTimer = Mathf.Max(_stunTimer, duration);
        }

        public void ApplyRoot(float duration)
        {
            IsRooted = true;
            _rootTimer = Mathf.Max(_rootTimer, duration);
        }

        public void ApplySilence(float duration)
        {
            IsSilenced = true;
            _silenceTimer = Mathf.Max(_silenceTimer, duration);
        }

        public void Kill()
        {
            IsDead = true;
        }

        private void UpdateStun()
        {
            if (!IsStunned) return;

            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
                IsStunned = false;
        }

        private void UpdateRoot()
        {
            if (!IsRooted) return;

            _rootTimer -= Time.deltaTime;
            if (_rootTimer <= 0f)
                IsRooted = false;
        }

        private void UpdateSilence()
        {
            if (!IsSilenced) return;

            _silenceTimer -= Time.deltaTime;
            if (_silenceTimer <= 0f)
                IsSilenced = false;
        }
    }
}