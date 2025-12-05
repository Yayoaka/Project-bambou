using System.Collections.Generic;
using Health;
using Interfaces;
using Skills;
using Stats;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool destroyOnHit = true;

    private Vector3 _direction;
    private float _timer;

    private EffectData _data;
    private IStatsEntity _sourceStats;
    private IAffectable _sourceEntity;

    // Pour éviter les doublons
    private readonly HashSet<IAffectable> _alreadyHit = new();

    // ----------------------------------------------------------------------

    public void Init(EffectData data, IStatsEntity sourceStats, IAffectable sourceEntity, Vector3 direction)
    {
        if (!IsServer) return;

        _data = data;
        _sourceStats = sourceStats;
        _sourceEntity = sourceEntity;

        _direction = direction.normalized;
    }

    // ----------------------------------------------------------------------

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;

        // Move
        transform.position += _direction * speed * Time.deltaTime;

        // Lifetime
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    // ----------------------------------------------------------------------
    //  COLLISION
    // ----------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (!other.TryGetComponent(out IAffectable target))
            return;

        // Déjà touché → ignorer
        if (_alreadyHit.Contains(target))
            return;

        _alreadyHit.Add(target);

        // Calcul des dégâts
        var isCrit = _sourceStats.ComputeCrit(_data.effectType);

        var eventData = new HealthEventData()
        {
            Amount = _sourceStats.ComputeStat(_data.healthModification, isCrit),
            Critical = isCrit,
            Source = _sourceEntity,
            Type = _data.effectType
        };

        target.Damage(eventData);

        // Option : détruire au premier hit ?
        if (destroyOnHit)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
