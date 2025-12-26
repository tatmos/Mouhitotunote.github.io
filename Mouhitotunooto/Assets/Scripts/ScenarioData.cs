using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    [Serializable]
    public class Choice
    {
        public int id;
        public string text;
        public string preview;
    }

    [Serializable]
    public class Branch
    {
        public string text;
        public bool hasWord;
        public string epilogue;
        public string epilogue2;
        public string hint;
    }

    [Serializable]
    public class Scenario
    {
        public int id;
        public string title;
        public string setup;
        public List<Choice> choices = new List<Choice>();
        public Dictionary<int, Branch> branches = new Dictionary<int, Branch>();

        // UnityのInspectorでDictionaryを表示するためのヘルパー
        [SerializeField]
        private List<int> branchKeys = new List<int>();
        [SerializeField]
        private List<Branch> branchValues = new List<Branch>();

        public void SerializeBranches()
        {
            branchKeys.Clear();
            branchValues.Clear();
            foreach (var kvp in branches)
            {
                branchKeys.Add(kvp.Key);
                branchValues.Add(kvp.Value);
            }
        }

        public void DeserializeBranches()
        {
            branches.Clear();
            for (int i = 0; i < branchKeys.Count && i < branchValues.Count; i++)
            {
                branches[branchKeys[i]] = branchValues[i];
            }
        }
    }

    [CreateAssetMenu(fileName = "NovelGameData", menuName = "Novel Game/Game Data")]
    public class NovelGameData : ScriptableObject
    {
        public List<Scenario> scenarios = new List<Scenario>();

        private void OnEnable()
        {
            foreach (var scenario in scenarios)
            {
                scenario.DeserializeBranches();
            }
        }
    }
}


