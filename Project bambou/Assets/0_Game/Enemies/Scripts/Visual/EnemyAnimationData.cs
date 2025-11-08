using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Enemies.Visual
{
    public struct EnemyAnimationState : IComponentData
    {
        public float Time;
        public float Speed;
    }
    
    public struct BoneName : IBufferElementData
    {
        public FixedString64Bytes Value;
    }
    
    public class EnemyAnimDriverRef : IComponentData
    {
        public GameObject RigPrefab;
        public GameObject Instance;
        public Transform[] DriverBones;
        public AnimationClip Clip;
        public float Speed = 1f;
        public Animator Animator;
    }
    
    public struct BoneEntity : IBufferElementData
    {
        public Entity Value;
    }

    public struct BindPose : IBufferElementData
    {
        public Unity.Mathematics.float4x4 Value;
    }

    public struct RootEntity : IComponentData
    {
        public Entity Value;
    }
}