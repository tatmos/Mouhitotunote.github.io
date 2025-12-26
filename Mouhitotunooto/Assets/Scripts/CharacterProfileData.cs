using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    [Serializable]
    public class CharacterProfile
    {
        public int scenarioId;
        public string name;
        public string role;
        public string job;
        public string feature;
        public string featureDarkMode;
        public string quote;
        public string quoteDarkMode;
        public string relationshipWithVoice;
        public string bugDescription;
        public Color profileColor;
        public Color borderColor;
    }

    [CreateAssetMenu(fileName = "CharacterProfiles", menuName = "Novel Game/Character Profiles")]
    public class CharacterProfilesData : ScriptableObject
    {
        public List<CharacterProfile> profiles = new List<CharacterProfile>();

        public CharacterProfile GetProfile(int scenarioId)
        {
            return profiles.Find(p => p.scenarioId == scenarioId);
        }
    }
}

