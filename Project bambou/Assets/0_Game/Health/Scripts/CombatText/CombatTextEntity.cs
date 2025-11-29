using System;
using System.Globalization;
using DG.Tweening;
using UnityEngine;

namespace Health.CombatText
{
    public class CombatTextEntity : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text healthEventText;
        [SerializeField] private Canvas healthEventCanvas;
        
        private Sequence _textSequence;
        
        public bool IsAvailable {get; private set;}

        private void Awake()
        {
            healthEventCanvas.worldCamera = UnityEngine.Camera.main;
            healthEventCanvas.overrideSorting = true;
        }
        
        public void Init(HealthEventData data)
        {
            IsAvailable = false;
            
            transform.position = data.HitPoint + Vector3.up * 0.5f;
            
            healthEventText.text = data.Amount.ToString(CultureInfo.InvariantCulture);
            healthEventText.color = data.Type switch
            {
                HealthEventType.Physical => Color.brown,
                HealthEventType.Magical => Color.darkMagenta,
                HealthEventType.True => Color.azure,
                HealthEventType.Healing => Color.chartreuse,
                _ => Color.black
            };
            healthEventText.alpha = 0;
            if (UnityEngine.Camera.main != null) transform.rotation = UnityEngine.Camera.main.transform.rotation;

            PlayTextAnimation();
        }

        private void PlayTextAnimation()
        {
            _textSequence?.Kill();
            
            _textSequence = DOTween.Sequence();

            _textSequence.AppendCallback(() => gameObject.SetActive(true));
            
            _textSequence.Append(healthEventText.DOFade(1, 0.5f));

            _textSequence.AppendInterval(0.5f);
            
            _textSequence.Append(transform.DOMoveZ(transform.position.z + 5f, 0.5f));

            _textSequence.Insert(1, healthEventText.DOFade(0, 0.5f));

            _textSequence.onKill += ResetTextAnimation;
            
            _textSequence.Play();
        }

        private void ResetTextAnimation()
        {
            gameObject.SetActive(false);
            IsAvailable = true;
        }
    }
}