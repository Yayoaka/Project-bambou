using Entity;
using UnityEngine;

namespace Character
{
    public class CharacterAnimationController : EntityComponent<CharacterBehaviour>
    {
        private Animator animator;
        private int _hashSpeed;
        private int _hashSkillTrigger;
        private Vector2 _lastMoveInput = Vector2.zero;

        public void GetAnimator()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void SetMoveInput(Vector2 move)
        {
            _lastMoveInput = move;
            float speed = move.magnitude;
            if (animator != null) animator.SetFloat(_hashSpeed, speed, 0.05f, Time.deltaTime);
        }

        public void TriggerSkill(int index)
        {
            if (animator != null) animator.SetTrigger("Spell" + index);
        }
    }
}