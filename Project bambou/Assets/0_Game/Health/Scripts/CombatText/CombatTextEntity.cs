using Data;
using DG.Tweening;
using Effect;
using Network;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Health.CombatText
{
    public class CombatTextEntity : NetworkBehaviour
    {
        [SerializeField] private CanvasGroup textCanvasGroup;
        [SerializeField] private TMPro.TMP_Text healthEventText;
        [SerializeField] private GameObject textContainer;
        
        private Sequence _textSequence;
        
        public void Init(HealthEventData data)
        {
            transform.position = data.HitPoint 
                                 + Vector3.up * 0.5f 
                                 + new Vector3(Random.value - 0.5f, Random.value * 0.3f, Random.value - 0.5f);

            textContainer.transform.localScale = data.Critical ? 
                Vector3.one * 1.1f : 
                Vector3.one * 0.8f;

            var color = GetColor(data.Type);

            string txt = Mathf.RoundToInt(data.Amount).ToString();

            if (data.Critical)
                txt = $"<sprite index=0 color=#{ColorUtility.ToHtmlStringRGBA(color)}>{txt}";

            healthEventText.text = txt;
            healthEventText.color = color;

            textCanvasGroup.alpha = 0;

            var cam = UnityEngine.Camera.main;
            if (cam != null)
                transform.rotation = cam.transform.rotation;

            PlayTextAnimation();
        }

        private void PlayTextAnimation()
        {
            _textSequence?.Kill(true);
            
            _textSequence = DOTween.Sequence();

            _textSequence.AppendCallback(() => gameObject.SetActive(true));
            
            _textSequence.Append(textCanvasGroup.DOFade(1, 0.5f));

            _textSequence.AppendInterval(0.5f);
            
            _textSequence.Append(transform.DOMoveZ(transform.position.z + 5f, 0.5f));

            _textSequence.Insert(1, textCanvasGroup.DOFade(0, 0.5f));

            _textSequence.onComplete += ResetTextAnimation;
            
            _textSequence.Play();
        }

        private void ResetTextAnimation()
        {
            NetworkObjectPool.Instance.Return(NetworkObject);
        }

        private Color GetColor(EffectType type)
        {
            var database = GameDatabase.Get<StatsDatabase>();
            return database.GetEvent(type).Color;
        }
    }
}