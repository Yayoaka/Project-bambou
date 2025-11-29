using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterComponent : NetworkBehaviour
    {
        protected CharacterBehaviour CharacterBehaviour;
        
        public void Init(CharacterBehaviour characterBehaviour)
        {
            CharacterBehaviour = characterBehaviour;
        }
    }
}