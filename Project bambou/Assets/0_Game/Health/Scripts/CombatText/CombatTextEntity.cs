using System;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Health.CombatText
{
    public class CombatTextEntity : MonoBehaviour
    {
        [SerializeField] private CanvasGroup textCanvasGroup;
        [SerializeField] private TMPro.TMP_Text healthEventText;
        [SerializeField] private GameObject critGameObject;
        [SerializeField] private Image critImage;
        
        private Sequence _textSequence;
        
        public bool IsAvailable {get; private set;}
        
        public void Init(HealthEventData data)
        {
            IsAvailable = false;
            
            transform.position = data.HitPoint + Vector3.up * 0.5f;
            
            healthEventText.text = Mathf.Round(data.Amount).ToString(CultureInfo.InvariantCulture);
            healthEventText.color = data.Type switch
            {
                HealthEventType.Physical => Color.darkOrange,
                HealthEventType.Magical => Color.lightSlateBlue,
                HealthEventType.True => Color.azure,
                HealthEventType.Healing => Color.chartreuse,
                _ => Color.black
            };

            critGameObject.SetActive(data.Critical);
            critImage.color = data.Type switch
            {
                HealthEventType.Physical => Color.darkOrange,
                HealthEventType.Magical => Color.lightSlateBlue,
                HealthEventType.True => Color.azure,
                HealthEventType.Healing => Color.chartreuse,
                _ => Color.black
            };
            
            textCanvasGroup.alpha = 0;
            if (UnityEngine.Camera.main != null) transform.rotation = UnityEngine.Camera.main.transform.rotation;

            PlayTextAnimation();
        }

        private void PlayTextAnimation()
        {
            _textSequence?.Kill();
            
            _textSequence = DOTween.Sequence();

            _textSequence.AppendCallback(() => gameObject.SetActive(true));
            
            _textSequence.Append(textCanvasGroup.DOFade(1, 0.5f));

            _textSequence.AppendInterval(0.5f);
            
            _textSequence.Append(transform.DOMoveZ(transform.position.z + 5f, 0.5f));

            _textSequence.Insert(1, textCanvasGroup.DOFade(0, 0.5f));

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