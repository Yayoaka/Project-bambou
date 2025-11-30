using System;
using System.Globalization;
using Data;
using DG.Tweening;
using GameState.Data;
using Stats.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Health.CombatText
{
    public class CombatTextEntity : MonoBehaviour
    {
        [SerializeField] private CanvasGroup textCanvasGroup;
        [SerializeField] private TMPro.TMP_Text healthEventText;
        [SerializeField] private GameObject textContainer;
        
        private Sequence _textSequence;
        
        public bool IsAvailable {get; private set;}
        
        public void Init(HealthEventData data)
        {
            IsAvailable = false;
            
            transform.position = data.HitPoint + Vector3.up * 0.5f + new Vector3(Random.value, Random.value, Random.value);
            
            textContainer.transform.localScale = data.Critical ? Vector3.one * 0.9f : Vector3.one * 0.7f;
            
            var color = GetColor(data.Type);

            healthEventText.text = (data.Critical ? $"<sprite index=0 color=#{ColorUtility.ToHtmlStringRGBA(color)}>" : "") +
                                   Mathf.Round(data.Amount).ToString(CultureInfo.InvariantCulture);
            healthEventText.color = color;
            
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

        private Color GetColor(HealthEventType type)
        {
            var database = GameDatabase.Get<StatsDatabase>();
            return database.GetEvent(type).Color;
        }
    }
}